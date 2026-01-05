using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private int inventorySize = 24;

    [Header("Runtime")]
    public List<InventorySlot> slots = new List<InventorySlot>();

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        Initialize(inventorySize);
    }

    public void Initialize(int size)
    {
        inventorySize = Mathf.Max(1, size);
        slots = new List<InventorySlot>(inventorySize);
        for (int i = 0; i < inventorySize; i++) slots.Add(new InventorySlot());
        OnInventoryChanged?.Invoke();
    }

    public int TryAddItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return amount;
        int remaining = amount;

        for (int i = 0; i < slots.Count && remaining > 0; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item == item && !s.IsFull)
            {
                int can = item.maxStack - s.amount;
                int add = Mathf.Min(can, remaining);
                s.amount += add;
                remaining -= add;
            }
        }

        for (int i = 0; i < slots.Count && remaining > 0; i++)
        {
            var s = slots[i];
            if (s.IsEmpty)
            {
                int add = Mathf.Min(item.maxStack, remaining);
                s.Set(item, add);
                remaining -= add;
            }
        }

        if (remaining != amount) OnInventoryChanged?.Invoke();
        return remaining;
    }

    public bool RemoveItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (!HasItem(item, amount)) return false;

        int remaining = amount;
        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item == item)
            {
                int take = Mathf.Min(s.amount, remaining);
                s.amount -= take;
                remaining -= take;
                if (s.amount <= 0) s.Clear();
            }
        }

        OnInventoryChanged?.Invoke();
        return remaining == 0;
    }

    public bool HasItem(ItemSO item, int amount)
    {
        if (item == null) return false;
        int total = slots.Where(s => !s.IsEmpty && s.item == item).Sum(s => s.amount);
        return total >= amount;
    }

    public int CountOf(ItemSO item)
    {
        if (item == null) return 0;
        return slots.Where(s => !s.IsEmpty && s.item == item).Sum(s => s.amount);
    }

    public int FindFirstAvailableSlot(ItemSO item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty && slots[i].item == item && !slots[i].IsFull) return i;
        }
        for (int i = 0; i < slots.Count; i++) if (slots[i].IsEmpty) return i;
        return -1;
    }

    public void MoveStack(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= slots.Count) return;
        if (toIndex < 0 || toIndex >= slots.Count) return;

        var from = slots[fromIndex];
        var to = slots[toIndex];

        if (from.IsEmpty) return;

        // same item -> merge
        if (!to.IsEmpty && to.item == from.item)
        {
            int can = to.item.maxStack - to.amount;
            int move = Mathf.Min(can, from.amount);
            to.amount += move;
            from.amount -= move;
            if (from.amount <= 0) from.Clear();
        }
        else if (to.IsEmpty)
        {
            to.Set(from.item, from.amount);
            from.Clear();
        }
        else
        {
            var tmpItem = to.item; var tmpAmount = to.amount;
            to.Set(from.item, from.amount);
            from.Set(tmpItem, tmpAmount);
        }

        OnInventoryChanged?.Invoke();
    }

    public void SplitStack(int index, int amount)
    {
        if (index < 0 || index >= slots.Count) return;
        var s = slots[index];
        if (s.IsEmpty || s.amount <= 1) return;

        int take = Mathf.Clamp(amount, 1, s.amount / 2);
        s.amount -= take;
        TryAddItem(s.item, take);
        OnInventoryChanged?.Invoke();
    }

    public void ClearInventory()
    {
        foreach (var s in slots) s.Clear();
        OnInventoryChanged?.Invoke();
    }

    public void SetSlot(int index, ItemSO item, int amount)
    {
        if (index < 0 || index >= slots.Count) return;
        if (item == null || amount <= 0) slots[index].Clear(); else slots[index].Set(item, amount);
    }

    public InventorySaveData GetSaveData()
    {
        var sd = new InventorySaveData();
        sd.slotIds = new System.Collections.Generic.List<string>();
        sd.slotCounts = new System.Collections.Generic.List<int>();
        foreach (var s in slots)
        {
            sd.slotIds.Add(s.IsEmpty ? "" : s.item.SaveId);
            sd.slotCounts.Add(s.IsEmpty ? 0 : s.amount);
        }
        return sd;
    }

    public void LoadFromSave(InventorySaveData sd)
    {
        if (sd == null) return;
        int total = Mathf.Min(sd.slotIds.Count, slots.Count);
        for (int i = 0; i < total; i++)
        {
            if (string.IsNullOrEmpty(sd.slotIds[i]) || sd.slotCounts[i] <= 0) slots[i].Clear();
            else
            {
                var item = Resources.Load<ItemSO>("Items/" + sd.slotIds[i]);
                if (item != null) slots[i].Set(item, sd.slotCounts[i]); else slots[i].Clear();
            }
        }
        OnInventoryChanged?.Invoke();
    }
}

[System.Serializable]
public class InventorySaveData
{
    public List<string> slotIds;
    public List<int> slotCounts;
}
