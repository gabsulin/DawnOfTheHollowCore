using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class WorldItem : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.12f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupDelay = 0.8f;
    private float pickupAvailableTime = 0f;

    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    private int amount = 1;

    private Vector3 startPos;

    private void Awake()
    {
        RegenerateColliderFromSprite();
    }

    public void Initialize(ItemSO itemData, int itemAmount)
    {
        item = itemData;
        amount = itemAmount;

        if (iconRenderer != null && item != null)
        {
            iconRenderer.sprite = item.icon;
            RegenerateColliderFromSprite();
        }

        startPos = transform.position;

        pickupAvailableTime = Time.time;
    }

    public void ApplyPickupDelay()
    {
        pickupAvailableTime = Time.time + pickupDelay;
    }

    private void Update()
    {
        if (item != null)
        {
            float y = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < pickupAvailableTime)
            return;

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
            if (leftover == 0)
                Destroy(gameObject);
        }
    }

    private void RegenerateColliderFromSprite()
    {
        var poly = GetComponent<PolygonCollider2D>();
        if (poly == null) return;

        var sr = iconRenderer;
        if (sr == null || sr.sprite == null) return;

        poly.pathCount = sr.sprite.GetPhysicsShapeCount();

        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < poly.pathCount; i++)
        {
            path.Clear();
            sr.sprite.GetPhysicsShape(i, path);
            poly.SetPath(i, path.ToArray());
        }
    }
}
