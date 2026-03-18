// Copyright (c) Meta Platforms, Inc. and affiliates.

using TMPro;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace MRBike
{
    public class TransformTarget : MonoBehaviour
    {
        [SerializeField] private GameObject m_grabbedObject;

        [Tooltip("Distance at which the part snaps and the step completes (while held or not).")]
        [SerializeField] private float m_thresholdDistance = 0.1f;

        [Tooltip("Distance at which magnetic attraction begins pulling the part toward the target. " +
                 "Set to 0 to disable. Should be larger than thresholdDistance.")]
        [SerializeField] private float m_attractionRadius = 0.25f;

        [Tooltip("How quickly the part is pulled toward the target inside the attraction radius (0–1 per frame lerp speed).")]
        [SerializeField] private float m_attractionStrength = 0.2f;

        [SerializeField] private float m_thresholdAngle = 180;
        [SerializeField] private float m_offset = 0;
        [SerializeField] private bool m_removeGrabbableOnComplete = false;

        [Header("Prerequisite")]
        [Tooltip("If set, this target only becomes active once the prerequisite TransformTarget has completed. " +
                 "Drag Wheel_Target here on Axle_Target so the axle can only snap after the front wheel is placed.")]
        [SerializeField] private TransformTarget m_prerequisite;

        [Header("Snap Feedback")]
        [Tooltip("Animator to play when this part snaps into place (e.g. Axle Animated.controller).")]
        [SerializeField] private Animator m_snapAnimator;

        [Tooltip("AudioClip to play when this part snaps into place.")]
        [SerializeField] private AudioClip m_snapAudioClip;

        [SerializeField] private TMP_Text m_debugText;

        [Header("Assembly Prefab")]
        [Tooltip("Prefab or GameObject to instantiate/enable when this part snaps. " +
                 "For seat assembly, drag the 3-SeatAdjustable GameObject here to show it after snapping.")]
        [SerializeField] private GameObject m_assemblyPrefabToShow;

        [Tooltip("If true, instantiate the prefab at the target location. " +
                 "If false, just enable it (assumes it's already in the scene).")]
        [SerializeField] private bool m_instantiatePrefab = false;

        [Header("Pre-Snap Visibility")]
        [Tooltip("Objects that should stay visible while this target has not been completed.")]
        [SerializeField] private GameObject[] m_showWhileIncomplete;

        [Tooltip("Objects that should stay hidden while this target has not been completed.")]
        [SerializeField] private GameObject[] m_hideWhileIncomplete;

        [Tooltip("If enabled, objects in Show While Incomplete are hidden once this target completes.")]
        [SerializeField] private bool m_hideShownObjectsOnComplete = true;

        public UnityEvent OnComplete;

        private bool m_completed = false;
        private bool m_snapped   = false;   // enforce position in LateUpdate
        private int  m_snapFrames = 0;      // how many frames to keep enforcing

        /// <summary>True once this target has been completed (used by prerequisite checks on other targets).</summary>
        public bool IsCompleted => m_completed;

        public GameObject GrabbedObject
        {
            set => m_grabbedObject = value;
        }

        private void Update()
        {
            EnforceIncompleteVisibility();

            if (m_completed || m_grabbedObject == null) return;

            // Block snap and attraction until the prerequisite part is placed.
            if (m_prerequisite != null && !m_prerequisite.IsCompleted) return;

            float dist  = Vector3.Distance(transform.position, m_grabbedObject.transform.position) - m_offset;
            float angle = Vector3.Angle(m_grabbedObject.transform.up, transform.up);

            if (m_debugText != null)
                m_debugText.text = dist.ToString("F2");

            // ── Snap zone: complete immediately, even while held ───────────────
            if (dist < m_thresholdDistance && angle < m_thresholdAngle)
            {
                SetOnTarget();
                return;
            }

            // ── Attraction zone: magnetically pull the part toward the target ──
            // This works even while the Grabbable transformer is active because we
            // directly move the transform; the attraction is intentionally gentle
            // so it feels like a "pull" rather than a teleport.
            if (m_attractionRadius > 0f && dist < m_attractionRadius)
            {
                m_grabbedObject.transform.position = Vector3.Lerp(
                    m_grabbedObject.transform.position,
                    transform.position,
                    m_attractionStrength);
            }
        }

        private void LateUpdate()
        {
            // After snapping, keep enforcing the target pose for several frames so
            // any still-running ISDK transformer or TransformReset.ReturnHome coroutine
            // cannot override the locked position.
            if (!m_snapped || m_grabbedObject == null) return;

            m_grabbedObject.transform.position = transform.position;
            m_grabbedObject.transform.rotation = transform.rotation;

            if (--m_snapFrames <= 0)
                m_snapped = false;
        }

        private void OnEnable()
        {
            EnforceIncompleteVisibility();
        }

        private void EnforceIncompleteVisibility()
        {
            if (m_completed) return;

            if (m_showWhileIncomplete != null)
            {
                foreach (var go in m_showWhileIncomplete)
                {
                    if (go != null && !go.activeSelf)
                        go.SetActive(true);
                }
            }

            if (m_hideWhileIncomplete != null)
            {
                foreach (var go in m_hideWhileIncomplete)
                {
                    if (go != null && go.activeSelf)
                        go.SetActive(false);
                }
            }
        }

        public void SetOnTarget()
        {
            if (m_completed) return;
            m_completed = true;

            if (m_grabbedObject != null)
            {
                // 1. Disable TransformReset FIRST so that when Grabbable is disabled
                //    (which fires WhenRelease → TransformReset.Released), the
                //    ReturnHome coroutine cannot start and undo our snap.
                var tr = m_grabbedObject.GetComponent<TransformReset>();
                if (tr != null) tr.enabled = false;

                // 2. Force-release any active ISDK grab so the transformer stops
                //    fighting our position assignment.
                var grabbable = m_grabbedObject.GetComponent<Grabbable>();
                if (grabbable != null) grabbable.enabled = false;

                // 3. Snap part to exact target pose.
                m_grabbedObject.transform.position = transform.position;
                m_grabbedObject.transform.rotation = transform.rotation;

                // 4. Freeze physics so the part stays locked at the target.
                var rb = m_grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic     = true;
                    rb.linearVelocity  = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // 5. Keep enforcing position for 10 frames via LateUpdate to beat
                //    any still-running coroutines or late transformers.
                m_snapped    = true;
                m_snapFrames = 10;

                // 6. Activate (it may be hidden by default) and play snap animation.
                if (m_snapAnimator != null)
                {
                    m_snapAnimator.gameObject.SetActive(true);
                    m_snapAnimator.enabled = true;
                    m_snapAnimator.Play(0, 0, 0f);
                }

                // 7. Play snap sound via PlayClipAtPoint — static call spawns its own
                //    temp AudioSource, so it survives this GameObject being deactivated.
                if (m_snapAudioClip != null)
                    AudioSource.PlayClipAtPoint(m_snapAudioClip, transform.position);

                if (m_removeGrabbableOnComplete)
                    m_grabbedObject.SetActive(false);

                // 8. Show or instantiate the assembly prefab (e.g. 3-SeatAdjustable)
                //    This makes the assembled part appear to complete the assembly feel.
                if (m_assemblyPrefabToShow != null)
                {
                    if (m_instantiatePrefab)
                    {
                        Instantiate(m_assemblyPrefabToShow, transform.position, transform.rotation);
                    }
                    else
                    {
                        m_assemblyPrefabToShow.SetActive(true);
                        
                        // Play seat adjustment audio if SeatAdjustementUpdater is present
                        var seatAdjuster = m_assemblyPrefabToShow.GetComponentInChildren<SeatAdjustementUpdater>(includeInactive: true);
                        if (seatAdjuster != null)
                        {
                            seatAdjuster.PlaySnapAudio();
                        }
                    }
                }

                if (m_hideShownObjectsOnComplete && m_showWhileIncomplete != null)
                {
                    foreach (var go in m_showWhileIncomplete)
                    {
                        if (go != null && go.activeSelf)
                            go.SetActive(false);
                    }
                }
            }

            gameObject.SetActive(false);

            if (m_debugText != null)
                m_debugText.text = "Contact";

            OnComplete?.Invoke();
        }
    }
}
