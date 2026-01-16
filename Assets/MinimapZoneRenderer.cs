using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MinimapZoneRenderer : MonoBehaviour
{
    [System.Serializable]
    public class Zone
    {
        public int index;
        public float radius;
        public Color color;

        public Zone(int idx, float rad, Color col)
        {
            index = idx;
            radius = rad;
            color = col;
        }
    }

    [Header("Zone Configuration")]
    public List<Zone> zones = new List<Zone>
    {
        new Zone(0, 10f, new Color(1f, 0f, 0f, 0.4f)),      // Èervená - støed
        new Zone(1, 20f, new Color(1f, 0.5f, 0f, 0.4f)),    // Oranžová
        new Zone(2, 30f, new Color(1f, 1f, 0f, 0.4f)),      // Žlutá
        new Zone(3, 40f, new Color(0f, 1f, 0f, 0.4f)),      // Zelená
        new Zone(4, 50f, new Color(0f, 0.5f, 1f, 0.4f))     // Modrá
    };

    [Header("Visual Settings")]
    [Range(32, 128)]
    public int circleSegments = 64;

    public bool renderAsRings = false;

    [Range(0.5f, 5f)]
    public float ringThickness = 1f;

    [Header("References")]
    public Material zoneMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh zoneMesh;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Ujisti se, že GameObject je na správném layeru
        gameObject.layer = LayerMask.NameToLayer("Minimap");

        // Pøiøaï materiál
        if (zoneMaterial != null)
        {
            meshRenderer.material = zoneMaterial;
        }
        else
        {
            // Vytvoø základní transparentní materiál pro URP
            Material defaultMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            defaultMat.color = Color.white;
            // Nastav transparent mode
            defaultMat.SetFloat("_Surface", 1); // Transparent
            defaultMat.SetFloat("_Blend", 0); // Alpha blend
            defaultMat.renderQueue = 3000;
            meshRenderer.material = defaultMat;
        }

        // Seøaï zóny podle radiusu (od nejvìtší po nejmenší)
        zones.Sort((a, b) => b.radius.CompareTo(a.radius));

        GenerateZoneMesh();
    }

    void GenerateZoneMesh()
    {
        if (zones == null || zones.Count == 0)
        {
            Debug.LogWarning("Žádné zóny nejsou definované!");
            return;
        }

        zoneMesh = new Mesh();
        zoneMesh.name = "Zone Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        if (renderAsRings)
        {
            GenerateRingMesh(vertices, triangles, colors);
        }
        else
        {
            GenerateFilledCircleMesh(vertices, triangles, colors);
        }

        zoneMesh.SetVertices(vertices);
        zoneMesh.SetTriangles(triangles, 0);
        zoneMesh.SetColors(colors);

        zoneMesh.RecalculateNormals();
        zoneMesh.RecalculateBounds();

        meshFilter.mesh = zoneMesh;
    }

    void GenerateFilledCircleMesh(List<Vector3> vertices, List<int> triangles, List<Color> colors)
    {
        foreach (Zone zone in zones)
        {
            int startIndex = vertices.Count;

            // Støed kruhu - Z = 0 pro 2D top-down
            vertices.Add(Vector3.zero);
            colors.Add(zone.color);

            // Vytvoø vertices po obvodu kruhu (XY rovina)
            for (int i = 0; i <= circleSegments; i++)
            {
                float angle = (float)i / circleSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * zone.radius;
                float y = Mathf.Sin(angle) * zone.radius;

                vertices.Add(new Vector3(x, y, 0)); // Z = 0
                colors.Add(zone.color);
            }

            // Vytvoø trojúhelníky
            for (int i = 0; i < circleSegments; i++)
            {
                triangles.Add(startIndex);
                triangles.Add(startIndex + i + 1);
                triangles.Add(startIndex + i + 2);
            }
        }
    }

    void GenerateRingMesh(List<Vector3> vertices, List<int> triangles, List<Color> colors)
    {
        for (int z = 0; z < zones.Count; z++)
        {
            Zone zone = zones[z];
            float outerRadius = zone.radius;
            float innerRadius = (z < zones.Count - 1) ? zones[z + 1].radius : zone.radius - ringThickness;

            if (innerRadius >= outerRadius - 0.1f)
            {
                innerRadius = outerRadius - ringThickness;
            }

            if (innerRadius < 0) innerRadius = 0;

            int startIndex = vertices.Count;

            // Vytvoø vertices pro vnitøní a vnìjší kruh (XY rovina)
            for (int i = 0; i <= circleSegments; i++)
            {
                float angle = (float)i / circleSegments * Mathf.PI * 2f;
                float cosAngle = Mathf.Cos(angle);
                float sinAngle = Mathf.Sin(angle);

                // Vnìjší vertex
                vertices.Add(new Vector3(cosAngle * outerRadius, sinAngle * outerRadius, 0));
                colors.Add(zone.color);

                // Vnitøní vertex
                vertices.Add(new Vector3(cosAngle * innerRadius, sinAngle * innerRadius, 0));
                colors.Add(zone.color);
            }

            // Vytvoø trojúhelníky
            for (int i = 0; i < circleSegments; i++)
            {
                int current = startIndex + i * 2;
                int next = startIndex + (i + 1) * 2;

                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);

                triangles.Add(next);
                triangles.Add(current + 1);
                triangles.Add(next + 1);
            }
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying && meshFilter != null)
        {
            GenerateZoneMesh();
        }
    }

    void OnDrawGizmos()
    {
        if (zones == null) return;

        foreach (Zone zone in zones)
        {
            Gizmos.color = zone.color;
            DrawCircleGizmo(transform.position, zone.radius);
        }
    }

    void DrawCircleGizmo(Vector3 center, float radius)
    {
        int segments = 32;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    public void SetZoneColor(int zoneIndex, Color newColor)
    {
        Zone zone = zones.Find(z => z.index == zoneIndex);
        if (zone != null)
        {
            zone.color = newColor;
            GenerateZoneMesh();
        }
    }

    public void SetZoneRadius(int zoneIndex, float newRadius)
    {
        Zone zone = zones.Find(z => z.index == zoneIndex);
        if (zone != null)
        {
            zone.radius = newRadius;
            zones.Sort((a, b) => b.radius.CompareTo(a.radius));
            GenerateZoneMesh();
        }
    }

    public Zone GetZoneAtPosition(Vector2 worldPosition)
    {
        Vector2 centerPos = new Vector2(transform.position.x, transform.position.y);
        float distance = Vector2.Distance(worldPosition, centerPos);

        zones.Sort((a, b) => a.radius.CompareTo(b.radius));
        foreach (Zone zone in zones)
        {
            if (distance <= zone.radius)
            {
                return zone;
            }
        }

        return null;
    }
}