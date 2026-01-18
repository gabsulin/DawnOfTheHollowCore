using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public AreaManager areaManager;
    public Transform player;
    public int baseEnemiesPerWave = 5;
    public float spawnDelay = 0.5f;

    bool isNight = false;
    int nightCount = 0;
    List<GameObject> activeEnemies = new();

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
        nightCount++;
        StartCoroutine(SpawnWave());
    }

    void EndNight()
    {
        isNight = false;

        foreach (var enemy in activeEnemies)
            if (enemy) Destroy(enemy);

        activeEnemies.Clear();
    }

    System.Collections.IEnumerator SpawnWave()
    {
        while (isNight)
        {
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

            int enemiesPerArea = Mathf.Max(1, totalWaveSize / unlockedAreas.Count);

            foreach (var area in unlockedAreas)
            {
                for (int i = 0; i < enemiesPerArea; i++)
                {
                    SpawnEnemyInArea(area);
                    yield return new WaitForSeconds(spawnDelay);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(5f, 15f - nightCount * 0.5f));
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