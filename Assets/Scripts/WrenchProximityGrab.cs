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

    private Transform _leftController;
    private Transform _rightController;
    private Transform _heldBy = null;
    private Rigidbody _rb;
    private WrenchTool wrenchTool;
    public WrenchCollisionHelper collisionHelper;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        wrenchTool = GetComponent<WrenchTool>();
        collisionHelper = GetComponent<WrenchCollisionHelper>();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

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
            TryGrab(OVRInput.Controller.LTouch, _leftController);
            TryGrab(OVRInput.Controller.RTouch, _rightController);
        }
        else
        {
            transform.position = _heldBy.position;
            transform.rotation = _heldBy.rotation;

            bool leftRelease = _heldBy == _leftController &&
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
            Grab(hand);
    }

    private void Grab(Transform hand)
    {
        _heldBy = hand;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        if(collisionHelper != null)
            collisionHelper.IgnoreCollisionWithWheel(true);

        if (wrenchTool != null)
            wrenchTool.SetHeld(true);

        Debug.Log("Wrench grabbed");
    }

    private void Drop()
    {
        _heldBy = null;

        if (wrenchTool != null)
            wrenchTool.SetHeld(false);

        transform.position += transform.right * 0.08f + Vector3.up * 0.03f;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = true;
            _rb.isKinematic = false;
        }

        if (collisionHelper != null)
            StartCoroutine(collisionHelper.ReenableWheelCollisionAfterDelay(0.25f));

        Debug.Log("Wrench dropped");
    }

    public void OnGrabbed()
    {
        if (wrenchTool != null)
            wrenchTool.SetHeld(true);
    }

    public void OnReleased()
    {
        if (wrenchTool != null)
            wrenchTool.SetHeld(false);
    }
}
