using UnityEngine;

public class SaveDataResetUtility : MonoBehaviour
{
    [Header("Auto-Reset Settings")]
    [Tooltip("AUTOMATICALLY reset save data EVERY TIME you press Play (for testing)")]
    [SerializeField] private bool resetOnPlay = true;
    
    [Header("Reset Options")]
    [Tooltip("Reset area unlock states")]
    [SerializeField] private bool resetAreaUnlocks = true;
    
    [Tooltip("Reset NPC dialogue completion")]
    [SerializeField] private bool resetNPCDialogue = true;
    
    [Tooltip("Reset recipes")]
    [SerializeField] private bool resetRecipes = true;

    [Header("Status")]
    [SerializeField] private string lastResetTime = "Never";

    private void Awake()
    {
        if (resetOnPlay)
        {
            Debug.LogWarning("========================================");
            Debug.LogWarning("[SaveReset] AUTO-RESET ON PLAY ENABLED!");
            Debug.LogWarning("[SaveReset] Resetting all save data...");
            Debug.LogWarning("========================================");
            
            ResetAllSaveData();
            
            Debug.LogWarning("[SaveReset] ✅ Save data reset complete!");
            Debug.LogWarning("[SaveReset] Disable 'Reset On Play' for production!");
            Debug.LogWarning("========================================");
        }
    }

    public void ResetAllSaveData()
    {
        Debug.Log("[SaveReset] RESETTING ALL SAVE DATA");

        if (resetAreaUnlocks)
        {
            ResetAreaUnlocks();
        }

        if (resetNPCDialogue)
        {
            ResetNPCDialogue();
        }

        if (resetRecipes)
        {
            ResetRecipes();
        }

        lastResetTime = System.DateTime.Now.ToString("HH:mm:ss");
        
        Debug.Log("[SaveReset] RESET COMPLETE!");
        Debug.Log("[SaveReset] You may need to restart the scene");
    }

    public void ResetAreaUnlocks()
    {
        if (AreaUnlockManager.Instance != null)
        {
            AreaUnlockManager.Instance.ResetAllProgression();
            Debug.Log("[SaveReset]Area unlocks reset");
        }
        else
        {
            for (int i = 1; i <= 4; i++)
            {
                PlayerPrefs.DeleteKey($"Area_{i}_NPCCompleted");
                PlayerPrefs.DeleteKey($"Area_{i}_Unlocked");
            }
            PlayerPrefs.Save();
            Debug.Log("[SaveReset] ✓ Area unlocks reset (manual)");
        }
    }

    public void ResetNPCDialogue()
    {
        int count = PlayerPrefs.GetInt("CompletedNPCCount", 0);
        
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"CompletedNPC_{i}");
        }
        PlayerPrefs.DeleteKey("CompletedNPCCount");

        string[] allKeys = GetAllPlayerPrefsKeys();
        foreach (string key in allKeys)
        {
            if (key.StartsWith("NPC_Completed_"))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveReset]NPC dialogue states reset");
    }

    [ContextMenu("Reset Recipes Only")]
    public void ResetRecipes()
    {
        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.ResetAllRecipes();
            Debug.Log("[SaveReset]Recipes reset");
        }
        else
        {
            Debug.LogWarning("[SaveReset] RecipeManager not found - restart scene to reset recipes");
        }
    }

    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.LogWarning("========================================");
        Debug.LogWarning("[SaveReset] ALL PLAYERPREFS DELETED!");
        Debug.LogWarning("[SaveReset] This includes ALL save data!");
        Debug.LogWarning("[SaveReset] RESTART THE SCENE NOW!");
        Debug.LogWarning("========================================");
    }

    public void ListAllSaveKeys()
    {
        Debug.Log("========================================");
        Debug.Log("[SaveReset] ACTIVE SAVE KEYS:");
        Debug.Log("========================================");

        string[] keys = GetAllPlayerPrefsKeys();
        
        if (keys.Length == 0)
        {
            Debug.Log("No save keys found!");
        }
        else
        {
            foreach (string key in keys)
            {
                int intValue = PlayerPrefs.GetInt(key, -999);
                if (intValue != -999)
                {
                    Debug.Log($"  {key} = {intValue}");
                }
                else
                {
                    string strValue = PlayerPrefs.GetString(key, "");
                    Debug.Log($"  {key} = \"{strValue}\"");
                }
            }
        }
    }

    private string[] GetAllPlayerPrefsKeys()
    {
        System.Collections.Generic.List<string> keys = new System.Collections.Generic.List<string>();

        for (int i = 0; i <= 4; i++)
        {
            if (PlayerPrefs.HasKey($"Area_{i}_NPCCompleted"))
                keys.Add($"Area_{i}_NPCCompleted");
            if (PlayerPrefs.HasKey($"Area_{i}_Unlocked"))
                keys.Add($"Area_{i}_Unlocked");
        }

        int npcCount = PlayerPrefs.GetInt("CompletedNPCCount", 0);
        if (PlayerPrefs.HasKey("CompletedNPCCount"))
            keys.Add("CompletedNPCCount");

        for (int i = 0; i < npcCount; i++)
        {
            if (PlayerPrefs.HasKey($"CompletedNPC_{i}"))
                keys.Add($"CompletedNPC_{i}");
        }

        Debug.Log("[SaveReset] Note: There may be additional NPC_Completed_ keys not shown");

        return keys.ToArray();
    }

    /*private void OnGUI()
    {
        // Show on-screen instructions
        GUI.Box(new Rect(10, 160, 280, 120), "Save Data Reset");
        
        // Show auto-reset status
        Color oldColor = GUI.color;
        if (resetOnPlay)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(20, 185, 260, 20), "⚠️ AUTO-RESET ON PLAY: ON");
            GUI.color = oldColor;
        }
        else
        {
            GUI.Label(new Rect(20, 185, 260, 20), "Auto-Reset: OFF");
        }
        
        GUI.Label(new Rect(20, 205, 260, 20), $"F10 - Manual Reset");
        GUI.Label(new Rect(20, 225, 260, 20), $"Last Reset: {lastResetTime}");
        
        if (GUI.Button(new Rect(20, 250, 250, 20), "Delete ALL PlayerPrefs (NUCLEAR)"))
        {
            DeleteAllPlayerPrefs();
        }
    }*/
}