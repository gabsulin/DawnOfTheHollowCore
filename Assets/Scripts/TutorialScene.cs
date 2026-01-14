using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialScene : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string stepName;
        [TextArea(3, 6)]
        public string tutorialText;
        public Sprite tutorialImage;
        public bool pauseGame = true;
        public bool requiresInput = true;
    }

    [Header("Tutorial Steps")]
    public TutorialStep[] tutorialSteps;

    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI continuePrompt;

    [Header("Settings")]
    public KeyCode continueKey = KeyCode.Return;
    public string continuePromptText = "Press ENTER to continue...";

    private int currentStepIndex = -1;
    private bool isShowingTutorial = false;
    private bool canContinue = false;

    void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.text = continuePromptText;
    }

    void Update()
    {
        if (isShowingTutorial && canContinue)
        {
            if (Input.GetKeyDown(continueKey))
            {
                HideTutorial();
            }
        }
    }

    public void ShowTutorialStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialSteps.Length)
        {
            Debug.LogWarning($"Tutorial step {stepIndex} out of range!");
            return;
        }

        currentStepIndex = stepIndex;
        TutorialStep step = tutorialSteps[stepIndex];

        if (step.pauseGame)
        {
            Time.timeScale = 0f;
        }

        if (tutorialText != null)
            tutorialText.text = step.tutorialText;

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(step.requiresInput);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        isShowingTutorial = true;
        canContinue = step.requiresInput;

        Debug.Log($"Showing tutorial: {step.stepName}");
    }

    public void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Time.timeScale = 1f;

        isShowingTutorial = false;
        canContinue = false;

        Debug.Log("Tutorial hidden, game resumed");
    }

    public bool IsShowingTutorial()
    {
        return isShowingTutorial;
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
}