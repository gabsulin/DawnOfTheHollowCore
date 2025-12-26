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
        //if (acceptVfx) acceptVfx.Play();
        return true;
    }
    public bool TryAcceptItem(ItemSO item)
    {
        if (item == null) return false;
        return AddItem(item);
    }
    public void AttemptCraft()
    {
        var match = FindMatchingRecipe();
        if (match != null)
        {
            Debug.Log($"[CRAFTING] Recipe matched! Crafting {match.outputAmount}x {match.output.itemName}");
            Consume(match);
            GiveOutput(match.output, match.outputAmount);
            OnCraftResult?.Invoke(true, match.output);
            //if (coreAnimator) coreAnimator.SetTrigger("Craft");
        }
        else
        {
            Debug.Log("[CRAFTING] No recipe matched.");
            OnCraftResult?.Invoke(false, null);
            //if (coreAnimator) coreAnimator.SetTrigger("Reject");
            //if (rejectVfx) rejectVfx.Play();
        }
    }

    RecipeSO FindMatchingRecipe()
    {
        foreach (var r in recipeDatabase.recipes)
        {
            if (!r.isUnlocked) continue;
            if (RecipeMatches(r, poolItems)) return r;
        }
        return null;
    }

    bool RecipeMatches(RecipeSO r, List<ItemSO> pool)
    {
        var need = new Dictionary<string, int>();
        foreach (var ing in r.ingredients)
        {
            if (!need.ContainsKey(ing.SaveId)) need[ing.SaveId] = 0;
            need[ing.SaveId]++;
        }
        var present = new Dictionary<string, int>();
        foreach (var p in pool)
        {
            if (!present.ContainsKey(p.SaveId)) present[p.SaveId] = 0;
            present[p.SaveId]++;
        }
        foreach (var kv in need)
        {
            if (!present.ContainsKey(kv.Key) || present[kv.Key] < kv.Value) return false;
        }
        return true;
    }

    void Consume(RecipeSO recipe)
    {
        Debug.Log("[CRAFTING] Consuming ingredients…");
        var need = new Dictionary<string, int>();
        foreach (var ing in recipe.ingredients)
        {
            if (!need.ContainsKey(ing.SaveId)) need[ing.SaveId] = 0;
            need[ing.SaveId]++;
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
            // spawn in world at pool position
            var go = new GameObject(output.name + "_drop");
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = output.icon;
            go.transform.position = transform.position + Vector3.up * 1f;
        }
    }

    public void ReturnAllToInventory()
    {
        if (inventoryManager == null) return;
        foreach (var it in poolItems) inventoryManager.TryAddItem(it, 1);
        poolItems.Clear();
    }

    public List<ItemSO> GetPoolContents() => new List<ItemSO>(poolItems);
}
