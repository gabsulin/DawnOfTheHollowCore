using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystemJSONExtended
{
    static string fileName = "player_full_save.json";
    static string PathToFile => Path.Combine(Application.persistentDataPath, fileName);

    [System.Serializable]
    class FullSaveData
    {
        public List<string> slotIds = new List<string>();
        public List<int> slotCounts = new List<int>();
        public List<string> unlockedRecipes = new List<string>();
        public List<string> completedNPCs = new List<string>();
    }
    public static void SaveGame(InventoryManager inv)
    {
        var saveData = new FullSaveData();

        var invData = inv.GetSaveData();
        saveData.slotIds = invData.slotIds;
        saveData.slotCounts = invData.slotCounts;

        if (RecipeManager.Instance != null)
        {
            saveData.unlockedRecipes = RecipeManager.Instance.GetUnlockedRecipeNames();
        }

        saveData.completedNPCs = GetCompletedNPCIds();

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(PathToFile, json);
        Debug.Log($"[SaveSystem] Game saved to: {PathToFile}");
    }
    public static bool LoadGame(InventoryManager inv)
    {
        if (!File.Exists(PathToFile))
        {
            Debug.LogWarning("[SaveSystem] No save file found");
            return false;
        }

        string json = File.ReadAllText(PathToFile);
        var saveData = JsonUtility.FromJson<FullSaveData>(json);

        var invData = new InventorySaveData
        {
            slotIds = saveData.slotIds,
            slotCounts = saveData.slotCounts
        };
        inv.LoadFromSave(invData);

        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.LoadUnlockedRecipes(saveData.unlockedRecipes);
        }

        SetCompletedNPCIds(saveData.completedNPCs);

        Debug.Log($"[SaveSystem] Game loaded from: {PathToFile}");
        return true;
    }
    private static List<string> GetCompletedNPCIds()
    {
        List<string> completed = new List<string>();

        int count = PlayerPrefs.GetInt("CompletedNPCCount", 0);

        for (int i = 0; i < count; i++)
        {
            string npcId = PlayerPrefs.GetString($"CompletedNPC_{i}", "");
            if (!string.IsNullOrEmpty(npcId))
                completed.Add(npcId);
        }

        return completed;
    }
    private static void SetCompletedNPCIds(List<string> completedNPCs)
    {
        if (completedNPCs == null) return;

        PlayerPrefs.SetInt("CompletedNPCCount", completedNPCs.Count);

        for (int i = 0; i < completedNPCs.Count; i++)
        {
            PlayerPrefs.SetString($"CompletedNPC_{i}", completedNPCs[i]);

            PlayerPrefs.SetInt($"NPC_Completed_{completedNPCs[i]}", 1);
        }

        PlayerPrefs.Save();
    }
    public static void DeleteSave()
    {
        if (File.Exists(PathToFile))
        {
            File.Delete(PathToFile);
            Debug.Log("[SaveSystem] Save file deleted");
        }

        int count = PlayerPrefs.GetInt("CompletedNPCCount", 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"CompletedNPC_{i}");
        }
        PlayerPrefs.DeleteKey("CompletedNPCCount");
        PlayerPrefs.Save();
    }
}