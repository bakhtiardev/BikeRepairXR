using UnityEngine;

/// <summary>
/// Recenters the VR tracking origin when a scene loads.
/// Attach this to the OVRCameraRig in each experiment scene to ensure
/// consistent player positioning across scene transitions.
/// </summary>
public class SceneRecenter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Enable debug logging")]
    public bool debugMode = true;

    [Tooltip("Delay in seconds before recentering (0 = immediate)")]
    public float recenterDelay = 0.1f;

    void Start()
    {
        if (recenterDelay > 0)
            Invoke(nameof(RecenterTracking), recenterDelay);
        else
            RecenterTracking();
    }

    void RecenterTracking()
    {
        if (OVRManager.display != null)
        {
            OVRManager.display.RecenterPose();
            if (debugMode)
                Debug.Log("[SceneRecenter] Tracking origin recentered for scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning("[SceneRecenter] OVRManager.display not found - cannot recenter");
        }
    }
}
