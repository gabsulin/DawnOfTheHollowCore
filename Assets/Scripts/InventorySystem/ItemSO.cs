using UnityEngine;

public enum ItemType { Ore, Crystal, BioResidue, Tool, Consumable, CoreFragment, Material, Upgrade }
public enum Rarity { Common, Rare, Epic, Legendary, Mythic }

[CreateAssetMenu(fileName = "New Item", menuName = "InventorySystem/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 64;
    public ItemType type;
    public Rarity rarity;
    [TextArea(3, 6)] public string description;

    public string SaveId => name;

    [Header("Upgrade Stats")]
    public float moveSpeedBonus;
    public float miningSpeedMultiplier;
    public float maxHealthBonus;
    public float maxShieldBonus;
    public float damageBonus;
    [Range(0f, 1f)] public float dashCooldownReduction;
    [Header("Consumable Stats")]
    public float healAmount;
    public float coreHealAmount;
}
