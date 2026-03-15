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
    public int maxActiveEnemies = 25;

    bool isNight = false;
    int nightCount = NightCounter.currentNight;
    List<GameObject> activeEnemies = new();

    Coroutine spawnCoroutine;

    [Header("Boss")]
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform bossSpawnPoint;
    [SerializeField] BossPositionIndicator bossIndicator;
    [SerializeField] GameObject bossIndicatorUI;

    public bool bossSpawned = false;
    private Transform bossTransform;

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
            StopCoroutine(spawnCoroutine);

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

        if (bossIndicator != null)
            bossIndicator.SetBoss(null);
    }

    void TrySpawnBoss()
    {
        if (bossSpawned) return;

        if (NightCounter.currentNight >= 20)
        {
            GameObject bossGO = Instantiate(
                bossPrefab,
                bossSpawnPoint.position,
                Quaternion.identity
            );

            bossTransform = bossGO.transform;
            bossSpawned = true;

            bossIndicatorUI.SetActive(true);
            bossIndicator.SetBoss(bossTransform);

            Debug.Log("Boss spawned & indicator linked");
        }
    }

    IEnumerator SpawnWave()
    {
        while (isNight)
        {
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
                baseEnemiesPerWave * (1f + nightCount * 0.01f) * mult
            );

            int remainingCapacity = maxActiveEnemies - activeEnemies.Count;
            totalWaveSize = Mathf.Min(totalWaveSize, remainingCapacity);

            int enemiesPerArea = Mathf.Max(1, totalWaveSize / unlockedAreas.Count);

            Debug.Log($"Spawning wave | Night: {nightCount} | Total enemies: {totalWaveSize} | Areas: {unlockedAreas.Count}");

            foreach (var area in unlockedAreas)
            {
                for (int i = 0; i < enemiesPerArea; i++)
                {
                    if (activeEnemies.Count >= maxActiveEnemies)
                        break;

                    SpawnEnemyInArea(area);
                    yield return new WaitForSeconds(spawnDelay);
                }

                if (activeEnemies.Count >= maxActiveEnemies)
                    break;
            }

            float waveTimer = Mathf.Max(15f, 30f - nightCount * 0.5f);
            float elapsed = 0f;
            bool clearedByPlayer = false;

            yield return new WaitUntil(() =>
            {
                activeEnemies.RemoveAll(enemy => enemy == null);
                elapsed += Time.deltaTime;

                if (activeEnemies.Count == 0)
                {
                    clearedByPlayer = true;
                    return true;
                }

                return elapsed >= waveTimer;
            });

            if (clearedByPlayer)
                Debug.Log("Wave cleared | Spawning new wave");
            else
                Debug.Log($"Timer reached 0 | {activeEnemies.Count} enemies still alive | Spawning new wave");
        }
    }

    List<AreaManager.Area> GetUnlockedAreas()
    {
        List<AreaManager.Area> unlocked = new();

        foreach (var area in areaManager.areas)
        {
            if (area.unlocked &&
                area.enemyGroup != null &&
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
            if (area.id > highest)
                highest = area.id;

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