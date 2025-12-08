using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.12f;

    private ItemSO item;
    private int amount;
    private Vector3 startPos;

    public void Initialize(ItemSO itemData, int itemAmount)
    {
        item = itemData; amount = itemAmount;
        if (iconRenderer != null && item != null) iconRenderer.sprite = item.icon;
        startPos = transform.position;
    }

    private void Update()
    {
        // aesthetic bob
        if (item != null)
        {
            float y = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryPickup(other.gameObject);
        }
    }

    public void TryPickup(GameObject player)
    {
        var inv = player.GetComponent<InventoryManager>();
        if (inv == null) inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            int leftover = inv.TryAddItem(item, amount);
            if (leftover == 0) Destroy(gameObject);
        }
    }
}
