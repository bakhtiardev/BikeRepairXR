using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles level restart functionality.
/// Press A button 5 times to show restart confirmation dialog.
/// Press A again to restart, or any other button to cancel.
/// </summary>
public class RestartLevelController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Number of A button presses required to trigger restart prompt")]
    public int pressesRequired = 5;
    
    [Tooltip("Time window in seconds to complete the button presses")]
    public float pressTimeWindow = 3f;
    
    [Tooltip("Cooldown time after canceling before counting can resume")]
    public float cooldownAfterCancel = 1f;
    
    [Header("UI References")]
    [Tooltip("Panel to show when restart is prompted")]
    public GameObject restartPromptPanel;
    
    [Tooltip("Text to display restart confirmation message")]
    public TextMeshProUGUI promptText;
    
    [Tooltip("Text to show current press count (optional)")]
    public TextMeshProUGUI countText;
    
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load for restart. If empty, reloads current scene.")]
    public string sceneToLoad = "";
    
    // State tracking
    private int pressCount = 0;
    private float lastPressTime = 0f;
    private float lastCancelTime = 0f;
    private bool isPromptShowing = false;
    
    // Button state tracking to prevent holding from counting multiple times
    private bool wasAPressed = false;
    private bool wasBPressed = false;
    private bool wasXPressed = false;
    private bool wasYPressed = false;
    private bool wasLeftTriggerPressed = false;
    private bool wasRightTriggerPressed = false;
    private bool wasLeftGripPressed = false;
    private bool wasRightGripPressed = false;
    
    void Start()
    {
        // Create UI if not assigned
        if (restartPromptPanel == null)
        {
            CreateDefaultUI();
        }
        
        // Hide prompt initially
        if (restartPromptPanel != null)
        {
            restartPromptPanel.SetActive(false);
        }
        
        if (countText != null)
        {
            countText.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // Don't process input if in cooldown
        if (Time.time - lastCancelTime < cooldownAfterCancel && !isPromptShowing)
        {
            return;
        }
        
        // Check for A button press (Button.One on right controller)
        bool aPressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
        
        if (aPressed && !wasAPressed)
        {
            OnAButtonPressed();
        }
        wasAPressed = aPressed;
        
        // If prompt is showing, check for other buttons to cancel
        if (isPromptShowing)
        {
            CheckCancelButtonPresses();
        }
    }
    
    void OnAButtonPressed()
    {
        if (isPromptShowing)
        {
            // Confirm restart
            RestartLevel();
            return;
        }
        
        // Check if we're within the time window
        if (Time.time - lastPressTime > pressTimeWindow)
        {
            // Reset count if too much time has passed
            pressCount = 0;
        }
        
        pressCount++;
        lastPressTime = Time.time;
        
        // Update count display
        if (countText != null)
        {
            countText.gameObject.SetActive(true);
            countText.text = $"Restart: {pressCount}/{pressesRequired}";
        }
        
        Debug.Log($"A button pressed: {pressCount}/{pressesRequired}");
        
        // Check if we've reached the required count
        if (pressCount >= pressesRequired)
        {
            ShowRestartPrompt();
            pressCount = 0;
            
            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }
        }
    }
    
    void CheckCancelButtonPresses()
    {
        // Check B button (Button.Two on right controller)
        bool bPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (bPressed && !wasBPressed)
        {
            CancelRestart();
            wasBPressed = bPressed;
            return;
        }
        wasBPressed = bPressed;
        
        // Check X button (Button.One on left controller)
        bool xPressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch);
        if (xPressed && !wasXPressed)
        {
            CancelRestart();
            wasXPressed = xPressed;
            return;
        }
        wasXPressed = xPressed;
        
        // Check Y button (Button.Two on left controller)
        bool yPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch);
        if (yPressed && !wasYPressed)
        {
            CancelRestart();
            wasYPressed = yPressed;
            return;
        }
        wasYPressed = yPressed;
        
        // Check triggers
        bool leftTrigger = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        bool rightTrigger = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        
        if (leftTrigger && !wasLeftTriggerPressed)
        {
            CancelRestart();
            wasLeftTriggerPressed = leftTrigger;
            return;
        }
        wasLeftTriggerPressed = leftTrigger;
        
        if (rightTrigger && !wasRightTriggerPressed)
        {
            CancelRestart();
            wasRightTriggerPressed = rightTrigger;
            return;
        }
        wasRightTriggerPressed = rightTrigger;
        
        // Check grips
        bool leftGrip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        bool rightGrip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        
        if (leftGrip && !wasLeftGripPressed)
        {
            CancelRestart();
            wasLeftGripPressed = leftGrip;
            return;
        }
        wasLeftGripPressed = leftGrip;
        
        if (rightGrip && !wasRightGripPressed)
        {
            CancelRestart();
            wasRightGripPressed = rightGrip;
            return;
        }
        wasRightGripPressed = rightGrip;
    }
    
    void ShowRestartPrompt()
    {
        isPromptShowing = true;
        
        if (restartPromptPanel != null)
        {
            restartPromptPanel.SetActive(true);
        }
        
        Debug.Log("Restart prompt shown. Press A to restart, or any other button to cancel.");
    }
    
    void CancelRestart()
    {
        isPromptShowing = false;
        lastCancelTime = Time.time;
        
        if (restartPromptPanel != null)
        {
            restartPromptPanel.SetActive(false);
        }
        
        Debug.Log("Restart canceled.");
    }
    
    void RestartLevel()
    {
        Debug.Log("Restarting level...");
        
        // Hide prompt
        if (restartPromptPanel != null)
        {
            restartPromptPanel.SetActive(false);
        }
        
        // Load the specified scene or reload current scene
        string sceneName = string.IsNullOrEmpty(sceneToLoad) ? SceneManager.GetActiveScene().name : sceneToLoad;
        SceneManager.LoadScene(sceneName);
    }
    
    void CreateDefaultUI()
    {
        // Create a simple canvas for the restart prompt
        GameObject canvasObj = new GameObject("RestartPromptCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        // Set canvas size and position
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.6f);
        canvasRect.position = new Vector3(0, 1.5f, 2f);
        canvasRect.rotation = Quaternion.Euler(0, 180, 0);
        
        // Add canvas scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;
        
        // Create panel
        GameObject panelObj = new GameObject("RestartPromptPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Create text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-0.1f, -0.1f);
        
        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Restart Level?\n\nPress A to confirm\nPress any other button to cancel";
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontSize = 0.08f;
        promptText.color = Color.white;
        
        // Assign references
        restartPromptPanel = panelObj;
        
        Debug.Log("Created default restart prompt UI");
    }
    
    // Public method to manually trigger restart (useful for UI buttons)
    public void TriggerRestart()
    {
        RestartLevel();
    }
    
    // Public method to manually show prompt
    public void ShowPrompt()
    {
        ShowRestartPrompt();
    }
    
    // Public method to manually cancel
    public void CancelPrompt()
    {
        CancelRestart();
    }
}
