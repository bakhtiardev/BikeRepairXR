using UnityEngine;

/// <summary>
/// Two-hand wheel removal — enabled by <see cref="BoltLooseningInteraction"/> once the bolt is loose.
///
/// INTERACTION FLOW
///   1. Both controllers must be within <see cref="grabRadius"/> of the wheel.
///   2. Player presses both INDEX triggers simultaneously to grab.
///   3. The wheel follows the midpoint of the two hands.
///   4. Once pulled more than <see cref="detachDistance"/> from its start, the wheel detaches from the bike.
///   5. While one trigger stays held the hold is maintained (releasing one hand is safe).
///   6. Releasing BOTH triggers before detach snaps the wheel back to its mount.
///   7. After detach the player can release, re-grab, release, re-grab as many times as needed.
///   8. The step completes only when the detached wheel is resting within
///      <see cref="carpetPlaceRadius"/> of <see cref="carpetTarget"/> (Carpet_Interact).
///
/// SCENE SETUP
///   1. Attach this script to the back wheel root GameObject.
///   2. Leave the component DISABLED — BoltLooseningInteraction enables it at the right time.
///   3. Assign the Carpet_Interact Transform to <see cref="carpetTarget"/>.
///   4. Tune <see cref="grabRadius"/>, <see cref="detachDistance"/>, and <see cref="carpetPlaceRadius"/>.
/// </summary>
[DisallowMultipleComponent]
public class WheelTwoHandGrab : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Tooltip("How close (metres) each hand must be to the wheel centre to initiate the grab.")]
    public float grabRadius = 0.35f;

    [Tooltip("How far (metres) the wheel must be moved from its start position before it fully detaches.")]
    public float detachDistance = 0.4f;

    [Tooltip("The CounterCanvas GameObject — hidden automatically once the wheel is fully detached.")]
    public GameObject counterCanvas;

    [Tooltip("The Carpet_Interact Transform — the detached wheel must be resting here to complete the step.")]
    public Transform carpetTarget;

    [Tooltip("How close (metres) the wheel centre must be to carpetTarget to count as placed.")]
    public float carpetPlaceRadius = 0.6f;

    // ── State ─────────────────────────────────────────────────────────────────
    private Transform _leftController;
    private Transform _rightController;
    private Rigidbody _rb;

    private Vector3    _startWorldPos;
    private Transform  _originalParent;
    private Vector3    _startLocalPos;
    private Quaternion _startLocalRot;
    private Vector3    _prevMidpoint;

    private bool _bothHeld  = false;
    private bool _detached  = false;

    // ── Unity ─────────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        // Disable every Grabbable (ISDK) on this wheel so the ISDK grabber doesn't
        // intercept index-trigger presses and fight with our manual position code.
        foreach (var b in GetComponents<Behaviour>())
        {
            if (b != this && b.GetType().Name == "Grabbable")
                b.enabled = false;
        }

        // Cache references each time the component is (re-)enabled
        _rb = GetComponent<Rigidbody>();
        // If the back wheel child has no Rigidbody of its own, add one so we can
        // manipulate kinematic state independently of the bike root.
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity   = false;
            _rb.isKinematic  = true;
        }

        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            _leftController  = rig.leftControllerAnchor;
            _rightController = rig.rightControllerAnchor;
        }

        _startWorldPos  = transform.position;
        _originalParent = transform.parent;
        _startLocalPos  = transform.localPosition;
        _startLocalRot  = transform.localRotation;
        _bothHeld       = false;
        _detached       = false;
    }

    private void Update()
    {
        bool leftIndex  = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        bool rightIndex = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (!_bothHeld)
        {
            // To START the grab: both hands must be near the wheel AND both triggers held.
            bool leftNear  = _leftController  != null &&
                             Vector3.Distance(_leftController.position,  transform.position) <= grabRadius;
            bool rightNear = _rightController != null &&
                             Vector3.Distance(_rightController.position, transform.position) <= grabRadius;

            if (leftNear && rightNear && leftIndex && rightIndex)
                BeginTwoHandHold();
        }
        else
        {
            // To MAINTAIN the grab: at least one trigger must stay held.
            // This lets the player accidentally release one hand without losing the wheel.
            // Releasing BOTH triggers ends the hold.
            if (!leftIndex && !rightIndex)
                ReleaseHold();
        }

        if (_bothHeld)
        {
            // Move wheel with the midpoint of both hands
            Vector3 mid   = Midpoint();
            Vector3 delta = mid - _prevMidpoint;
            transform.position += delta;
            _prevMidpoint = mid;

            // Detach once far enough from original position (only check before detach)
            if (!_detached && Vector3.Distance(transform.position, _startWorldPos) >= detachDistance)
                Detach();
        }

        // While the detached wheel is at rest (not held), continuously check whether
        // it has been placed close enough to the carpet to complete the step.
        if (_detached && !_bothHeld && carpetTarget != null &&
            Vector3.Distance(transform.position, carpetTarget.position) <= carpetPlaceRadius)
        {
            FinalizeOnCarpet();
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    private void BeginTwoHandHold()
    {
        _bothHeld = true;
        if (_rb != null) _rb.isKinematic = true;

        // Un-parent from bike so the wheel can move independently
        transform.SetParent(null, worldPositionStays: true);
        _prevMidpoint = Midpoint();
    }

    private void ReleaseHold()
    {
        _bothHeld = false;

        if (!_detached)
        {
            // Not yet detached — snap back to mount so the player can try again.
            transform.SetParent(_originalParent, worldPositionStays: false);
            transform.localPosition = _startLocalPos;
            transform.localRotation = _startLocalRot;
            _startWorldPos = transform.position;

            if (_rb != null)
            {
                _rb.isKinematic     = true;
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }
        // Already detached: the wheel simply stays where it is in world space.
        // The script remains active so the player can re-grab and keep moving it.
        // Completion is handled by the carpet proximity check in Update.
    }

    private void Detach()
    {
        _detached = true;

        // Un-parent from bike (may already be null from BeginTwoHandHold)
        transform.SetParent(null, worldPositionStays: true);

        if (counterCanvas != null)
            counterCanvas.SetActive(false);

        Debug.Log("[WheelTwoHandGrab] Back wheel detached — carry it to the carpet.");
        // Player keeps holding; do NOT freeze or disable here.
    }

    private void FinalizeOnCarpet()
    {
        // Freeze the wheel in place over the carpet and end the interaction.
        if (_rb != null)
        {
            _rb.isKinematic     = true;
            _rb.useGravity      = false;
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("[WheelTwoHandGrab] Wheel placed on carpet — step complete!");
        enabled = false;
    }

    private Vector3 Midpoint() =>
        (_leftController.position + _rightController.position) * 0.5f;

    // ── Editor Gizmo ──────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
