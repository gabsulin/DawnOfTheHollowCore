using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "DoHC/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public List<ItemSO> ingredients = new List<ItemSO>();
    public ItemSO output;
    public int outputAmount = 1;
    [TextArea(2, 4)] public string loreText;
    public bool isUnlocked = false;
    public int craftingTier = 1;
}
