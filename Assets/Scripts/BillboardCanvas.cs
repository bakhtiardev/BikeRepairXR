using UnityEngine;

/// <summary>
/// Billboard — makes this GameObject always face the player's headset camera.
/// Attach to the CounterCanvas so it rotates away from the player rather than
/// presenting a flat plane that blocks the view.
///
/// OPTIONAL: set <see cref="anchorAbove"/> to (e.g.) the BoltZone transform and
/// the canvas will float <see cref="heightOffset"/> metres above that point,
/// always staying clear of the player's line of sight.
/// </summary>
public class BillboardCanvas : MonoBehaviour
{
    [Tooltip("If assigned, this canvas is repositioned above that transform every frame.")]
    public Transform anchorAbove;

    [Tooltip("Metres above 'anchorAbove' to float the canvas (ignored if anchorAbove is null).")]
    public float heightOffset = 0.5f;

    [Tooltip("Lock rotation so only the Y-axis tracks the player (avoids tilting up/down).")]
    public bool yAxisOnlyRotation = false;

    private Transform _cam;

    private void Start()
    {
        // Try to find the OVR center-eye anchor; fall back to Camera.main
        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null)
            _cam = rig.centerEyeAnchor;
        else if (Camera.main != null)
            _cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        // Reposition above the anchor if one is set
        if (anchorAbove != null)
        {
            Vector3 p = anchorAbove.position;
            p.y += heightOffset;
            transform.position = p;
        }

        // Face the camera
        if (yAxisOnlyRotation)
        {
            Vector3 dir = _cam.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(-dir);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - _cam.position);
        }
    }
}
