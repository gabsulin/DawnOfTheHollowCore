using UnityEngine;

public class MapFogBoundary : MonoBehaviour
{
    [Header("References")]
    public AreaManager areaManager;

    [Header("Fog Appearance")]
    [Tooltip("Color of the fog/mist")]
    public Color fogColor = new Color(0.85f, 0.90f, 0.95f, 1f);

    [Tooltip("How many world units inward the fog starts fading in from the boundary")]
    public float fogWidth = 8f;

    [Tooltip("How far beyond the boundary the fog extends (fully opaque). Covers the void.)")]
    public float fogOvershoot = 15f;

    [Header("Quality")]
    [Range(128, 1024)]
    [Tooltip("Texture resolution. 512 is more than enough for a smooth gradient.")]
    public int textureSize = 512;

    void Start()
    {
        BuildFog();
    }

    void BuildFog()
    {
        float boundaryRadius = GetOutermostRadius();
        float totalRadius = boundaryRadius + fogOvershoot;

        Texture2D tex = GenerateFogTexture(boundaryRadius, totalRadius);

        float spritePPU = textureSize / (totalRadius * 2f);
        Sprite fogSprite = Sprite.Create(
            tex,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            spritePPU
        );

        GameObject fogObj = new GameObject("MapFogBoundary");
        fogObj.transform.SetParent(transform, false);
        fogObj.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = fogObj.AddComponent<SpriteRenderer>();
        sr.sprite = fogSprite;
        sr.color = fogColor;
        sr.sortingLayerName = GetTopSortingLayer();
        sr.sortingOrder = 9999;
    }

    Texture2D GenerateFogTexture(float boundaryRadius, float totalRadius)
    {
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[textureSize * textureSize];

        float center = textureSize * 0.5f;
        float fogStartRadius = boundaryRadius - fogWidth;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float px = (x + 0.5f - center) / center * totalRadius;
                float py = (y + 0.5f - center) / center * totalRadius;
                float dist = Mathf.Sqrt(px * px + py * py);

                float alpha;
                if (dist <= fogStartRadius)
                {
                    alpha = 0f;
                }
                else if (dist >= boundaryRadius)
                {
                    alpha = 1f;
                }
                else
                {
                    float t = Mathf.InverseLerp(fogStartRadius, boundaryRadius, dist);
                    alpha = Mathf.SmoothStep(0f, 1f, t);
                }

                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    string GetTopSortingLayer()
    {
        var layers = SortingLayer.layers;
        if (layers != null && layers.Length > 0)
            return layers[layers.Length - 1].name;
        return "Default";
    }

    float GetOutermostRadius()
    {
        if (areaManager == null || areaManager.areas == null || areaManager.areas.Count == 0)
        {
            Debug.LogWarning("[MapFogBoundary] No AreaManager assigned - defaulting to radius 50.");
            return 50f;
        }

        float max = 0f;
        foreach (var area in areaManager.areas)
            max = Mathf.Max(max, area.outerRadius);

        return max;
    }

    void OnDrawGizmos()
    {
        float boundary = GetOutermostRadius();
        float fogStart = boundary - fogWidth;

        DrawGizmoCircle(fogStart, new Color(0.5f, 0.8f, 1f, 0.4f));
        DrawGizmoCircle(boundary, new Color(0.5f, 0.8f, 1f, 0.9f));
    }

    void DrawGizmoCircle(float radius, Color color)
    {
        Gizmos.color = color;
        const int segments = 64;
        float step = Mathf.PI * 2f / segments;
        Vector3 prev = transform.position + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = step * i;
            Vector3 next = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}