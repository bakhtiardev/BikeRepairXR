using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RepairMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelSteps;

    [Header("Main Menu")]
    public Transform optionsParent;
    public Button optionButtonPrefab;

    [Header("Steps UI")]
    public TMP_Text repairTitle;
    public TMP_Text stepBody;
    public TMP_Text stepCounter;
    public Button prevButton;
    public Button nextButton;
    public Button backButton;

    [Header("Repair Options")]
    public RepairOption[] repairOptions;

    private RepairOption current;
    private int stepIndex;

    void Start()
    {
        BuildMainMenu();

        prevButton.onClick.AddListener(PrevStep);
        nextButton.onClick.AddListener(NextStep);
        backButton.onClick.AddListener(ShowMainMenu);

        ShowMainMenu();
    }

    void BuildMainMenu()
    {
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);

        foreach (var opt in repairOptions)
        {
            Button btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<TMP_Text>().text = opt.repairName;
            btn.onClick.AddListener(() => StartRepair(opt));
        }
    }

    void StartRepair(RepairOption opt)
    {
        current = opt;
        stepIndex = 0;
        panelMain.SetActive(false);
        panelSteps.SetActive(true);
        RefreshStepUI();
    }

    void ShowMainMenu()
    {
        panelMain.SetActive(true);
        panelSteps.SetActive(false);
    }

    void RefreshStepUI()
    {
        repairTitle.text = current.repairName;
        stepBody.text = current.steps[stepIndex];
        stepCounter.text = (stepIndex + 1) + " / " + current.steps.Count;

        prevButton.interactable = stepIndex > 0;
        nextButton.interactable = stepIndex < current.steps.Count - 1;
    }

    void NextStep()
    {
        stepIndex++;
        RefreshStepUI();
    }

    void PrevStep()
    {
        stepIndex--;
        RefreshStepUI();
    }
}