using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int amount;

    public InventorySlot() { Clear(); }

    public bool IsEmpty => item == null || amount <= 0;
    public bool IsFull => !IsEmpty && amount >= item.maxStack;

    public int FreeSpace => IsEmpty ? (item != null ? item.maxStack : 0) : (item.maxStack - amount);

    public void Set(ItemSO newItem, int newAmount)
    {
        item = newItem; amount = newAmount;
    }

    public void Clear() { item = null; amount = 0; }
}
