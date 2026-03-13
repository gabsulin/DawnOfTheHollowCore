using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;

    private HashSet<Vector2Int> temporarilyBlocked = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> physicsObstacleCache = new HashSet<Vector2Int>(); // FIX: baked physics cache

    private void Awake()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game"))
            BakePhysicsCache();
    }

    // FIX: scan every ground tile once and store physics obstacles
    public void BakePhysicsCache()
    {
        physicsObstacleCache.Clear();

        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldCenter = GridToWorld(gridPos);

                Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.1f, LayerMask.GetMask("Collision"));
                if (hit != null)
                    physicsObstacleCache.Add(gridPos);
            }
        }
    }

    // FIX: call this when an obstacle is removed (ore mined, etc.)
    public void InvalidateCell(Vector2Int gridPos)
    {
        physicsObstacleCache.Remove(gridPos);
        temporarilyBlocked.Remove(gridPos);
    }

    // FIX: no more live Physics2D call — pure cache lookup
    public bool IsWalkable(Vector2Int gridPos)
    {
        if (temporarilyBlocked.Contains(gridPos))
            return false;

        if (obstacleTilemap.HasTile((Vector3Int)gridPos))
            return false;

        if (physicsObstacleCache.Contains(gridPos))
            return false;

        return true;
    }

    public bool IsWalkableWithClearance(Vector2Int gridPos, int clearance)
    {
        for (int x = -clearance; x <= clearance; x++)
        {
            for (int y = -clearance; y <= clearance; y++)
            {
                if (!IsWalkable(gridPos + new Vector2Int(x, y)))
                    return false;
            }
        }
        return true;
    }

    public void AddTemporaryBlock(Vector2Int pos) { temporarilyBlocked.Add(pos); }
    public void ClearTemporaryBlocks() { temporarilyBlocked.Clear(); }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        return groundTilemap.CellToWorld((Vector3Int)gridPos) + new Vector3(0.5f, 0.5f);
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return (Vector2Int)groundTilemap.WorldToCell(worldPos);
    }
}