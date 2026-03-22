using Oculus.Interaction;
using Oculus.Interaction.Locomotion;
using UnityEngine;

/// <summary>
/// Bridges Meta's TeleportInteractable hotspots with the OVRCameraRig so selecting a hotspot
/// actually moves the player rig to the requested destination.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TeleportInteractable))]
public class TeleportHotspotHandler : MonoBehaviour
{
    [Tooltip("Optional override. If left empty the first OVRCameraRig in the scene is used.")]
    [SerializeField] private OVRCameraRig cameraRigOverride;

    private TeleportInteractable _teleportInteractable;
    private OVRCameraRig _cameraRig;
    private Transform _headAnchor;

    private void Awake()
    {
        _teleportInteractable = GetComponent<TeleportInteractable>();
    }

    private void OnEnable()
    {
        EnsureRigReferences();
        if (_teleportInteractable != null)
        {
            _teleportInteractable.WhenSelectingInteractorAdded.Action += HandleTeleportRequest;
        }
    }

    private void OnDisable()
    {
        if (_teleportInteractable != null)
        {
            _teleportInteractable.WhenSelectingInteractorAdded.Action -= HandleTeleportRequest;
        }
    }

    private void HandleTeleportRequest(TeleportInteractor _)
    {
        if (_teleportInteractable == null)
        {
            return;
        }

        EnsureRigReferences();
        if (_cameraRig == null || _headAnchor == null)
        {
            return;
        }

        Pose hitPose = new Pose(transform.position, transform.rotation);
        Pose targetPose = _teleportInteractable.TargetPose(hitPose);

        Vector3 headOffset = _headAnchor.position - _cameraRig.transform.position;
        Vector3 horizontalOffset = new Vector3(headOffset.x, 0f, headOffset.z);

        Vector3 desiredRigPosition = targetPose.position - horizontalOffset;
        if (_teleportInteractable.EyeLevel)
        {
            desiredRigPosition.y = targetPose.position.y - headOffset.y;
        }
        else
        {
            desiredRigPosition.y = targetPose.position.y;
        }

        _cameraRig.transform.position = desiredRigPosition;

        if (_teleportInteractable.FaceTargetDirection)
        {
            float targetYaw = targetPose.rotation.eulerAngles.y;
            Vector3 currentEuler = _cameraRig.transform.rotation.eulerAngles;
            _cameraRig.transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        }
    }

    private void EnsureRigReferences()
    {
        if (_cameraRig == null)
        {
            _cameraRig = cameraRigOverride != null
                ? cameraRigOverride
                : FindFirstObjectByType<OVRCameraRig>(FindObjectsInactive.Exclude);

            if (_cameraRig == null)
            {
                Debug.LogError("[TeleportHotspotHandler] No OVRCameraRig found in scene.", this);
                return;
            }
        }

        if (_headAnchor == null)
        {
            _headAnchor = _cameraRig.centerEyeAnchor != null
                ? _cameraRig.centerEyeAnchor
                : _cameraRig.GetComponentInChildren<Camera>()?.transform;

            if (_headAnchor == null)
            {
                Debug.LogError("[TeleportHotspotHandler] No head anchor found under OVRCameraRig.", this);
            }
        }
    }
}
