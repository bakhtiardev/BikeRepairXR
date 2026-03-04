using UnityEngine;

/// <summary>
/// Simulates XR/VR head and controller movement in the Unity Editor using keyboard and mouse.
/// Attach this script to the OVRCameraRig (or any parent transform) to enable simulation.
/// 
/// Controls:
///   Right Mouse   - Hold to rotate view (look around)
///   W/A/S/D       - Move forward/left/backward/right
///   Q/E           - Move down/up
///   Left Shift    - Hold to move faster
/// </summary>
public class XRSimulator : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fastMoveMultiplier = 3f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 2f;

    private float _yaw;
    private float _pitch;
    private bool _rotating;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _rotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _rotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (_rotating)
        {
            _yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, -80f, 80f);
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }

    private void HandleMovement()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMoveMultiplier : 1f);
        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
        if (Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.E)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) dir -= Vector3.up;

        transform.position += dir.normalized * speed * Time.deltaTime;
    }
#endif
}
