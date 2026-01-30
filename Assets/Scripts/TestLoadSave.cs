using UnityEngine;

public class TestLoadSave : MonoBehaviour
{
    public InventoryManager inv;
    public RecipeDatabase db;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SaveSystemJSONExtended.SaveGame(inv);

        if (Input.GetKeyDown(KeyCode.F9))
            SaveSystemJSONExtended.LoadGame(inv);
    }
}
