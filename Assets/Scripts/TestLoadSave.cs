using UnityEngine;

public class TestLoadSave : MonoBehaviour
{
    public InventoryManager inv;
    public RecipeDatabase db;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SaveSystemJSON.SaveInventory(inv);

        if (Input.GetKeyDown(KeyCode.F9))
            SaveSystemJSON.LoadInventory(inv, out var recipes);
    }
}
