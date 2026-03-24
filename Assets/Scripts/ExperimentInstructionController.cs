using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Manages instruction navigation for the ExperimentInstructions canvas.
/// Allows users to navigate through multiple instruction pages using Next/Prev buttons.
/// Each instruction can have an associated video clip.
/// </summary>
public class ExperimentInstructionController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display instructions")]
    public TextMeshProUGUI instructionText;
    
    [Tooltip("Next button to go to the next instruction")]
    public Button nextButton;
    
    [Tooltip("Previous/Back button to go to the previous instruction")]
    public Button prevButton;
    
    [Header("Video References")]
    [Tooltip("VideoPlayer component for playing instruction videos")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("RawImage component for displaying video output")]
    public RawImage videoImage;
    
    [Header("Instructions")]
    [Tooltip("Array of instruction strings for each page")]
    [TextArea(3, 10)]
    public string[] instructions = new string[]
    {
        "Welcome! Follow the instructions to complete the task.",
        "Step 1: Locate the pedal that needs to be replaced.",
        "Step 2: Use the wrench to loosen the pedal bolt.",
        "Step 3: Remove the old pedal carefully.",
        "Step 4: Install the new pedal and tighten securely.",
        "You're all set! Great job completing the task."
    };
    
    [Header("Videos")]
    [Tooltip("Array of video clips corresponding to each instruction. Leave null for instructions without video.")]
    public VideoClip[] instructionVideos;
    
    [Header("Settings")]
    [Tooltip("Whether to loop back to the first instruction after the last one")]
    public bool loopInstructions = false;
    
    [Tooltip("Whether to loop the current video while on its instruction")]
    public bool loopCurrentVideo = true;
    
    [Header("Controller Button Mapping")]
    [Tooltip("Use left controller X button for Prev and Y button for Next")]
    public bool enableControllerButtons = true;

    [Header("Ray-Based Controller Interaction")]
    [Tooltip("Allow index trigger rays to click UI buttons on this instruction canvas")]
    public bool enableRayTriggerInteraction = true;

    [Tooltip("Maximum ray distance in meters for trigger interactions")]
    public float maxRayDistance = 10f;

    private int currentIndex = 0;
    private Canvas targetCanvas;
    private Transform leftController;
    private Transform rightController;
    private RenderTexture _dynamicRT;
    // Preserve the width set in the editor; height will be computed from video aspect ratio
    private float _displayWidth;

    void Update()
    {
        if (enableControllerButtons)
        {
            // Y button (left controller) -> Next
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
                OnNextClicked();

            // X button (left controller) -> Prev
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
                OnPrevClicked();
        }

        if (enableRayTriggerInteraction)
        {
            bool leftTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
            bool rightTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

            if (leftTrigger) TryClickButton(leftController);
            if (rightTrigger) TryClickButton(rightController);
        }
    }

    void Start()
    {
        targetCanvas = GetComponent<Canvas>();

        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            leftController = rig.leftControllerAnchor;
            rightController = rig.rightControllerAnchor;
        }

        // Auto-find UI elements if not assigned
        if (instructionText == null)
        {
            GameObject textObj = GameObject.Find("InstructionText");
            if (textObj != null) instructionText = textObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (nextButton == null)
        {
            GameObject nextBtnObj = GameObject.Find("NextButton");
            if (nextBtnObj != null) nextButton = nextBtnObj.GetComponent<Button>();
        }
        
        if (prevButton == null)
        {
            GameObject prevBtnObj = GameObject.Find("PrevButton");
            if (prevBtnObj != null) prevButton = prevBtnObj.GetComponent<Button>();
        }
        
        // Auto-find video components if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
            if (videoPlayer == null)
            {
                GameObject videoObj = GameObject.Find("VideoImage");
                if (videoObj != null) videoPlayer = videoObj.GetComponent<VideoPlayer>();
            }
        }
        
        if (videoImage == null)
        {
            videoImage = GetComponentInChildren<RawImage>(true);
            if (videoImage == null)
            {
                GameObject videoObj = GameObject.Find("VideoImage");
                if (videoObj != null) videoImage = videoObj.GetComponent<RawImage>();
            }
        }
        
        // Cache the designed display width from the RectTransform so we can
        // recompute height once we know the video's actual aspect ratio.
        if (videoImage != null)
            _displayWidth = videoImage.rectTransform.sizeDelta.x;
        if (_displayWidth <= 0f) _displayWidth = 1f;

        // Initialize video player settings
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = loopCurrentVideo;
            videoPlayer.playOnAwake = false;

            // RenderTexture mode is required; AspectRatio is ignored in this mode
            // so we handle it manually via prepareCompleted.
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = null; // will be assigned dynamically
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
        
        // Add listeners to buttons
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }
        else
        {
            Debug.LogWarning("ExperimentInstructionController: NextButton not found!");
        }
        
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(OnPrevClicked);
        }
        else
        {
            Debug.LogWarning("ExperimentInstructionController: PrevButton not found!");
        }
        
        // Display first instruction
        UpdateInstructionDisplay();
        
        Debug.Log($"ExperimentInstructionController initialized with {instructions.Length} instructions");
    }

    void TryClickButton(Transform anchor)
    {
        if (anchor == null || targetCanvas == null) return;

        Ray ray = new Ray(anchor.position, anchor.forward);
        Plane canvasPlane = new Plane(targetCanvas.transform.forward, targetCanvas.transform.position);

        if (!canvasPlane.Raycast(ray, out float distance) || distance > maxRayDistance) return;

        Vector3 hitPoint = ray.GetPoint(distance);

        foreach (Button button in targetCanvas.GetComponentsInChildren<Button>(true))
        {
            if (!button.IsInteractable()) continue;

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null && WorldPointInRect(rect, hitPoint))
            {
                button.onClick.Invoke();
                return;
            }
        }
    }

    static bool WorldPointInRect(RectTransform rectTransform, Vector3 worldPoint)
    {
        Vector3 localPoint = rectTransform.InverseTransformPoint(worldPoint);
        return rectTransform.rect.Contains(new Vector2(localPoint.x, localPoint.y));
    }
    
    void OnNextClicked()
    {
        if (instructions.Length == 0) return;
        
        currentIndex++;
        
        if (currentIndex >= instructions.Length)
        {
            if (loopInstructions)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = instructions.Length - 1;
                Debug.Log("Reached last instruction");
            }
        }
        
        UpdateInstructionDisplay();
        Debug.Log($"Next clicked. Now showing instruction {currentIndex + 1}/{instructions.Length}");
    }
    
    void OnPrevClicked()
    {
        if (instructions.Length == 0) return;
        
        currentIndex--;
        
        if (currentIndex < 0)
        {
            if (loopInstructions)
            {
                currentIndex = instructions.Length - 1;
            }
            else
            {
                currentIndex = 0;
                Debug.Log("Already at first instruction");
            }
        }
        
        UpdateInstructionDisplay();
        Debug.Log($"Previous clicked. Now showing instruction {currentIndex + 1}/{instructions.Length}");
    }
    
    void UpdateInstructionDisplay()
    {
        if (instructionText != null && instructions.Length > 0 && currentIndex >= 0 && currentIndex < instructions.Length)
        {
            instructionText.text = instructions[currentIndex];
        }
        
        // Update video for current instruction
        UpdateVideoDisplay();
        
        // Update button interactability
        UpdateButtonStates();
    }
    
    void UpdateVideoDisplay()
    {
        if (videoPlayer == null) return;
        
        // Stop current video
        videoPlayer.Stop();
        
        // Check if there's a video for this instruction
        if (instructionVideos != null && currentIndex >= 0 && currentIndex < instructionVideos.Length)
        {
            VideoClip clip = instructionVideos[currentIndex];
            if (clip != null)
            {
                videoPlayer.clip = clip;
                videoPlayer.isLooping = loopCurrentVideo;
                // Prepare first — OnVideoPrepared will create a correctly-sized
                // RenderTexture and then start playback.
                videoPlayer.Prepare();
                
                // Show video image
                if (videoImage != null)
                    videoImage.enabled = true;
            }
            else
            {
                // No video for this instruction
                videoPlayer.clip = null;
                if (videoImage != null)
                    videoImage.enabled = false;
            }
        }
        else
        {
            // No video array or index out of range
            videoPlayer.clip = null;
            if (videoImage != null)
                videoImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Called by the VideoPlayer once it has decoded the first frame and knows
    /// the video's native width and height. Creates a RenderTexture that exactly
    /// matches those dimensions so no aspect-ratio distortion occurs, then plays.
    /// </summary>
    void OnVideoPrepared(VideoPlayer vp)
    {
        uint vidW = vp.width;
        uint vidH = vp.height;

        if (vidW == 0 || vidH == 0)
        {
            Debug.LogWarning("ExperimentInstructionController: Video reported 0 dimensions after prepare.");
            vp.Play();
            return;
        }

        // Release the previous dynamic RT if we had one
        if (_dynamicRT != null)
        {
            if (videoPlayer != null) videoPlayer.targetTexture = null;
            if (videoImage  != null) videoImage.texture        = null;
            _dynamicRT.Release();
            Destroy(_dynamicRT);
            _dynamicRT = null;
        }

        _dynamicRT = new RenderTexture((int)vidW, (int)vidH, 0);
        _dynamicRT.name = "VideoRT_Dynamic";
        _dynamicRT.Create();

        vp.targetTexture = _dynamicRT;

        if (videoImage != null)
        {
            videoImage.texture = _dynamicRT;
            // Full UV — no manual cropping hacks needed
            videoImage.uvRect = new Rect(0f, 0f, 1f, 1f);

            // Resize the display quad to match the video's true aspect ratio
            float aspect = (float)vidW / vidH;
            RectTransform rt = videoImage.rectTransform;
            rt.sizeDelta = new Vector2(_displayWidth, _displayWidth / aspect);
        }

        vp.Play();
        Debug.Log($"ExperimentInstructionController: Video prepared at {vidW}x{vidH}, RT created, playing.");
    }

    void UpdateButtonStates()
    {
        if (!loopInstructions)
        {
            // Disable prev button on first page
            if (prevButton != null)
            {
                prevButton.interactable = currentIndex > 0;
            }
            
            // Disable next button on last page
            if (nextButton != null)
            {
                nextButton.interactable = currentIndex < instructions.Length - 1;
            }
        }
    }
    
    /// <summary>
    /// Set the current instruction index programmatically
    /// </summary>
    public void SetInstructionIndex(int index)
    {
        if (index >= 0 && index < instructions.Length)
        {
            currentIndex = index;
            UpdateInstructionDisplay();
        }
        else
        {
            Debug.LogWarning($"Invalid instruction index: {index}. Valid range: 0 to {instructions.Length - 1}");
        }
    }
    
    /// <summary>
    /// Get the current instruction index
    /// </summary>
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    
    /// <summary>
    /// Get the total number of instructions
    /// </summary>
    public int GetInstructionCount()
    {
        return instructions.Length;
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextClicked);
        if (prevButton != null)
            prevButton.onClick.RemoveListener(OnPrevClicked);
        
        // Clean up video player
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            if (videoPlayer.isPlaying) videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }

        // Release the dynamically created RenderTexture
        if (_dynamicRT != null)
        {
            if (videoImage != null) videoImage.texture = null;
            _dynamicRT.Release();
            Destroy(_dynamicRT);
            _dynamicRT = null;
        }
    }
}
