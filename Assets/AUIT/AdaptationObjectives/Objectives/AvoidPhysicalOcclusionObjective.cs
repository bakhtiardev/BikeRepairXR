using AUIT.AdaptationObjectives.Definitions;
using AUIT.AdaptationObjectives.Extras;
using System.Collections.Generic;
using UnityEngine;

namespace AUIT.AdaptationObjectives
{
    public class AvoidPhysicalOcclusionObjective : LocalObjective
    {
        [SerializeField]
        private ContextSource<Transform> userContextSource;

        [SerializeField]
        private LayerMask physicalLayerMask;

        [SerializeField]
        private float castRadius = 0.05f;

        [SerializeField]
        private float moveStep = 0.25f;

        public override ObjectiveType ObjectiveType => ObjectiveType.AvoidOcclusion;

        private RectTransform rectTransform;

        private new void OnEnable()
        {
            base.OnEnable();

            if (userContextSource == null)
                userContextSource = GetUserPoseContextSource();

            rectTransform = GetComponent<RectTransform>();
        }

        private Vector3[] GetCheckPoints(Layout layout)
        {
            float width = 1f;
            float height = 1f;

            if (rectTransform != null)
            {
                // World-space canvas: rect width/height scaled into world units
                width = rectTransform.rect.width * transform.lossyScale.x;
                height = rectTransform.rect.height * transform.lossyScale.y;
            }

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            // Center + 4 corners + edge midpoints
            Vector3[] localPoints = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),                  // center
                new Vector3(-halfW, -halfH, 0f),         // bottom-left
                new Vector3(-halfW,  halfH, 0f),         // top-left
                new Vector3( halfW, -halfH, 0f),         // bottom-right
                new Vector3( halfW,  halfH, 0f),         // top-right
                new Vector3(0f, -halfH, 0f),             // bottom
                new Vector3(0f,  halfH, 0f),             // top
                new Vector3(-halfW, 0f, 0f),             // left
                new Vector3( halfW, 0f, 0f),             // right
            };

            Matrix4x4 trs = Matrix4x4.TRS(layout.Position, layout.Rotation, Vector3.one);

            Vector3[] worldPoints = new Vector3[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
                worldPoints[i] = trs.MultiplyPoint(localPoints[i]);

            return worldPoints;
        }

        private bool IsOccluding(Vector3 targetPoint, out RaycastHit nearestHit)
        {
            nearestHit = default;

            Transform user = userContextSource.GetValue();
            Vector3 origin = user.position;
            Vector3 toTarget = targetPoint - origin;
            float distance = toTarget.magnitude;

            if (distance <= 0.0001f)
                return false;

            Vector3 direction = toTarget / distance;

            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                castRadius,
                direction,
                distance,
                physicalLayerMask,
                QueryTriggerInteraction.Ignore
            );

            float nearestDistance = float.MaxValue;
            bool found = false;

            foreach (var hit in hits)
            {
                if (hit.transform == null)
                    continue;

                // Ignore self / children
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestHit = hit;
                    found = true;
                }
            }

            return found;
        }

        public override float CostFunction(Layout optimizationTarget, Layout initialLayout = null)
        {
            if (userContextSource == null)
            {
                Debug.LogError("AvoidPhysicalOcclusionObjective: User context source is not set.");
                return 0f;
            }

            Vector3[] checkPoints = GetCheckPoints(optimizationTarget);

            foreach (Vector3 point in checkPoints)
            {
                if (IsOccluding(point, out _))
                    return 1f;
            }

            return 0f;
        }

        public override Layout OptimizationRule(Layout optimizationTarget, Layout initialLayout = null)
        {
            Layout result = optimizationTarget.Clone();

            if (userContextSource == null)
            {
                Debug.LogError("AvoidPhysicalOcclusionObjective: User context source is not set.");
                return result;
            }

            Vector3[] checkPoints = GetCheckPoints(optimizationTarget);

            Vector3 accumulatedPush = Vector3.zero;
            int hitCount = 0;

            foreach (Vector3 point in checkPoints)
            {
                if (IsOccluding(point, out RaycastHit hit))
                {
                    // Push away from the hit surface normal
                    accumulatedPush += hit.normal;
                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                Vector3 moveDirection = (accumulatedPush / hitCount).normalized;
                if (moveDirection.sqrMagnitude > 0.0001f)
                {
                    result.Position += moveStep * moveDirection;
                }
            }

            return result;
        }

        public override Layout DirectRule(Layout optimizationTarget)
        {
            return optimizationTarget;
        }

        public override float[] GetParameters()
        {
            return new float[] { weight };
        }

        public override void SetParameters(float[] parameters)
        {
            weight = parameters[0];
        }
    }
}