using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Image))]
public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TMP_Text amountText;
    public Image borderImage;

    InventoryManager inventoryManager;
    InventoryUI parentUI;
    public int slotIndex;

    private bool isHovering = false;

    public void Initialize(InventoryManager inv, int index, InventoryUI parent)
    {
        inventoryManager = inv;
        slotIndex = index;
        parentUI = parent;
    }

    public void Refresh(InventorySlot data)
    {
        if (data.IsEmpty)
        {
            iconImage.enabled = false;
            amountText.text = "";
            borderImage.enabled = false;

            ForceTooltipRefresh();
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = data.item.icon;

        amountText.text = data.amount > 1 ? data.amount.ToString() : "";
        borderImage.enabled = true;
        borderImage.color = GetRarityColor(data.item);

        ForceTooltipRefresh();
    }

    private void ForceTooltipRefresh()
    {
        if (!isHovering) return;

        var slot = inventoryManager.slots[slotIndex];

        if (!slot.IsEmpty)
            Tooltip.Instance.Show(slot.item);
        else
            Tooltip.Instance.Hide();
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
            var dropper = inventoryManager.GetComponent<ItemDropper>();
            if (dropper != null)
                dropper.DropAtCursor(slot.item, 1);

            slot.amount -= 1;
            if (slot.amount <= 0) slot.Clear();

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

        ForceTooltipRefresh();
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var result = eventData.pointerCurrentRaycast.gameObject;

        if (result == null)
        {
            DropStackAtCursor();
            return;
        }

        var otherSlot = result.GetComponent<InventorySlotUI>();

        if (otherSlot != null && otherSlot.inventoryManager == inventoryManager)
        {
            HandleSlotDrop(otherSlot);
            return;
        }

        DropStackAtCursor();
    }

    private void DropStackAtCursor()
    {
        if (MouseItemSlot.Instance.item != null)
        {
            var dropper = inventoryManager.GetComponent<ItemDropper>();
            if (dropper != null)
                dropper.DropAtCursor(MouseItemSlot.Instance.item, MouseItemSlot.Instance.amount);

            MouseItemSlot.Instance.Clear();
            parentUI.RefreshAll();
        }

        ForceTooltipRefresh();
    }

    private void HandleSlotDrop(InventorySlotUI otherSlot)
    {
        var target = inventoryManager.slots[otherSlot.slotIndex];
        var heldItem = MouseItemSlot.Instance.item;
        var heldAmount = MouseItemSlot.Instance.amount;

        if (!target.IsEmpty && target.item == heldItem)
        {
            int can = target.item.maxStack - target.amount;
            int move = Mathf.Min(can, heldAmount);

            target.amount += move;
            heldAmount -= move;

            if (heldAmount > 0)
            {
                int idx = inventoryManager.FindFirstAvailableSlot(heldItem);
                if (idx >= 0)
                    inventoryManager.slots[idx].Set(heldItem, heldAmount);
            }
        }
        else
        {
            var tmpItem = target.item;
            var tmpAmt = target.amount;

            target.Set(heldItem, heldAmount);
            inventoryManager.slots[slotIndex].Set(tmpItem, tmpAmt);
        }

        MouseItemSlot.Instance.Clear();
        parentUI.RefreshAll();

        ForceTooltipRefresh();
        otherSlot.ForceTooltipRefresh();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        var slot = inventoryManager.slots[slotIndex];
        if (!slot.IsEmpty)
            Tooltip.Instance.Show(slot.item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        Tooltip.Instance.Hide();
    }
}
