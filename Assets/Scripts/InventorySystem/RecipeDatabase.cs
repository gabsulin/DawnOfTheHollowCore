using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Recipe Database", menuName = "InventorySystem/RecipeDatabase")]
public class RecipeDatabase : ScriptableObject
{
    public List<RecipeSO> recipes = new List<RecipeSO>();
}
