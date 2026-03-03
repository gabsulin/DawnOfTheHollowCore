using UnityEngine;

public class DeathZoneAutoSetup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your AreaManager with area configurations")]
    [SerializeField] private AreaManager areaManager;

    [Header("Death Zone Settings")]
    [Tooltip("Buffer distance INSIDE area boundary (e.g., 2-3 units smaller)")]
    [SerializeField] private float radiusOffset = 2f;

    [Header("Death Zone GameObjects")]
    [Tooltip("Death zone protecting Area 1 (at Area 0's outer radius)")]
    [SerializeField] private AreaDeathZone deathZone_Area1;

    [Tooltip("Death zone protecting Area 2 (at Area 1's outer radius)")]
    [SerializeField] private AreaDeathZone deathZone_Area2;

    [Tooltip("Death zone protecting Area 3 (at Area 2's outer radius)")]
    [SerializeField] private AreaDeathZone deathZone_Area3;

    [Tooltip("Death zone protecting Area 4 (at Area 3's outer radius)")]
    [SerializeField] private AreaDeathZone deathZone_Area4;

    [Header("Status")]
    [SerializeField] private bool isSetupComplete = false;

    [ContextMenu("Setup Death Zones")]
    public void SetupDeathZones()
    {
        if (areaManager == null)
        {
            Debug.LogError("[DeathZoneAutoSetup] AreaManager not assigned!");
            return;
        }

        if (areaManager.areas == null || areaManager.areas.Count < 4)
        {
            Debug.LogError("[DeathZoneAutoSetup] AreaManager needs at least 4 areas configured!");
            return;
        }

        int successCount = 0;

        if (deathZone_Area1 != null)
        {
            float radius = areaManager.areas[1].innerRadius;
            SetupDeathZone(deathZone_Area1, 1, radius);
            successCount++;
        }

        if (deathZone_Area2 != null)
        {
            float radius = areaManager.areas[2].innerRadius;
            SetupDeathZone(deathZone_Area2, 2, radius);
            successCount++;
        }

        if (deathZone_Area3 != null)
        {
            float radius = areaManager.areas[3].innerRadius;
            SetupDeathZone(deathZone_Area3, 3, radius);
            successCount++;
        }

        if (deathZone_Area4 != null)
        {
            float radius = areaManager.areas[4].innerRadius;
            SetupDeathZone(deathZone_Area4, 4, radius);
            successCount++;
        }

        isSetupComplete = true;
        Debug.Log($"[DeathZoneAutoSetup] Setup complete! Configured {successCount} death zones.");
    }

    private void SetupDeathZone(AreaDeathZone deathZone, int protectedAreaId, float areaInnerRadius)
    {
        CircleCollider2D collider = deathZone.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            Debug.LogError($"[DeathZoneAutoSetup] Death zone for Area {protectedAreaId} is missing CircleCollider2D!");
            return;
        }
        float deathZoneRadius = areaInnerRadius - radiusOffset;

        if (deathZoneRadius <= 0)
        {
            Debug.LogError($"[DeathZoneAutoSetup] Area {protectedAreaId} radius offset too large! Radius would be negative.");
            deathZoneRadius = areaInnerRadius * 0.9f;
        }

        collider.radius = deathZoneRadius;
        collider.isTrigger = true;

        deathZone.transform.position = Vector3.zero;

        Debug.Log($"[DeathZoneAutoSetup] Area {protectedAreaId} death zone: Radius set to {deathZoneRadius} (area: {areaInnerRadius}, offset: {radiusOffset})");
    }

    [ContextMenu("Create Death Zone GameObjects")]
    public void CreateDeathZoneGameObjects()
    {
        if (deathZone_Area1 == null)
        {
            deathZone_Area1 = CreateDeathZoneObject("DeathZone_ProtectsArea1", 1);
        }

        if (deathZone_Area2 == null)
        {
            deathZone_Area2 = CreateDeathZoneObject("DeathZone_ProtectsArea2", 2);
        }

        if (deathZone_Area3 == null)
        {
            deathZone_Area3 = CreateDeathZoneObject("DeathZone_ProtectsArea3", 3);
        }

        if (deathZone_Area4 == null)
        {
            deathZone_Area4 = CreateDeathZoneObject("DeathZone_ProtectsArea4", 4);
        }

        Debug.Log("[DeathZoneAutoSetup] Death zone GameObjects created! Now assign your visual prefabs and click 'Setup Death Zones'.");
    }

    private AreaDeathZone CreateDeathZoneObject(string name, int protectedAreaId)
    {
        GameObject go = new GameObject(name);
        go.transform.position = Vector3.zero;
        go.transform.SetParent(transform);

        // Add components
        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 10f;

        AreaDeathZone zone = go.AddComponent<AreaDeathZone>();

        Debug.Log($"[DeathZoneAutoSetup] Created {name}");
        return zone;
    }

    [ContextMenu("Reset Setup")]
    public void ResetSetup()
    {
        isSetupComplete = false;
        Debug.Log("[DeathZoneAutoSetup] Setup reset. You can run Setup Death Zones again.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (areaManager == null || areaManager.areas == null) return;

        DrawDeathZoneGizmo(deathZone_Area1, 0, Color.yellow);
        DrawDeathZoneGizmo(deathZone_Area2, 1, Color.green);
        DrawDeathZoneGizmo(deathZone_Area3, 2, new Color(1f, 0.5f, 0f));
        DrawDeathZoneGizmo(deathZone_Area4, 3, Color.red);
    }

    private void DrawDeathZoneGizmo(AreaDeathZone zone, int areaIndex, Color color)
    {
        if (zone == null || areaIndex >= areaManager.areas.Count) return;

        float areaRadius = areaManager.areas[areaIndex].innerRadius;
        float deathZoneRadius = areaRadius - radiusOffset;

        Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
        DrawCircle(Vector3.zero, areaRadius);

        Gizmos.color = color;
        DrawCircle(Vector3.zero, deathZoneRadius);

        UnityEditor.Handles.Label(
            new Vector3(0, deathZoneRadius + 2, 0),
            $"Area {areaIndex + 1} Death Zone\nArea Radius: {areaRadius}\nDeath Radius: {deathZoneRadius}\nOffset: {radiusOffset}",
            new GUIStyle() { normal = new GUIStyleState() { textColor = color } }
        );
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        const int points = 80;
        float step = (Mathf.PI * 2f) / points;

        Vector3 prev = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

        for (int i = 1; i <= points; i++)
        {
            float angle = step * i;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
