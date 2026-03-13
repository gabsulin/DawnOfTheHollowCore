using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public GameObject slotPrefab;
    public Transform gridParent;
    public RectTransform slotGridRect;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private void Start()
    {
        BuildUI();
        inventoryManager.OnInventoryChanged += RefreshAll;
    }

    public void BuildUI()
    {
        foreach (Transform t in gridParent) Destroy(t.gameObject);
        uiSlots.Clear();
        for (int i = 0; i < inventoryManager.slots.Count; i++)
        {
            var go = Instantiate(slotPrefab, gridParent);
            var ui = go.GetComponent<InventorySlotUI>();
            ui.Initialize(inventoryManager, i, this);
            uiSlots.Add(ui);
        }
        RefreshAll();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].Refresh(inventoryManager.slots[i]);
        }
    }
}
