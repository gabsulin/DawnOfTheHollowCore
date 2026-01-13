using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Ore, Crystal, BioResidue, Tool, Consumable, CoreFragment, Material, Upgrade }
public enum Rarity { Common, Rare, Epic, Legendary, Mythic }

[CreateAssetMenu(fileName = "New Item", menuName = "DoHC/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 64;
    public ItemType type;
    public Rarity rarity;
    [TextArea(3, 6)] public string description;
    public int value;
    public List<string> tags = new List<string>();

    public string SaveId => name;

    [Header("Upgrade Stats")]
    public float moveSpeedBonus;
    public float miningSpeedMultiplier;
    public float maxHealthBonus;
    public float maxShieldBonus;
    public int damageBonus;
}
