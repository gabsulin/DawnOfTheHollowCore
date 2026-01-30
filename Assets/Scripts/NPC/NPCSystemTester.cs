using UnityEngine;

/// <summary>
/// Helper script for testing NPC and recipe systems
/// Attach to any GameObject in your scene for quick testing
/// </summary>
public class NPCSystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private InventoryManager inventoryManager;

    /*[Header("Debug Keys")]
    [Tooltip("F5 - Save Game")]
    [Tooltip("F9 - Load Game")]
    [Tooltip("F1 - Reset All Recipes")]
    [Tooltip("F2 - List Unlocked Recipes")]
    [Tooltip("F3 - Reset All NPCs")]
    [Tooltip("F4 - List Completed NPCs")]*/
    [TextArea]
    public string debugInstructions =
        "F5 - Save Game\n" +
        "F9 - Load Game\n" +
        "F1 - Reset All Recipes\n" +
        "F2 - List Unlocked Recipes\n" +
        "F3 - Reset All NPCs\n" +
        "F4 - List Completed NPCs";

    private void Update()
    {
        // Save Game
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (inventoryManager != null)
            {
                SaveSystemJSONExtended.SaveGame(inventoryManager);
                Debug.Log("=== GAME SAVED ===");
            }
            else
            {
                Debug.LogError("Inventory Manager not assigned!");
            }
        }

        // Load Game
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (inventoryManager != null)
            {
                bool success = SaveSystemJSONExtended.LoadGame(inventoryManager);
                if (success)
                    Debug.Log("=== GAME LOADED ===");
            }
            else
            {
                Debug.LogError("Inventory Manager not assigned!");
            }
        }

        // Reset All Recipes
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (RecipeManager.Instance != null)
            {
                RecipeManager.Instance.ResetAllRecipes();
                Debug.Log("=== ALL RECIPES RESET ===");
            }
        }

        // List Unlocked Recipes
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (RecipeManager.Instance != null)
            {
                var recipes = RecipeManager.Instance.GetUnlockedRecipes();
                Debug.Log($"=== UNLOCKED RECIPES ({recipes.Count}) ===");
                foreach (var recipe in recipes)
                {
                    Debug.Log($"  - {recipe.recipeName}");
                }
            }
        }

        // Reset All NPCs
        if (Input.GetKeyDown(KeyCode.F3))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("=== ALL NPC STATES RESET (RESTART SCENE) ===");
        }

        // List Completed NPCs
        if (Input.GetKeyDown(KeyCode.F4))
        {
            int count = PlayerPrefs.GetInt("CompletedNPCCount", 0);
            Debug.Log($"=== COMPLETED NPCs ({count}) ===");
            for (int i = 0; i < count; i++)
            {
                string npcId = PlayerPrefs.GetString($"CompletedNPC_{i}", "");
                Debug.Log($"  - {npcId}");
            }
        }
    }

    private void OnGUI()
    {
        // Show controls on screen
        GUI.Box(new Rect(10, 10, 250, 140), "NPC System Tester");
        GUI.Label(new Rect(20, 35, 230, 20), "F5 - Save Game");
        GUI.Label(new Rect(20, 55, 230, 20), "F9 - Load Game");
        GUI.Label(new Rect(20, 75, 230, 20), "F1 - Reset All Recipes");
        GUI.Label(new Rect(20, 95, 230, 20), "F2 - List Recipes (Console)");
        GUI.Label(new Rect(20, 115, 230, 20), "F3 - Reset All NPCs");
        GUI.Label(new Rect(20, 135, 230, 20), "F4 - List NPCs (Console)");
    }
}