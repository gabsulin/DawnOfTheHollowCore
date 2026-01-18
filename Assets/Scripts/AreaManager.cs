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
    public float npcSpawnRadius = 5f;

    Area currentArea;

    void Start()
    {
        if (areas.Count > 0)
            areas[0].unlocked = true;

        if (crystalsParent == null)
            Debug.LogWarning("Crystals parent is NOT assigned. Ores will be unparented.");

        foreach (var area in areas)
        {
            SpawnOresInArea(area);
        }
    }

    void Update()
    {
        if (!player || areas.Count == 0) return;

        float dist = Vector2.Distance(Vector2.zero, player.position);
        Area newArea = GetAreaFromDistance(dist);

        if (newArea != currentArea)
        {
            currentArea = newArea;
            HandleAreaEnter(currentArea);
        }
    }

    Area GetAreaFromDistance(float dist)
    {
        foreach (var area in areas)
        {
            if (dist >= area.innerRadius && dist < area.outerRadius)
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
        }
    }

    // ===================== ORE SPAWNING =====================

    public void SpawnOresInArea(Area area)
    {
        foreach (var settings in area.raritySettings)
        {
            if (settings.orePrefabs == null || settings.orePrefabs.Count == 0)
                continue;

            for (int i = 0; i < settings.spawnTries; i++)
            {
                if (Random.value <= settings.chancePerTry)
                {
                    GameObject prefab = settings.orePrefabs[Random.Range(0, settings.orePrefabs.Count)];
                    Vector2 pos = RandomPositionInArea(area);

                    GameObject ore = Instantiate(
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
