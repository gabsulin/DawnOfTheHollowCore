using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;

    private bool isPaused = false;
    private TutorialScene tutorialScene;
    private InventoryController inventoryController;

    void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        tutorialScene = FindFirstObjectByType<TutorialScene>();
        inventoryController = FindFirstObjectByType<InventoryController>();
        Debug.Log(inventoryController);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryController != null && inventoryController.open)
                return;

            if (settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        isPaused = false;

        if (tutorialScene != null && tutorialScene.IsTutorialPaused())
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        NightCounter.currentNight = 0;

        if (AreaUnlockManager.Instance != null)
            AreaUnlockManager.Instance.ResetAllProgression();

        if (RecipeManager.Instance != null)
            RecipeManager.Instance.ResetAllRecipes();

        SceneManager.LoadScene("MainMenu");
    }
}