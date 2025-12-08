using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystemJSON
{
    static string fileName = "player_inventory_save.json";
    static string PathToFile => Path.Combine(Application.persistentDataPath, fileName);

    [System.Serializable]
    class Wrapper
    {
        public List<string> slotIds = new List<string>();
        public List<int> slotCounts = new List<int>();
        public List<string> unlockedRecipes = new List<string>();
    }

    public static void SaveInventory(InventoryManager inv, List<string> unlockedRecipes = null)
    {
        var w = new Wrapper();
        var sd = inv.GetSaveData();
        w.slotIds = sd.slotIds;
        w.slotCounts = sd.slotCounts;
        if (unlockedRecipes != null) w.unlockedRecipes = unlockedRecipes;
        string json = JsonUtility.ToJson(w, true);
        File.WriteAllText(PathToFile, json);
        Debug.Log("Saved inventory to: " + PathToFile);
    }

    public static bool LoadInventory(InventoryManager inv, out List<string> unlockedRecipes)
    {
        unlockedRecipes = new List<string>();
        if (!File.Exists(PathToFile)) return false;
        string json = File.ReadAllText(PathToFile);
        var w = JsonUtility.FromJson<Wrapper>(json);
        var sd = new InventorySaveData(); sd.slotIds = w.slotIds; sd.slotCounts = w.slotCounts;
        inv.LoadFromSave(sd);
        unlockedRecipes = w.unlockedRecipes;
        Debug.Log("Loaded inventory from: " + PathToFile);
        return true;
    }
}
