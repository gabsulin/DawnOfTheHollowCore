using UnityEngine;
using System.Collections.Generic;

public enum ItemType { Ore, Crystal, BioResidue, Tool, Consumable, CoreFragment, Material }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

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

    // For save lookup convenience, use asset name by default
    public string SaveId => name;
}
