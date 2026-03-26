using UnityEngine;
using Meta.XR.ImmersiveDebugger;

/// <summary>
/// Toggles locomotion on/off for experiments.
/// Controls both the OVR Locomotor GameObject and any custom locomotion.
///
/// Attach this to the OVRCameraRig GameObject.
///
/// locomotionEnabled can be toggled live from the Immersive Debugger panel
/// (Category: Experiment) to disable/re-enable player movement mid-experiment.
/// </summary>
public class ThumbstickLocomotion : MonoBehaviour
{
    [Tooltip("Reference to the OVR Locomotor GameObject to toggle.")]
    public GameObject ovrLocomotor;

    [DebugMember(Category = "Experiment", DisplayName = "Locomotion Enabled", Tweakable = true)]
    public bool locomotionEnabled = true;

    private bool _lastLocomotionEnabled = true;

    private void Start()
    {
        if (ovrLocomotor == null)
        {
            ovrLocomotor = transform.Find("Locomotor")?.gameObject;
            if (ovrLocomotor == null)
                ovrLocomotor = GameObject.Find("Locomotor");
        }

        ApplyLocomotionState();
    }

    private void Update()
    {
        if (locomotionEnabled != _lastLocomotionEnabled)
        {
            Debug.Log($"[ThumbstickLocomotion] locomotionEnabled changed to: {locomotionEnabled}");
            _lastLocomotionEnabled = locomotionEnabled;
            ApplyLocomotionState();
        }
    }

    private void ApplyLocomotionState()
    {
        if (ovrLocomotor != null && ovrLocomotor.activeSelf != locomotionEnabled)
        {
            ovrLocomotor.SetActive(locomotionEnabled);
            Debug.Log($"[ThumbstickLocomotion] OVR Locomotor set active: {locomotionEnabled}");
        }
    }
}
