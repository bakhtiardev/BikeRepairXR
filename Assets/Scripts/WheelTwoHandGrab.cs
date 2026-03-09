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
    public float carpetPlaceRadius = 2.0f;

    [Tooltip("Canvas shown when the wheel is placed on the carpet (Level Completed message).")]
    public GameObject levelCompletedCanvas;
    [Header("Horizontal Landing")]
    [Tooltip("World-space Euler angles the wheel snaps to when it lands on the floor or carpet (lying flat). " +
             "Adjust to match your model — default assumes the wheel's spin axis is local-X.")]
    public Vector3 horizontalEulers = new Vector3(0f, 0f, 90f);
    // ── State ─────────────────────────────────────────────────────────────────
    private Transform _leftController;
    private Transform _rightController;
    private Rigidbody _rb;

    private Vector3    _startWorldPos;
    private Transform  _originalParent;
    private Vector3    _startLocalPos;
    private Quaternion _startLocalRot;
    private Vector3    _prevMidpoint;

    private bool _bothHeld   = false;
    private bool _detached   = false;
    private bool _finalized  = false;

    /// <summary>True once the wheel has been fully pulled off the bike.</summary>
    public bool IsDetached => _detached;

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
        _finalized      = false;
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
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }

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
        else
        {
            // Already detached — enable gravity so the wheel falls to the floor.
            // ContinuousDynamic prevents the wheel from tunnelling through the thin
            // floor trigger box at high fall speeds.
            if (_rb != null)
            {
                _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                _rb.isKinematic = false;
                _rb.useGravity  = true;
            }
        }
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
        if (_finalized) return;
        _finalized = true;

        // Freeze the wheel in place over the carpet and end the interaction.
        if (_rb != null)
        {
            _rb.isKinematic     = true;
            _rb.useGravity      = false;
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // Snap wheel to carpet centre so it sits cleanly, lying flat
        transform.position = carpetTarget.position + Vector3.up * 0.05f;
        transform.rotation = Quaternion.Euler(horizontalEulers);

        // Disable all Behaviour components on the wheel and its children
        // (catches Grabbable, ISDK interactables, etc. on any child)
        foreach (var b in GetComponentsInChildren<Behaviour>(includeInactive: true))
        {
            if (b != this)
                b.enabled = false;
        }

        // Show the level-completion canvas
        if (levelCompletedCanvas != null)
            levelCompletedCanvas.SetActive(true);
        else
            Debug.LogWarning("[WheelTwoHandGrab] levelCompletedCanvas is not assigned!");

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
