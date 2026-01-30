using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [Header("Recipe Database")]
    public RecipeDatabase recipeDatabase;

    private HashSet<string> unlockedRecipeNames = new HashSet<string>();
    public System.Action OnRecipesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UnlockRecipe(RecipeSO recipe)
    {
        if (recipe == null) return;
        if (!unlockedRecipeNames.Contains(recipe.name))
        {
            unlockedRecipeNames.Add(recipe.name);
            recipe.isUnlocked = true;
            Debug.Log($"[RecipeManager] Unlocked recipe: {recipe.recipeName}");
            OnRecipesChanged?.Invoke();
        }
    }

    public void UnlockRecipes(List<RecipeSO> recipes)
    {
        if (recipes == null || recipes.Count == 0) return;

        bool anyUnlocked = false;
        foreach (var recipe in recipes)
        {
            if (recipe != null && !unlockedRecipeNames.Contains(recipe.name))
            {
                unlockedRecipeNames.Add(recipe.name);
                recipe.isUnlocked = true;
                Debug.Log($"[RecipeManager] Unlocked recipe: {recipe.recipeName}");
                anyUnlocked = true;
            }
        }
        if (anyUnlocked)
        {
            OnRecipesChanged?.Invoke();
        }
    }
    public bool IsRecipeUnlocked(RecipeSO recipe)
    {
        if (recipe == null) return false;
        return unlockedRecipeNames.Contains(recipe.name);
    }
    public List<RecipeSO> GetUnlockedRecipes()
    {
        if (recipeDatabase == null) return new List<RecipeSO>();

        return recipeDatabase.recipes
            .Where(r => r != null && unlockedRecipeNames.Contains(r.name))
            .ToList();
    }
    public List<string> GetUnlockedRecipeNames()
    {
        return new List<string>(unlockedRecipeNames);
    }
    public void LoadUnlockedRecipes(List<string> recipeNames)
    {
        if (recipeNames == null) return;
        unlockedRecipeNames.Clear();

        foreach (var name in recipeNames)
        {
            if (string.IsNullOrEmpty(name)) continue;

            var recipe = recipeDatabase?.recipes.FirstOrDefault(r => r != null && r.name == name);
            if (recipe != null)
            {
                unlockedRecipeNames.Add(name);
                recipe.isUnlocked = true;
            }
        }
        Debug.Log($"[RecipeManager] Loaded {unlockedRecipeNames.Count} unlocked recipes");
        OnRecipesChanged?.Invoke();
    }
    public void ResetAllRecipes()
    {
        unlockedRecipeNames.Clear();
        if (recipeDatabase != null)
        {
            foreach (var recipe in recipeDatabase.recipes)
            {
                if (recipe != null)
                {
                    recipe.isUnlocked = false;
                }
            }
        }
        OnRecipesChanged?.Invoke();
        Debug.Log("[RecipeManager] All recipes reset");
    }
}