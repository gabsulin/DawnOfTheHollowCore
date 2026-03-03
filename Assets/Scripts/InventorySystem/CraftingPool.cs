using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CraftingPool : MonoBehaviour
{
    [Header("References")]
    public RecipeDatabase recipeDatabase;
    public InventoryManager inventoryManager;

    [Header("Pool")]
    public int maxPoolItems = 9;
    [SerializeField] private List<ItemSO> poolItems = new List<ItemSO>();
    public IReadOnlyList<ItemSO> CurrentItems => poolItems;

    [Header("World Drop (overflow)")]
    [SerializeField] private GameObject worldItemPrefab;

    [Header("VFX")]
    public ParticleSystem acceptVfx;
    public ParticleSystem rejectVfx;
    public Animator coreAnimator;

    public System.Action<ItemSO> OnItemAdded;
    public System.Action<bool, ItemSO> OnCraftResult;

    public bool AddItem(ItemSO item)
    {
        if (poolItems.Count >= maxPoolItems)
        {
            Debug.Log("[CRAFTING] Pool full — cannot add item");
            return false;
        }
        poolItems.Add(item);
        Debug.Log($"[CRAFTING] Added item: {item.itemName} (Pool now has {poolItems.Count} items)");
        OnItemAdded?.Invoke(item);
        return true;
    }

    public bool TryAcceptItem(ItemSO item)
    {
        if (item == null) return false;
        return AddItem(item);
    }

    public void AttemptCraft()
    {
        var match = FindBestMatchingRecipe();
        if (match != null)
        {
            Debug.Log($"[CRAFTING] Recipe matched! Crafting {match.outputAmount}x {match.output.itemName}");
            Consume(match);
            GiveOutput(match.output, match.outputAmount);
            OnCraftResult?.Invoke(true, match.output);
        }
        else
        {
            Debug.Log("[CRAFTING] No recipe matched.");
            OnCraftResult?.Invoke(false, null);
        }
    }

    public void ReturnAllToInventory()
    {
        if (inventoryManager == null)
        {
            poolItems.Clear();
            return;
        }

        foreach (var it in poolItems)
        {
            int leftover = inventoryManager.TryAddItem(it, 1);
            if (leftover > 0)
            {
                SpawnWorldDrop(it, leftover);
            }
        }

        poolItems.Clear();
    }

    public List<ItemSO> GetPoolContents() => new List<ItemSO>(poolItems);
    RecipeSO FindBestMatchingRecipe()
    {
        var candidates = recipeDatabase.recipes
            .Where(r => r != null && r.isUnlocked && RecipeMatches(r, poolItems))
            .OrderByDescending(r => r.TotalIngredientCount)
            .ToList();

        return candidates.Count > 0 ? candidates[0] : null;
    }

    bool RecipeMatches(RecipeSO r, List<ItemSO> pool)
    {
        var need = new Dictionary<string, int>();
        foreach (var ing in r.ingredients)
        {
            if (ing.item == null) continue;
            if (!need.ContainsKey(ing.item.SaveId)) need[ing.item.SaveId] = 0;
            need[ing.item.SaveId] += ing.amount;
        }

        var present = new Dictionary<string, int>();
        foreach (var p in pool)
        {
            if (!present.ContainsKey(p.SaveId)) present[p.SaveId] = 0;
            present[p.SaveId]++;
        }

        foreach (var kv in need)
        {
            if (!present.ContainsKey(kv.Key) || present[kv.Key] < kv.Value)
                return false;
        }
        return true;
    }

    void Consume(RecipeSO recipe)
    {
        Debug.Log("[CRAFTING] Consuming ingredients…");

        var need = new Dictionary<string, int>();
        foreach (var ing in recipe.ingredients)
        {
            if (ing.item == null) continue;
            if (!need.ContainsKey(ing.item.SaveId)) need[ing.item.SaveId] = 0;
            need[ing.item.SaveId] += ing.amount;
        }

        for (int i = poolItems.Count - 1; i >= 0; i--)
        {
            var id = poolItems[i].SaveId;
            if (need.ContainsKey(id) && need[id] > 0)
            {
                need[id]--;
                poolItems.RemoveAt(i);
            }
        }
    }

    void GiveOutput(ItemSO output, int amount)
    {
        int leftover = inventoryManager.TryAddItem(output, amount);
        if (leftover > 0)
        {
            Debug.Log($"[CRAFTING] Inventory full — dropping {leftover}x {output.itemName} in world");
            SpawnWorldDrop(output, leftover);
        }
    }

    void SpawnWorldDrop(ItemSO item, int amount)
    {
        if (worldItemPrefab != null)
        {
            Vector3 offset = (Vector3)(Random.insideUnitCircle * 0.5f);
            GameObject go = Instantiate(worldItemPrefab, transform.position + offset, Quaternion.identity);
            var wi = go.GetComponent<WorldItem>();
            if (wi != null)
            {
                wi.Initialize(item, amount);
                wi.ApplyPickupDelay();
            }
        }
        else
        {
            Debug.LogWarning($"[CRAFTING] worldItemPrefab not assigned — spawning fallback sprite for {item.itemName}");
            var go = new GameObject(item.name + "_drop");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = item.icon;
            go.transform.position = transform.position + Vector3.up * 0.5f;
        }
    }
}