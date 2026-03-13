using UnityEngine;
using System.Collections.Generic;

public class AStarPathFinder : MonoBehaviour
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, GridManager grid, int maxIterations = 1000, int clearance = 0)
    {
        if (!grid.IsWalkable(goal))
        {
            for (int r = 1; r <= 5; r++)
            {
                bool found = false;
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue;

                        Vector2Int check = new Vector2Int(goal.x + x, goal.y + y);
                        if (grid.IsWalkable(check))
                        {
                            goal = check;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
                if (found) break;
            }
        }

        var openSet = new PriorityQueue<Vector2Int>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int iterations = 0;
        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Vector2Int current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            foreach (Vector2Int neighbour in GetNeighbours(current, grid, clearance))
            {
                if (closedSet.Contains(neighbour))
                    continue;

                float moveCost = 1.0f;
                if (Mathf.Abs(neighbour.x - current.x) == 1 && Mathf.Abs(neighbour.y - current.y) == 1)
                    moveCost = 1.414f;

                float tentativeScore = gScore[current] + moveCost;

                if (!gScore.ContainsKey(neighbour) || tentativeScore < gScore[neighbour])
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeScore;
                    float fScore = tentativeScore + Heuristic(neighbour, goal);

                    if (!openSet.Contains(neighbour))
                        openSet.Enqueue(neighbour, fScore);
                    else
                        openSet.UpdatePriority(neighbour, fScore);
                }
            }
        }
        return new List<Vector2Int>();
    }

    static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) + (1.414f - 2) * Mathf.Min(dx, dy);
    }

    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        Vector2Int.down, Vector2Int.up,
        Vector2Int.left, Vector2Int.right,
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
    };

    static List<Vector2Int> GetNeighbours(Vector2Int pos, GridManager grid, int clearance)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>(8);

        foreach (Vector2Int dir in Directions)
        {
            Vector2Int next = pos + dir;

            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                Vector2Int side1 = pos + new Vector2Int(dir.x, 0);
                Vector2Int side2 = pos + new Vector2Int(0, dir.y);
                if (!grid.IsWalkableWithClearance(side1, clearance) || !grid.IsWalkableWithClearance(side2, clearance))
                    continue;
            }

            if (grid.IsWalkableWithClearance(next, clearance))
                neighbours.Add(next);
        }
        return neighbours;
    }

    static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int>() { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}