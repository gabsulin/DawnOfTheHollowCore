using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public AreaManager areaManager;
    public Transform player;
    public int baseEnemiesPerWave = 5;
    public float spawnDelay = 0.5f;

    [Header("Spawn Limits")]
    [Tooltip("Maximum number of enemies that can be active at once")]
    public int maxActiveEnemies = 50;

    bool isNight = false;
    int nightCount = NightCounter.currentNight;
    List<GameObject> activeEnemies = new();

    Coroutine spawnCoroutine; // Track the coroutine!

    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform bossSpawnPoint;
    public bool bossSpawned = false;

    void OnEnable()
    {
        DayNightCycle.OnNightStart += StartNight;
        DayNightCycle.OnDayStart += EndNight;
    }

    void OnDisable()
    {
        DayNightCycle.OnNightStart -= StartNight;
        DayNightCycle.OnDayStart -= EndNight;
    }

    void StartNight()
    {
        isNight = true;
        nightCount = NightCounter.currentNight;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnWave());
        TrySpawnBoss();
    }

    void EndNight()
    {
        isNight = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        foreach (var enemy in activeEnemies)
            if (enemy) Destroy(enemy);

        activeEnemies.Clear();
    }

    void TrySpawnBoss()
    {
        if (bossSpawned) return;

        if (NightCounter.currentNight >= 20)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            bossSpawned = true;
            Debug.Log("Boss spawned for night " + NightCounter.currentNight);
        }
        else
        {
            Debug.Log("Not night twenty");
            Debug.Log(NightCounter.currentNight);
        }
    }

    IEnumerator SpawnWave()
    {
        while (isNight)
        {
            Debug.Log($"[EnemySpawner] Wave loop iteration - isNight: {isNight}, activeEnemies: {activeEnemies.Count}");

            activeEnemies.RemoveAll(enemy => enemy == null);

            if (activeEnemies.Count >= maxActiveEnemies)
            {
                yield return new WaitForSeconds(2f);
                continue;
            }

            List<AreaManager.Area> unlockedAreas = GetUnlockedAreas();

            if (unlockedAreas.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            int highestAreaId = GetHighestUnlockedAreaId(unlockedAreas);
            int mult = Mathf.Max(1, highestAreaId);
            int totalWaveSize = Mathf.RoundToInt(
                baseEnemiesPerWave * (1f + nightCount * 0.2f) * mult
            );

            int remainingCapacity = maxActiveEnemies - activeEnemies.Count;
            totalWaveSize = Mathf.Min(totalWaveSize, remainingCapacity);

            Debug.Log($"[EnemySpawner] Spawning wave: totalWaveSize={totalWaveSize}, unlocked areas={unlockedAreas.Count}");

            int enemiesPerArea = Mathf.Max(1, totalWaveSize / unlockedAreas.Count);

            foreach (var area in unlockedAreas)
            {
                for (int i = 0; i < enemiesPerArea; i++)
                {
                    if (activeEnemies.Count >= maxActiveEnemies)
                    {
                        break;
                    }

                    SpawnEnemyInArea(area);
                    yield return new WaitForSeconds(spawnDelay);
                }

                if (activeEnemies.Count >= maxActiveEnemies)
                    break;
            }

            float delayTime = Mathf.Max(15f, 30f - nightCount * 0.5f);
            yield return new WaitForSeconds(delayTime);
        }
    }

    List<AreaManager.Area> GetUnlockedAreas()
    {
        List<AreaManager.Area> unlocked = new();

        foreach (var area in areaManager.areas)
        {
            if (area.unlocked && area.enemyGroup != null &&
                area.enemyGroup.enemies != null &&
                area.enemyGroup.enemies.Count > 0)
            {
                unlocked.Add(area);
            }
        }

        return unlocked;
    }

    int GetHighestUnlockedAreaId(List<AreaManager.Area> unlockedAreas)
    {
        int highest = 0;
        foreach (var area in unlockedAreas)
        {
            if (area.id > highest)
                highest = area.id;
        }
        return highest;
    }

    void SpawnEnemyInArea(AreaManager.Area area)
    {
        if (area.enemyGroup == null ||
            area.enemyGroup.enemies == null ||
            area.enemyGroup.enemies.Count == 0)
            return;

        GameObject prefab = area.enemyGroup.enemies[
            Random.Range(0, area.enemyGroup.enemies.Count)
        ];

        Vector2 spawnPos = RandomPositionInArea(area);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    Vector2 RandomPositionInArea(AreaManager.Area area)
    {
        float radius = Random.Range(area.innerRadius, area.outerRadius);
        float angle = Random.Range(0f, Mathf.PI * 2);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}