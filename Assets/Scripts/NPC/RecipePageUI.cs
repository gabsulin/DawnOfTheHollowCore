using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        Image outputImage = null;
        TMP_Text outputText = null;

        var outputImageTransform = card.transform.Find("OutputImage")
                                ?? card.transform.Find("OutputSection/OutputImage");
        if (outputImageTransform != null)
            outputImage = outputImageTransform.GetComponent<Image>();

        var outputTextTransform = card.transform.Find("OutputText")
                               ?? card.transform.Find("OutputSection/OutputText");
        if (outputTextTransform != null)
            outputText = outputTextTransform.GetComponent<TMP_Text>();

        if (outputImage != null && recipe.output != null)
        {
            outputImage.sprite = recipe.output.icon;
            outputImage.enabled = true;
            outputImage.color = Color.white;
        }

        if (outputText != null)
        {
            string outputName = recipe.output != null ? recipe.output.itemName : recipe.recipeName;
            outputText.text = $"{outputName} x{recipe.outputAmount}";
        }

        Transform ingredientsParent = card.transform.Find("IngredientsPanel");
        GameObject ingredientSlotPrefab = null;

        if (ingredientsParent != null)
        {
            var slotTransform = ingredientsParent.Find("IngredientSlotPrefab");
            if (slotTransform != null)
                ingredientSlotPrefab = slotTransform.gameObject;
        }

        Debug.Log($"[RecipeCard] Setting up: {recipe.recipeName}");
        Debug.Log($"  - Output Image found: {outputImage != null}");
        Debug.Log($"  - Output Text found: {outputText != null}");
        Debug.Log($"  - Ingredients Parent found: {ingredientsParent != null}");
        Debug.Log($"  - Ingredient Slot Prefab found: {ingredientSlotPrefab != null}");

        if (ingredientsParent != null && ingredientSlotPrefab != null)
        {
            ingredientSlotPrefab.SetActive(false);

            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient == null || ingredient.item == null) continue;

                GameObject slot = Instantiate(ingredientSlotPrefab, ingredientsParent);
                slot.SetActive(true);

                var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                var count = slot.transform.Find("Count")?.GetComponent<TMP_Text>();

                if (icon != null)
                {
                    icon.sprite = ingredient.item.icon;
                    icon.enabled = true;
                    icon.color = Color.white;
                }

                if (count != null)
                {
                    count.text = $"x{ingredient.amount}";
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