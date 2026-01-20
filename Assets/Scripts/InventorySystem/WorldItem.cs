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

    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    private int amount = 1;

    private Vector3 startPos;
    private float pickupAvailableTime = 0f;
    private float timeOffset;
    private InventoryManager cachedInventory;
    private PolygonCollider2D polyCollider;

    private void Awake()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        RegenerateColliderFromSprite();

        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Start()
    {
        // Fallback: if Initialize() wasn't called, set startPos here
        if (startPos == Vector3.zero)
        {
            startPos = transform.position;
        }
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
        float initialY = startPos.y + Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatAmplitude;
        transform.position = new Vector3(startPos.x, initialY, startPos.z);

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
            float y = startPos.y + Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatAmplitude;
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
        if (cachedInventory == null)
        {
            cachedInventory = player.GetComponent<InventoryManager>();
            if (cachedInventory == null)
            {
                cachedInventory = FindFirstObjectByType<InventoryManager>();
            }
        }

        if (cachedInventory != null)
        {
            int leftover = cachedInventory.TryAddItem(item, amount);
            if (leftover == 0)
            {
                Destroy(gameObject);
            }
            else if (leftover < amount)
            {
                amount = leftover;
            }
        }
    }

    private void RegenerateColliderFromSprite()
    {
        if (polyCollider == null || iconRenderer == null || iconRenderer.sprite == null)
            return;

        int shapeCount = iconRenderer.sprite.GetPhysicsShapeCount();
        if (shapeCount == 0)
            return;

        polyCollider.pathCount = shapeCount;
        List<Vector2> path = new List<Vector2>();

        for (int i = 0; i < shapeCount; i++)
        {
            path.Clear();
            iconRenderer.sprite.GetPhysicsShape(i, path);
            polyCollider.SetPath(i, path);
        }
    }
}