using UnityEngine;

/// <summary>
/// Creates a sprite-based energy barrier around locked areas
/// Can be attached to a GameObject with SpriteRenderer
/// </summary>
public class AreaEnergyBarrier : MonoBehaviour
{
    [Header("Barrier Appearance")]
    [SerializeField] private Color barrierColor = new Color(1f, 0.2f, 0f, 0.6f); // Semi-transparent red-orange
    [SerializeField] private float barrierRadius = 10f;
    [SerializeField] private int segmentCount = 32; // How many sprites form the circle

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = 10;
    [SerializeField] private string sortingLayerName = "Default";

    [Header("Animation")]
    [SerializeField] private bool shimmerEffect = true;
    [SerializeField] private float shimmerSpeed = 3f;
    [SerializeField] private float shimmerIntensity = 0.2f;

    [SerializeField] private bool rotateBarrier = false;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;

    private GameObject[] barrierSegments;
    private SpriteRenderer[] segmentRenderers;
    private float originalAlpha;
    private Vector3 originalScale;

    private void Start()
    {
        // Use the barrierRadius that might have been set externally
        CreateBarrier();
        originalAlpha = barrierColor.a;
        originalScale = transform.localScale;

        Debug.Log($"[AreaEnergyBarrier] Barrier created with radius {barrierRadius}");
    }

    private void CreateBarrier()
    {
        barrierSegments = new GameObject[segmentCount];
        segmentRenderers = new SpriteRenderer[segmentCount];

        float angleStep = 360f / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * barrierRadius, // Uses the set radius
                Mathf.Sin(angle) * barrierRadius,
                0f
            );

            // Create segment
            GameObject segment = new GameObject($"BarrierSegment_{i}");
            segment.transform.SetParent(transform);
            segment.transform.localPosition = position;
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);

            // Add sprite renderer
            SpriteRenderer sr = segment.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBarrierSprite();
            sr.color = barrierColor;
            sr.sortingOrder = sortingOrder; // Use configurable sorting order
            sr.sortingLayerName = sortingLayerName;

            // Scale to connect segments
            float segmentLength = 2f * Mathf.PI * barrierRadius / segmentCount;
            segment.transform.localScale = new Vector3(segmentLength, 0.5f, 1f);

            barrierSegments[i] = segment;
            segmentRenderers[i] = sr;
        }
    }

    private Sprite CreateBarrierSprite()
    {
        // Use Unity's built-in white sprite (always available)
        // First try the Resources built-in
        Sprite sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        if (sprite == null)
        {
            // Fallback: create a simple white square texture
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f),
                1f
            );
        }

        return sprite;
    }

    private void Update()
    {
        // Rotation effect
        if (rotateBarrier)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // Shimmer effect (alpha pulsing)
        if (shimmerEffect && segmentRenderers != null)
        {
            float shimmer = Mathf.Sin(Time.time * shimmerSpeed) * shimmerIntensity;
            float newAlpha = Mathf.Clamp01(originalAlpha + shimmer);

            foreach (var sr in segmentRenderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = newAlpha;
                    sr.color = c;
                }
            }
        }

        // Pulse effect (scale pulsing)
        if (pulseEffect)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }

    /// <summary>
    /// Set the radius of the barrier (should match death zone radius)
    /// </summary>
    public void SetRadius(float radius)
    {
        barrierRadius = radius;

        Debug.Log($"[AreaEnergyBarrier] SetRadius called with {radius}. Segments exist: {barrierSegments != null}");

        // Reposition all segments if they exist
        if (barrierSegments != null && barrierSegments.Length > 0)
        {
            float angleStep = 360f / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                if (barrierSegments[i] == null) continue;

                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * barrierRadius,
                    Mathf.Sin(angle) * barrierRadius,
                    0f
                );

                barrierSegments[i].transform.localPosition = position;

                // Rescale segments to fit new radius
                float segmentLength = 2f * Mathf.PI * barrierRadius / segmentCount;
                barrierSegments[i].transform.localScale = new Vector3(segmentLength, 0.5f, 1f);
            }

            Debug.Log($"[AreaEnergyBarrier] Repositioned {barrierSegments.Length} segments to radius {radius}");
        }
        else
        {
            Debug.Log($"[AreaEnergyBarrier] Radius {radius} stored, will be applied when barrier is created");
        }
    }

    /// <summary>
    /// Change the barrier color
    /// </summary>
    public void SetColor(Color color)
    {
        barrierColor = color;
        originalAlpha = color.a;

        if (segmentRenderers != null)
        {
            foreach (var sr in segmentRenderers)
            {
                if (sr != null)
                {
                    sr.color = color;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up created segments
        if (barrierSegments != null)
        {
            foreach (var segment in barrierSegments)
            {
                if (segment != null)
                {
                    Destroy(segment);
                }
            }
        }
    }
}