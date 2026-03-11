using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [Header("World Drop")]
    [SerializeField] private GameObject worldItemPrefab;

    private PlayerCraftingSensor craftingSensor;

    private void Awake()
    {
        craftingSensor = FindFirstObjectByType<PlayerCraftingSensor>();
    }

    public void DropAtCursor(ItemSO item, int amount)
    {
        if (item == null) return;

        if (craftingSensor != null && craftingSensor.currentPool != null)
        {
            Debug.Log($"[CRAFTING POOL] Dropping {amount}x {item.itemName} into crafting pool");

            for (int i = 0; i < amount; i++)
            {
                bool accepted = craftingSensor.currentPool.TryAcceptItem(item);
                if (!accepted)
                {
                    Debug.Log("[CRAFTING POOL] Pool full — stopped early");
                    break;
                }
            }

            return;
        }

        if (worldItemPrefab == null) return;

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;

        GameObject go = Instantiate(worldItemPrefab, pos, Quaternion.identity);

        var wi = go.GetComponent<WorldItem>();
        if (wi != null)
        {
            wi.Initialize(item, amount);
            wi.ApplyPickupDelay();
        }
        AudioManager.Instance?.PlaySFX("ItemDrop");
    }
}