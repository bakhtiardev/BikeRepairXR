using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroFlowController : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject descriptionPanel;

    public Button startButton;
    public Button backButton;
    public Button nextButton;
    public int currentPage = 0;

    [Header("Scene Settings")]
    public string bicycleSceneName = "BicycleScene";

    void Start()
    {
        if (welcomePanel != null) welcomePanel.SetActive(true);
        if (descriptionPanel != null) descriptionPanel.SetActive(false);

        if (startButton != null) startButton.onClick.AddListener(OnStartPressed);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextPressed);
        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
    }

    void OnStartPressed()
    {
        currentPage = 1;
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (descriptionPanel != null) descriptionPanel.SetActive(true);
    }

    void OnNextPressed()
    {
        currentPage++;
        SceneManager.LoadScene(bicycleSceneName);
    }

    void OnBackPressed()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 0;
            if (welcomePanel != null) welcomePanel.SetActive(true);
            if (descriptionPanel != null) descriptionPanel.SetActive(false);
        }
    }
}