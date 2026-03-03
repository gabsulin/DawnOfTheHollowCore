using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RecipeIngredient
{
    public ItemSO item;
    [Min(1)] public int amount = 1;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "DoHC/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
    public ItemSO output;
    public int outputAmount = 1;
    [TextArea(2, 4)] public string loreText;
    public bool isUnlocked = false;
    public int craftingTier = 1;

    public int TotalIngredientCount
    {
        get
        {
            int total = 0;
            foreach (var ing in ingredients)
                total += ing.amount;
            return total;
        }
    }
}