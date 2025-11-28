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
            if (area == null || !area.unlocked) { yield return new WaitForSeconds(1f); continue; }

            int mult = Mathf.Max(1, area.id);
            int waveSize = Mathf.RoundToInt(baseEnemiesPerWave * (1f + nightCount * 0.2f) * mult);

            /*for (int i = 0; i < waveSize; i++)
            {
                var list = areaManager.rarityGroups[area.id].orePrefabs;
                yield return null;
            }*/

            yield return new WaitForSeconds(Mathf.Max(5f, 15f - nightCount * 0.5f));
        }
    }

    AreaManager.Area GetArea(float dist)
    {
        foreach (var a in areaManager.areas)
            if (dist >= a.innerRadius && dist < a.outerRadius)
                return a;
        return areaManager.areas[0];
    }
}
