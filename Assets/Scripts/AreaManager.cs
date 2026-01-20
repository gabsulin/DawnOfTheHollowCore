using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    [System.Serializable]
    public class Area
    {
        public int id;

        public float innerRadius;
        public float outerRadius;

        public bool unlocked = false;

        public GameObject npcForThisArea;

        [HideInInspector] public GameObject spawnedNpc;

        public List<RaritySpawnSettings> raritySettings = new();

        public EnemySpawnGroup enemyGroup;

        // Cache pro optimalizaci
        [HideInInspector] public float innerRadiusSqr;
        [HideInInspector] public float outerRadiusSqr;
    }

    [System.Serializable]
    public class RaritySpawnSettings
    {
        public OreNode.Rarity rarity;

        [Range(0, 100)]
        public int spawnTries = 10;

        [Range(0f, 1f)]
        public float chancePerTry = 1f;

        public List<GameObject> orePrefabs;
    }

    [System.Serializable]
    public class EnemySpawnGroup
    {
        public List<GameObject> enemies;
    }

    [Header("References")]
    public Transform player;
    public Transform crystalsParent;

    public List<Area> areas;

    [Header("NPC Settings")]
    public float npcSpawnRadius = 2f;

    [Header("Minimap NPC Icons")]
    public bool createMinimapIcons = true;

    public GameObject customIconPrefab;

    public bool useNpcSprite = true;

    public Color npcIconColor = Color.red;

    public float npcIconSize = 0.8f;

    public string minimapLayer = "Minimap";

    public float iconZPosition = -10f;

    [Header("Optimization")]
    public float areaCheckInterval = 0.1f;

    Area currentArea;
    float nextAreaCheck = 0f;

    Sprite cachedMinimapSprite;
    int minimapLayerInt;

    void Start()
    {
        foreach (var area in areas)
        {
            area.innerRadiusSqr = area.innerRadius * area.innerRadius;
            area.outerRadiusSqr = area.outerRadius * area.outerRadius;
        }

        if (areas.Count > 0)
            areas[0].unlocked = true;

        if (crystalsParent == null)
            Debug.LogWarning("Crystals parent is NOT assigned. Ores will be unparented.");

        minimapLayerInt = LayerMask.NameToLayer(minimapLayer);
        if (minimapLayerInt == -1)
        {
            Debug.LogWarning($"[AreaManager] Layer '{minimapLayer}' doesn't exist!");
            minimapLayerInt = 0;
        }

        if (createMinimapIcons && customIconPrefab == null && !useNpcSprite)
        {
            cachedMinimapSprite = GetMinimapSprite();
        }

        foreach (var area in areas)
        {
            SpawnOresInArea(area);
        }
    }

    void Update()
    {
        if (!player || areas.Count == 0) return;

        if (Time.time < nextAreaCheck) return;
        nextAreaCheck = Time.time + areaCheckInterval;

        float distSqr = player.position.sqrMagnitude;
        Area newArea = GetAreaFromDistanceSqr(distSqr);

        if (newArea != currentArea)
        {
            currentArea = newArea;
            HandleAreaEnter(currentArea);
        }
    }

    Area GetAreaFromDistanceSqr(float distSqr)
    {
        for (int i = areas.Count - 1; i >= 0; i--)
        {
            var area = areas[i];
            if (distSqr >= area.innerRadiusSqr && distSqr < area.outerRadiusSqr)
                return area;
        }

        return areas[0];
    }

    void HandleAreaEnter(Area area)
    {
        if (!area.unlocked)
            area.unlocked = true;

        if (area.npcForThisArea != null && area.spawnedNpc == null)
        {
            Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle * npcSpawnRadius;
            area.spawnedNpc = Instantiate(area.npcForThisArea, spawnPos, Quaternion.identity);

            // Vytvoø ikonu pro minimapu
            if (createMinimapIcons)
            {
                CreateMinimapIconForNPC(area.spawnedNpc);
            }
        }
    }

    // ===================== MINIMAP ICON =====================

    void CreateMinimapIconForNPC(GameObject npc)
    {
        if (npc == null) return;

        GameObject iconObject;

        if (customIconPrefab != null)
        {
            iconObject = Instantiate(customIconPrefab, npc.transform);
            iconObject.name = $"{npc.name}_MinimapIcon";
            iconObject.transform.localPosition = new Vector3(0, 0, iconZPosition);
            iconObject.transform.localRotation = Quaternion.identity;
            iconObject.layer = minimapLayerInt;

            SpriteRenderer[] renderers = iconObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                sr.color = npcIconColor;
                sr.sortingOrder = 99;
            }
            iconObject.transform.localScale = Vector3.one * npcIconSize;

            Debug.Log($"[AreaManager] Custom prefab icon created for {npc.name}");
            return;
        }

        if (useNpcSprite)
        {
            Sprite npcSprite = GetNPCSprite(npc);
            if (npcSprite != null)
            {
                iconObject = CreateSpriteIcon(npc, npcSprite);
                Debug.Log($"[AreaManager] Icon created using NPC sprite for {npc.name}");
                return;
            }
        }

        if (cachedMinimapSprite == null)
        {
            cachedMinimapSprite = GetMinimapSprite();
        }

        iconObject = CreateSpriteIcon(npc, cachedMinimapSprite);
        Debug.Log($"[AreaManager] Default circle icon created for {npc.name}");
    }

    Sprite GetNPCSprite(GameObject npc)
    {
        SpriteRenderer npcRenderer = npc.GetComponent<SpriteRenderer>();
        if (npcRenderer != null && npcRenderer.sprite != null)
        {
            npcIconColor = npcRenderer.color;
            return npcRenderer.sprite;
        }

        npcRenderer = npc.GetComponentInChildren<SpriteRenderer>();
        if (npcRenderer != null && npcRenderer.sprite != null)
        {
            return npcRenderer.sprite;
        }

        return null;
    }

    GameObject CreateSpriteIcon(GameObject npc, Sprite sprite)
    {
        GameObject iconObject = new GameObject($"{npc.name}_MinimapIcon");
        iconObject.transform.SetParent(npc.transform);
        iconObject.transform.localPosition = new Vector3(0, 0, iconZPosition);
        iconObject.transform.localRotation = Quaternion.identity;
        iconObject.layer = minimapLayerInt;

        SpriteRenderer sr = iconObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        sr.color = npcIconColor;

        sr.material = new Material(Shader.Find("Sprites/Default"));
        sr.material.color = npcIconColor;

        sr.sortingOrder = 99;

        iconObject.transform.localScale = Vector3.one * npcIconSize;

        return iconObject;
    }

    Sprite GetMinimapSprite()
    {
        Sprite sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

        if (sprite == null)
        {
            sprite = CreateCircleSprite(32);
        }

        return sprite;
    }

    Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance <= radius ? 1f : 0f;

                // Vyhlazený okraj
                if (distance > radius - 2f && distance <= radius)
                {
                    alpha = (radius - distance) / 2f;
                }

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // ===================== ORE SPAWNING =====================

    public void SpawnOresInArea(Area area)
    {
        if (area.raritySettings == null || area.raritySettings.Count == 0)
            return;

        // =============================
        // 1) GUARANTEE HIGHEST RARITY
        // =============================

        RaritySpawnSettings highestRaritySettings = null;

        foreach (var settings in area.raritySettings)
        {
            if (highestRaritySettings == null ||
                settings.rarity > highestRaritySettings.rarity)
            {
                highestRaritySettings = settings;
            }
        }

        if (highestRaritySettings != null &&
            highestRaritySettings.orePrefabs != null &&
            highestRaritySettings.orePrefabs.Count > 0)
        {
            GameObject guaranteedPrefab =
                highestRaritySettings.orePrefabs[
                    Random.Range(0, highestRaritySettings.orePrefabs.Count)
                ];

            Vector2 guaranteedPos = RandomPositionInArea(area);

            Instantiate(
                guaranteedPrefab,
                guaranteedPos,
                Quaternion.identity,
                crystalsParent
            );
        }

        // =============================
        // 2) NORMAL RNG SPAWNING
        // =============================

        foreach (var settings in area.raritySettings)
        {
            if (settings.orePrefabs == null || settings.orePrefabs.Count == 0)
                continue;

            for (int i = 0; i < settings.spawnTries; i++)
            {
                if (Random.value <= settings.chancePerTry)
                {
                    GameObject prefab =
                        settings.orePrefabs[
                            Random.Range(0, settings.orePrefabs.Count)
                        ];

                    Vector2 pos = RandomPositionInArea(area);

                    Instantiate(
                        prefab,
                        pos,
                        Quaternion.identity,
                        crystalsParent
                    );
                }
            }
        }
    }


    Vector2 RandomPositionInArea(Area area)
    {
        float radius = Random.Range(area.innerRadius, area.outerRadius);
        float angle = Random.Range(0f, Mathf.PI * 2);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    // ===================== GIZMOS =====================

    void OnDrawGizmos()
    {
        if (areas == null) return;

        foreach (var area in areas)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
            DrawCircle(Vector3.zero, area.innerRadius);

            Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
            DrawCircle(Vector3.zero, area.outerRadius);
        }
    }

    void DrawCircle(Vector3 center, float radius)
    {
        const int points = 80;
        float step = (Mathf.PI * 2f) / points;

        Vector3 prev = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

        for (int i = 1; i <= points; i++)
        {
            float angle = step * i;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}