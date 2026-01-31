using UnityEngine;
using System.Collections.Generic;
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
            Destroy(gameObject);
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
        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("[RecipeManager] Tried to unlock recipes but list was null or empty!");
            return;
        }

        Debug.Log($"[RecipeManager] Attempting to unlock {recipes.Count} recipes...");

        bool anyUnlocked = false;
        foreach (var recipe in recipes)
        {
            if (recipe == null)
            {
                Debug.LogWarning("[RecipeManager] Null recipe in list, skipping...");
                continue;
            }

            Debug.Log($"[RecipeManager] Processing recipe: {recipe.recipeName} (asset name: {recipe.name})");

            if (!unlockedRecipeNames.Contains(recipe.name))
            {
                unlockedRecipeNames.Add(recipe.name);
                recipe.isUnlocked = true;
                Debug.Log($"[RecipeManager] UNLOCKED: {recipe.recipeName}");
                anyUnlocked = true;
            }
            else
            {
                Debug.Log($"[RecipeManager] Already unlocked: {recipe.recipeName}");
            }
        }

        if (anyUnlocked)
        {
            Debug.Log($"[RecipeManager] Invoking OnRecipesChanged event. Total unlocked: {unlockedRecipeNames.Count}");
            OnRecipesChanged?.Invoke();
        }
        else
        {
            Debug.Log("[RecipeManager] No new recipes were unlocked.");
        }
    }
    public bool IsRecipeUnlocked(RecipeSO recipe)
    {
        if (recipe == null) return false;
        return unlockedRecipeNames.Contains(recipe.name);
    }
    public List<RecipeSO> GetUnlockedRecipes()
    {
        if (recipeDatabase == null)
        {
            Debug.LogError("[RecipeManager] RecipeDatabase is NULL! Cannot get unlocked recipes.");
            return new List<RecipeSO>();
        }

        Debug.Log($"[RecipeManager] Getting unlocked recipes. Database has {recipeDatabase.recipes.Count} total recipes.");
        Debug.Log($"[RecipeManager] Currently {unlockedRecipeNames.Count} recipe names are marked as unlocked:");
        foreach (var name in unlockedRecipeNames)
        {
            Debug.Log($"  - {name}");
        }

        var unlockedList = recipeDatabase.recipes
            .Where(r => r != null && unlockedRecipeNames.Contains(r.name))
            .ToList();

        Debug.Log($"[RecipeManager] Found {unlockedList.Count} matching recipes in database:");
        foreach (var recipe in unlockedList)
        {
            Debug.Log($"{recipe.recipeName} ({recipe.name})");
        }

        return unlockedList;
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
                    recipe.isUnlocked = false;
            }
        }

        OnRecipesChanged?.Invoke();
        Debug.Log("[RecipeManager] All recipes reset");
    }
}