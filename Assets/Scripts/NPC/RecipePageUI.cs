using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipePageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject recipeListParent;
    [SerializeField] private GameObject recipeCardPrefab;
    [SerializeField] private TMP_Text noRecipesText;

    [Header("Page Toggle")]
    [SerializeField] private GameObject inventoryPage;
    [SerializeField] private GameObject recipePage;
    [SerializeField] private Button showRecipesButton;
    [SerializeField] private Button showInventoryButton;

    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Start()
    {
        if (showRecipesButton != null)
            showRecipesButton.onClick.AddListener(ShowRecipePage);
        if (showInventoryButton != null)
            showInventoryButton.onClick.AddListener(ShowInventoryPage);
        if (RecipeManager.Instance != null)
            RecipeManager.Instance.OnRecipesChanged += RefreshRecipeDisplay;

        ShowInventoryPage();
    }
    private void OnDestroy()
    {
        if (RecipeManager.Instance != null)
            RecipeManager.Instance.OnRecipesChanged -= RefreshRecipeDisplay;
    }

    public void ShowRecipePage()
    {
        if (inventoryPage != null)
            inventoryPage.SetActive(false);
        if (recipePage != null)
            recipePage.SetActive(true);
        RefreshRecipeDisplay();
    }
    public void ShowInventoryPage()
    {
        if (recipePage != null)
            recipePage.SetActive(false);
        if (inventoryPage != null)
            inventoryPage.SetActive(true);
    }
    private void RefreshRecipeDisplay()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null)
                Destroy(card);
        }
        spawnedCards.Clear();

        if (RecipeManager.Instance == null)
        {
            ShowNoRecipesMessage(true);
            return;
        }
        List<RecipeSO> unlockedRecipes = RecipeManager.Instance.GetUnlockedRecipes();
        if (unlockedRecipes.Count == 0)
        {
            ShowNoRecipesMessage(true);
            return;
        }
        ShowNoRecipesMessage(false);

        foreach (var recipe in unlockedRecipes)
        {
            if (recipe == null) continue;
            GameObject card = Instantiate(recipeCardPrefab, recipeListParent.transform);
            spawnedCards.Add(card);
            SetupRecipeCard(card, recipe);
        }
    }
    private void SetupRecipeCard(GameObject card, RecipeSO recipe)
    {
        var outputImage = card.transform.Find("OutputImage")?.GetComponent<Image>();
        var outputText = card.transform.Find("OutputText")?.GetComponent<TMP_Text>();
        var ingredientsParent = card.transform.Find("IngredientsPanel");
        var ingredientSlotPrefab = card.transform.Find("IngredientSlotPrefab")?.gameObject;

        if (outputImage != null && recipe.output != null)
        {
            outputImage.sprite = recipe.output.icon;
            outputImage.enabled = true;
        }
        if (outputText != null)
        {
            string outputName = recipe.output != null ? recipe.output.itemName : recipe.recipeName;
            outputText.text = $"{outputName} x {recipe.outputAmount}";
        }
        if (ingredientsParent != null && ingredientSlotPrefab != null)
        {
            ingredientSlotPrefab.SetActive(false);
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient == null) continue;
                GameObject slot = Instantiate(ingredientSlotPrefab, ingredientsParent);
                slot.SetActive(true);

                var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                var count = slot.transform.Find("Count")?.GetComponent<TMP_Text>();

                if (icon != null)
                {
                    icon.sprite = ingredient.icon;
                    icon.enabled = true;
                }

                if (count != null)
                {
                    /*count.text = $"x1 {ingredient.amount}";*/
                    count.text = $"x1";
                }
            }
        }
    }
    private void ShowNoRecipesMessage(bool show)
    {
        if (noRecipesText != null)
            noRecipesText.gameObject.SetActive(show);
    }
}