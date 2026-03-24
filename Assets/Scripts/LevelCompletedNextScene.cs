using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles transitioning to the next experiment scene when the B button is pressed.
/// Attach this to the LevelCompletedCanvas GameObject.
///
/// USAGE:
///   1. Attach this script to the LevelCompletedCanvas in each experiment scene.
///   2. Set the 'nextSceneName' field to the name of the scene to load next.
///   3. When the canvas is active, pressing B on the right controller loads the next scene.
/// </summary>
public class LevelCompletedNextScene : MonoBehaviour
{
    [Header("Next Scene")]
    [Tooltip("Name of the scene to load when B is pressed. Must match exactly the scene name in Build Settings.")]
    public string nextSceneName = "";

    [Header("Settings")]
    [Tooltip("Enable to show debug messages in the console")]
    public bool debugMode = true;

    // Button state tracking to prevent multiple triggers from holding
    private bool wasBPressed = false;

    // Track if this canvas is active
    private bool isActive = false;

    void OnEnable()
    {
        isActive = true;
        if (debugMode)
            Debug.Log($"[LevelCompletedNextScene] Canvas enabled. Press B to go to: {nextSceneName}");
    }

    void OnDisable()
    {
        isActive = false;
    }

    void Update()
    {
        if (!isActive)
            return;

        // Check for B button press on right controller (OVR Button.Two)
        bool bPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch);

        if (bPressed && !wasBPressed)
        {
            LoadNextExperiment();
        }

        wasBPressed = bPressed;
    }

    /// <summary>
    /// Loads the next experiment scene.
    /// </summary>
    public void LoadNextExperiment()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError($"[LevelCompletedNextScene] 'nextSceneName' is not set! Please specify the next scene in the Inspector.");
            return;
        }

        if (debugMode)
            Debug.Log($"[LevelCompletedNextScene] Loading next scene: {nextSceneName}");

        SceneManager.LoadScene(nextSceneName);
    }
}
