using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    public ItemSO sacrificeItem;

    [Header("UI")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Timing")]
    public float delayBeforeWinScreen = 2f;
    public float delayBeforeMainMenu = 3f;

    private PlayerHpSystem playerHpSystem;
    private InventoryController inventoryController;
    private bool gameEnded = false;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        playerHpSystem = FindFirstObjectByType<PlayerHpSystem>();
        inventoryController = FindFirstObjectByType<InventoryController>();

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        var craftingPool = FindFirstObjectByType<CraftingPool>();
        if (craftingPool != null)
            craftingPool.OnItemAdded += OnItemAddedToPool;
        else
            Debug.LogWarning("[GameManager] No CraftingPool found in scene.");
    }

    private void OnItemAddedToPool(ItemSO item)
    {
        if (gameEnded) return;
        if (sacrificeItem == null) return;
        if (item.SaveId == sacrificeItem.SaveId)
            EndGame(2);
    }

    public void EndGame(int reason)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (reason == 1)
        {
            Time.timeScale = 0f;

            if (playerHpSystem != null)
                playerHpSystem.ForceDeath();

            if (losePanel != null) losePanel.SetActive(true);
            Debug.Log("[GameManager] Core destroyed - Player loses.");

            StartCoroutine(ReturnToMainMenuAfterDelay());
        }
        else if (reason == 2)
        {
            StartCoroutine(WinSequence());
        }
    }

    private IEnumerator WinSequence()
    {
        if (inventoryController != null)
            inventoryController.Close();

        if (playerHpSystem != null)
            playerHpSystem.ForceDeath();

        Debug.Log("[GameManager] Sacrifice complete - waiting before showing win screen.");

        yield return new WaitForSecondsRealtime(delayBeforeWinScreen);

        Time.timeScale = 0f;

        if (winPanel != null) winPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(delayBeforeMainMenu);

        Time.timeScale = 1f;

        NightCounter.currentNight = 0;

        if (AreaUnlockManager.Instance != null)
            AreaUnlockManager.Instance.ResetAllProgression();

        if (RecipeManager.Instance != null)
            RecipeManager.Instance.ResetAllRecipes();

        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator ReturnToMainMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeMainMenu);

        Time.timeScale = 1f;

        NightCounter.currentNight = 0;

        if (AreaUnlockManager.Instance != null)
            AreaUnlockManager.Instance.ResetAllProgression();

        if (RecipeManager.Instance != null)
            RecipeManager.Instance.ResetAllRecipes();

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        var craftingPool = FindFirstObjectByType<CraftingPool>();
        if (craftingPool != null)
            craftingPool.OnItemAdded -= OnItemAddedToPool;
    }
}