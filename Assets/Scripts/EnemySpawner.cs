using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public AreaManager areaManager;
    public Transform player;
    public float spawnRadius = 10f;
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
            float dist = Vector2.Distance(Vector2.zero, player.position);
            var area = GetArea(dist);

            if (area == null || !area.unlocked)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // must have enemies here
            if (area.enemyGroup == null ||
                area.enemyGroup.enemies == null ||
                area.enemyGroup.enemies.Count == 0)
            {
                yield return new WaitForSeconds(2f);
                continue;
            }

            int mult = Mathf.Max(1, area.id);
            int waveSize = Mathf.RoundToInt(
                baseEnemiesPerWave * (1f + nightCount * 0.2f) * mult
            );

            for (int i = 0; i < waveSize; i++)
            {
                SpawnEnemy(area);
                yield return new WaitForSeconds(spawnDelay);
            }

            yield return new WaitForSeconds(Mathf.Max(5f, 15f - nightCount * 0.5f));
        }
    }

    void SpawnEnemy(AreaManager.Area area)
    {
        if (area.enemyGroup == null ||
            area.enemyGroup.enemies == null ||
            area.enemyGroup.enemies.Count == 0)
            return;

        GameObject prefab = area.enemyGroup.enemies[
            Random.Range(0, area.enemyGroup.enemies.Count)
        ];

        Vector2 spawnPos = (Vector2)player.position +
                            Random.insideUnitCircle.normalized * spawnRadius;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    AreaManager.Area GetArea(float dist)
    {
        foreach (var a in areaManager.areas)
            if (dist >= a.innerRadius && dist < a.outerRadius)
                return a;

        return areaManager.areas[0];
    }
}
