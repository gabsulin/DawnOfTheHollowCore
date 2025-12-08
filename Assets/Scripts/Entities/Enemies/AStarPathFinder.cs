using UnityEngine;
using System.Collections.Generic;

public class AStarPathFinder : MonoBehaviour
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, GridManager grid, int maxIterations = 1000)
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
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        int iterations = 0;
        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Vector2Int current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (Vector2Int neighbour in GetNeighbours(current, grid))
            {
                float moveCost = 1.0f;
                if (Mathf.Abs(neighbour.x - current.x) == 1 && Mathf.Abs(neighbour.y - current.y) == 1)
                    moveCost = 1.4142f;

                float tentativeScore = gScore[current] + moveCost;

                if (!gScore.ContainsKey(neighbour) || tentativeScore < gScore[neighbour])
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeScore;
                    fScore[neighbour] = tentativeScore + Heuristic(neighbour, goal);

                    if (!openSet.Contains(neighbour))
                        openSet.Enqueue(neighbour, fScore[neighbour]);
                }
            }
        }
        return new List<Vector2Int>();
    }

    static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }
    static List<Vector2Int> GetNeighbours(Vector2Int pos, GridManager grid)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.down, Vector2Int.up,
            Vector2Int.left, Vector2Int.right,
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        List<Vector2Int> neighbours = new List<Vector2Int>();

        foreach (Vector2Int dir in directions)
        {
            Vector2Int next = pos + dir;
            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                Vector2Int side1 = pos + new Vector2Int(dir.x, 0);
                Vector2Int side2 = pos + new Vector2Int(0, dir.y);
                if (!grid.IsWalkable(side1) || !grid.IsWalkable(side2))
                    continue;

                
            }
            if (grid.IsWalkable(next))
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
