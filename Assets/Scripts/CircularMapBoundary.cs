using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class CircularMapBoundary : MonoBehaviour
{
    [Header("References")]
    public AreaManager areaManager;

    [Header("Boundary Settings")]
    [Tooltip("Extra units added to the outermost radius. Use a small negative value to pull the wall just inside the visible edge.")]
    public float radiusOffset = -0.5f;

    [Tooltip("How many points make up the circle. More = smoother, but 64 is plenty for gameplay.")]
    [Range(16, 256)]
    public int resolution = 64;

    EdgeCollider2D edgeCollider;

    void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        BuildCircle();
    }

    void BuildCircle()
    {
        float radius = GetOutermostRadius() + radiusOffset;

        Vector2[] points = new Vector2[resolution + 1];
        float step = (Mathf.PI * 2f) / resolution;

        for (int i = 0; i < resolution; i++)
        {
            float angle = step * i;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        points[resolution] = points[0];

        edgeCollider.SetPoints(new System.Collections.Generic.List<Vector2>(points));
    }

    float GetOutermostRadius()
    {
        if (areaManager == null || areaManager.areas == null || areaManager.areas.Count == 0)
        {
            Debug.LogWarning("[CircularMapBoundary] No AreaManager assigned — defaulting to radius 50.");
            return 50f;
        }

        float max = 0f;
        foreach (var area in areaManager.areas)
            max = Mathf.Max(max, area.outerRadius);

        return max;
    }

    void OnDrawGizmos()
    {
        float radius = GetOutermostRadius() + radiusOffset;
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);

        int segments = Mathf.Max(resolution, 32);
        float step = Mathf.PI * 2f / segments;
        Vector3 prev = transform.position + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = step * i;
            Vector3 next = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}