using UnityEngine;

/// <summary>
/// Allows picking up the wrench with the GRIP (side) trigger on either controller.
/// The player's controller must be within <see cref="pickupRange"/> of the wrench to grab it,
/// which naturally enforces proximity to the table.
/// The INDEX (front) trigger is untouched — free for task interactions.
/// Drop by releasing the grip trigger.
/// </summary>
public class WrenchProximityGrab : MonoBehaviour
{
    [Tooltip("How close (metres) a controller must be to the wrench to pick it up.")]
    public float pickupRange = 0.4f;

    [Tooltip("Optional child transform at the short Allen-key tip. Used by BoltLooseningInteraction for precise zone detection. Leave empty to fall back to wrench root + tipOffset.")]
    public Transform wrenchTip;

    // ── Public state for other scripts ───────────────────────────────────────
    /// <summary>True while the wrench is being held (grip trigger held).</summary>
    public bool IsHeld => _heldBy != null;

    /// <summary>Which OVR controller is currently holding the wrench (None if not held).</summary>
    public OVRInput.Controller HoldingController => _holdingController;

    // ── Private fields ────────────────────────────────────────────────────────
    private Transform _leftController;
    private Transform _rightController;
    private Transform _heldBy = null;
    private OVRInput.Controller _holdingController = OVRInput.Controller.None;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        // Start kinematic so wrench rests on the table without physics.
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }

        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            _leftController  = rig.leftControllerAnchor;
            _rightController = rig.rightControllerAnchor;
        }
    }

    private void Update()
    {
        if (_heldBy == null)
        {
            // Try grab with left grip
            TryGrab(OVRInput.Controller.LTouch, _leftController);
            // Try grab with right grip
            TryGrab(OVRInput.Controller.RTouch, _rightController);
        }
        else
        {
            // Follow the holding hand
            transform.position = _heldBy.position;
            transform.rotation = _heldBy.rotation;

            // Release when grip is let go
            bool leftRelease  = _heldBy == _leftController  &&
                                OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
            bool rightRelease = _heldBy == _rightController &&
                                OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

            if (leftRelease || rightRelease)
                Drop();
        }
    }

    private void TryGrab(OVRInput.Controller controller, Transform hand)
    {
        if (hand == null) return;
        if (!OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller)) return;

        float dist = Vector3.Distance(hand.position, transform.position);
        if (dist <= pickupRange)
            Grab(hand, controller);
    }

    private void Grab(Transform hand, OVRInput.Controller controller)
    {
        _heldBy = hand;
        _holdingController = controller;
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }
    }

    private void Drop()
    {
        _heldBy = null;
        _holdingController = OVRInput.Controller.None;
        // Enable gravity so the wrench falls to the floor.
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity  = true;
        }
    }
}
