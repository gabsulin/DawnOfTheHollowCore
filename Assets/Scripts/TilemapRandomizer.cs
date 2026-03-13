using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRandomizer : MonoBehaviour
{

    [System.Serializable]
    public class WeightedTile
    {
        public TileBase tile;
        [Range(0f, 100f)]
        [Tooltip("Higher weight = appears more often")]
        public float weight = 10f;
    }

    [System.Serializable]
    public class AreaTileSettings
    {
        [Header("Identity")]
        [Tooltip("Matches the Area index in AreaManager (0 = innermost)")]
        public string areaLabel = "Area 0";

        [Header("Base Layer (dirt / stone / ground)")]
        public List<WeightedTile> baseTiles = new();

        [Header("Overlay Layer (grass / plants / cracks / decay)")]
        [Range(0f, 1f)]
        [Tooltip("Chance that an overlay tile is painted on any given cell")]
        public float overlayDensity = 0.15f;
        public List<WeightedTile> overlayTiles = new();
    }


    [Header("References")]
    public AreaManager areaManager;
    public Tilemap baseTilemap;
    public Tilemap overlayTilemap;

    [Header("Area Settings (index matches AreaManager.areas)")]
    public List<AreaTileSettings> areaSettings = new();

    [Header("Generation")]
    [Tooltip("How many Unity units one tile covers (default 1 for PPU = 1 unit)")]
    public float tileSize = 1f;
    [Tooltip("Seed for reproducible maps. 0 = random each run.")]
    public int seed = 0;

    [Header("Border Blending")]
    [Range(0f, 0.5f)]
    [Tooltip(
        "Fraction of each area's radial width used as a transition zone into the next area. " +
        "0.15 = the outer 15% of the ring blends tiles from the neighbouring area. " +
        "0 = hard edge (no blending).")]
    public float borderBlendFraction = 0.15f;


    void Start()
    {
        if (!ValidateReferences()) return;

        if (seed != 0)
            Random.InitState(seed);

        PaintAllAreas();
    }
    void PaintAllAreas()
    {
        baseTilemap.ClearAllTiles();
        overlayTilemap.ClearAllTiles();

        for (int areaIndex = 0; areaIndex < areaManager.areas.Count; areaIndex++)
        {
            if (areaIndex >= areaSettings.Count)
            {
                Debug.LogWarning($"[TilemapRandomizer] No AreaTileSettings for area index {areaIndex}. Add more entries.");
                continue;
            }

            var area = areaManager.areas[areaIndex];
            var settings = areaSettings[areaIndex];

            AreaTileSettings nextSettings = null;
            if (areaIndex + 1 < areaSettings.Count)
                nextSettings = areaSettings[areaIndex + 1];

            PaintArea(area, settings, nextSettings);
        }
        var grid = FindFirstObjectByType<GridManager>();
        if (grid != null)
            grid.BakePhysicsCache();

        Debug.Log("[TilemapRandomizer] Tilemap generation complete.");
        Debug.Log("[TilemapRandomizer] Tilemap generation complete.");
    }

    void PaintArea(AreaManager.Area area, AreaTileSettings settings, AreaTileSettings nextSettings)
    {
        if (settings.baseTiles == null || settings.baseTiles.Count == 0)
        {
            Debug.LogWarning($"[TilemapRandomizer] Area '{settings.areaLabel}' has no base tiles.");
            return;
        }

        float ringWidth = area.outerRadius - area.innerRadius;
        float blendStartDist = area.outerRadius - ringWidth * borderBlendFraction;
        bool canBlend = nextSettings != null &&
                               nextSettings.baseTiles != null &&
                               nextSettings.baseTiles.Count > 0 &&
                               borderBlendFraction > 0f;

        int minCell = Mathf.CeilToInt(-area.outerRadius / tileSize);
        int maxCell = Mathf.FloorToInt(area.outerRadius / tileSize);

        for (int x = minCell; x <= maxCell; x++)
        {
            for (int y = minCell; y <= maxCell; y++)
            {
                Vector2 worldPos = new Vector2(x * tileSize, y * tileSize);
                float dist = worldPos.magnitude;
                float distSqr = dist * dist;

                if (distSqr < area.innerRadiusSqr || distSqr >= area.outerRadiusSqr)
                    continue;

                Vector3Int cell = new Vector3Int(x, y, 0);
                float blendT = 0f;
                if (canBlend && dist >= blendStartDist)
                {
                    float raw = Mathf.InverseLerp(blendStartDist, area.outerRadius, dist);
                    blendT = Mathf.SmoothStep(0f, 1f, raw);
                }

                List<WeightedTile> basePool = (canBlend && Random.value < blendT)
                    ? nextSettings.baseTiles
                    : settings.baseTiles;

                TileBase baseTile = PickWeightedTile(basePool);
                if (baseTile != null)
                    baseTilemap.SetTile(cell, baseTile);

                float effectiveDensity = settings.overlayDensity;
                if (canBlend && blendT > 0f)
                {
                    float nextDensity = nextSettings.overlayTiles != null &&
                                        nextSettings.overlayTiles.Count > 0
                        ? nextSettings.overlayDensity
                        : 0f;
                    effectiveDensity = Mathf.Lerp(settings.overlayDensity, nextDensity, blendT);
                }

                if (Random.value < effectiveDensity)
                {
                    bool thisHasOverlay = settings.overlayTiles != null && settings.overlayTiles.Count > 0;
                    bool nextHasOverlay = canBlend &&
                                         nextSettings.overlayTiles != null &&
                                         nextSettings.overlayTiles.Count > 0;

                    List<WeightedTile> overlayPool = null;

                    if (thisHasOverlay && nextHasOverlay)
                        overlayPool = (Random.value < blendT) ? nextSettings.overlayTiles : settings.overlayTiles;
                    else if (thisHasOverlay)
                        overlayPool = settings.overlayTiles;
                    else if (nextHasOverlay)
                        overlayPool = nextSettings.overlayTiles;

                    if (overlayPool != null)
                    {
                        TileBase overlayTile = PickWeightedTile(overlayPool);
                        if (overlayTile != null)
                            overlayTilemap.SetTile(cell, overlayTile);
                    }
                }
            }
        }
    }
    TileBase PickWeightedTile(List<WeightedTile> pool)
    {
        float totalWeight = 0f;
        foreach (var entry in pool)
            totalWeight += Mathf.Max(0f, entry.weight);

        if (totalWeight <= 0f) return null;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in pool)
        {
            cumulative += Mathf.Max(0f, entry.weight);
            if (roll <= cumulative)
                return entry.tile;
        }

        return pool[pool.Count - 1].tile;
    }

    bool ValidateReferences()
    {
        bool ok = true;
        if (areaManager == null) { Debug.LogError("[TilemapRandomizer] AreaManager is not assigned!"); ok = false; }
        if (baseTilemap == null) { Debug.LogError("[TilemapRandomizer] Base Tilemap is not assigned!"); ok = false; }
        if (overlayTilemap == null) { Debug.LogError("[TilemapRandomizer] Overlay Tilemap is not assigned!"); ok = false; }
        return ok;
    }

#if UNITY_EDITOR
    [ContextMenu("Regenerate Tilemaps")]
    void RegenerateInEditor()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[TilemapRandomizer] Regeneration only works in Play Mode via this menu. " +
                             "Use the Seed field + enter Play Mode for reproducible results.");
            return;
        }
        PaintAllAreas();
    }
#endif
}