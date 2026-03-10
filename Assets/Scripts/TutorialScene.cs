using UnityEngine;

public class TutorialScene : MonoBehaviour
{
    [Header("Tutorial Panels - Design these in Canvas!")]
    public GameObject[] tutorialPanels;

    [Header("Settings")]
    public KeyCode continueKey = KeyCode.Return;
    public bool pauseGameDuringTutorial = true;

    private int currentPanelIndex = -1;
    private bool isShowingTutorial = false;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
    }
    void Start()
    {
        HideAllPanels();
    }

    void Update()
    {
        if (isShowingTutorial && Input.GetKeyDown(continueKey))
        {
            HideCurrentPanel();
        }
    }

    public void ShowPanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= tutorialPanels.Length)
        {
            Debug.LogWarning($"Tutorial panel {panelIndex} out of range!");
            return;
        }

        HideAllPanels();

        currentPanelIndex = panelIndex;
        tutorialPanels[panelIndex].SetActive(true);
        isShowingTutorial = true;

        if (pauseGameDuringTutorial)
        {
            Time.timeScale = 0f;
        }

        Debug.Log($"Showing tutorial panel {panelIndex}");
    }

    public void HideCurrentPanel()
    {
        if (currentPanelIndex >= 0 && currentPanelIndex < tutorialPanels.Length)
        {
            tutorialPanels[currentPanelIndex].SetActive(false);
        }

        currentPanelIndex = -1;
        isShowingTutorial = false;

        Time.timeScale = 1f;

        Debug.Log("Tutorial panel hidden, game resumed");
    }

    private void HideAllPanels()
    {
        foreach (GameObject panel in tutorialPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    public bool IsShowingTutorial()
    {
        return isShowingTutorial;
    }

    public bool IsTutorialPaused()
    {
        return isShowingTutorial && pauseGameDuringTutorial;
    }
}