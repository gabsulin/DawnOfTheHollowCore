using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Image))]
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image iconImage;
    public TMP_Text amountText;
    public Image borderImage;

    InventoryManager inventoryManager;
    InventoryUI parentUI;
    public int slotIndex;

    public void Initialize(InventoryManager inv, int index, InventoryUI parent)
    {
        inventoryManager = inv; slotIndex = index; parentUI = parent;
    }

    public void Refresh(InventorySlot data)
    {
        if (data.IsEmpty)
        {
            iconImage.enabled = false; amountText.text = ""; borderImage.enabled = false; return;
        }
        iconImage.enabled = true; iconImage.sprite = data.item.icon;
        amountText.text = data.amount > 1 ? data.amount.ToString() : "";
        borderImage.enabled = true; borderImage.color = GetRarityColor(data.item);
    }

    Color GetRarityColor(ItemSO it)
    {
        switch (it.rarity)
        {
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return Color.cyan;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Legendary: return new Color(1f, 0.6f, 0f);
            case Rarity.Mythic: return Color.yellow;
            default: return Color.white;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = inventoryManager.slots[slotIndex];
        if (eventData.button == PointerEventData.InputButton.Right && !slot.IsEmpty)
        {
            // drop single
            var dropper = inventoryManager.GetComponent<ItemDropper>();
            if (dropper != null) dropper.Drop(slot.item, 1, inventoryManager.transform.position + Vector3.down * 0.5f);
            slot.amount -= 1; if (slot.amount <= 0) slot.Clear();
            parentUI.RefreshAll();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = inventoryManager.slots[slotIndex];
        if (slot.IsEmpty) return;
        MouseItemSlot.Instance.Set(slot.item, slot.amount);
        slot.Clear();
        parentUI.RefreshAll();
    }

    public void OnDrag(PointerEventData eventData) { /* visuals handled by MouseItemSlot */ }

    public void OnEndDrag(PointerEventData eventData)
    {
        var result = eventData.pointerCurrentRaycast.gameObject;
        if (result == null)
        {
            // dropped outside -> drop to world
            if (MouseItemSlot.Instance.item != null)
            {
                var dropper = inventoryManager.GetComponent<ItemDropper>();
                if (dropper != null) dropper.Drop(MouseItemSlot.Instance.item, MouseItemSlot.Instance.amount, inventoryManager.transform.position + Vector3.down * 0.5f);
                MouseItemSlot.Instance.Clear();
                parentUI.RefreshAll();
            }
            return;
        }

        var otherSlot = result.GetComponent<InventorySlotUI>();
        if (otherSlot != null && otherSlot.inventoryManager == inventoryManager)
        {
            // merge or swap
            var target = inventoryManager.slots[otherSlot.slotIndex];
            if (!target.IsEmpty && target.item == MouseItemSlot.Instance.item)
            {
                int can = target.item.maxStack - target.amount;
                int move = Mathf.Min(can, MouseItemSlot.Instance.amount);
                target.amount += move;
                MouseItemSlot.Instance.amount -= move;
                if (MouseItemSlot.Instance.amount > 0)
                {
                    // put remainder back to original (find slot)
                    int idx = inventoryManager.FindFirstAvailableSlot(MouseItemSlot.Instance.item);
                    if (idx >= 0) inventoryManager.slots[idx].Set(MouseItemSlot.Instance.item, MouseItemSlot.Instance.amount);
                }
            }
            else
            {
                // swap with target
                var tmpItem = target.item; var tmpAmt = target.amount;
                target.Set(MouseItemSlot.Instance.item, MouseItemSlot.Instance.amount);
                var originalSlotIndex = slotIndex; // note: original slot is emptied already
                inventoryManager.slots[originalSlotIndex].Set(tmpItem, tmpAmt);
            }
            MouseItemSlot.Instance.Clear();
            parentUI.RefreshAll();
            return;
        }

        // dropped on non-slot (e.g. world) -> drop
        if (MouseItemSlot.Instance.item != null)
        {
            var dropper = inventoryManager.GetComponent<ItemDropper>();
            if (dropper != null) dropper.Drop(MouseItemSlot.Instance.item, MouseItemSlot.Instance.amount, inventoryManager.transform.position + Vector3.down * 0.5f);
            MouseItemSlot.Instance.Clear();
            parentUI.RefreshAll();
        }
    }
}
