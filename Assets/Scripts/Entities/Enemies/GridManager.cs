using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;
    
    HashSet<Vector2Int> temporarilyBlocked = new HashSet<Vector2Int>();
    public bool IsWalkable(Vector2Int gridPos)
    {
        Vector3 worldCenter = GridToWorld(gridPos);

        if (temporarilyBlocked.Contains(gridPos))
            return false;

        if (obstacleTilemap.HasTile((Vector3Int)gridPos))
            return false;

        Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.1f, LayerMask.GetMask("Collision"));
        if (hit != null)
            return false;

        return true;
    }
    public void AddTemporaryBlock(Vector2Int pos)
    {
        temporarilyBlocked.Add(pos);
    }
    public void ClearTemporaryBlocks()
    {
        temporarilyBlocked.Clear();
    }
    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        return groundTilemap.CellToWorld((Vector3Int)gridPos) + new Vector3(0.5f, 0.5f);
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return (Vector2Int)groundTilemap.WorldToCell(worldPos);
    }
}
