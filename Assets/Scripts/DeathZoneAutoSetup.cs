using UnityEngine;

/// <summary>
/// Helper script to automatically setup death zones with correct radii based on AreaManager configuration
/// Attach this to your scene and click "Setup Death Zones" in the inspector
/// </summary>
public class DeathZoneAutoSetup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your AreaManager with area configurations")]
    [SerializeField] private AreaManager areaManager;

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

        // Setup Area 1 death zone (uses Area 0's outer radius)
        if (deathZone_Area1 != null)
        {
            float radius = areaManager.areas[0].outerRadius;
            SetupDeathZone(deathZone_Area1, 1, radius);
            successCount++;
        }

        // Setup Area 2 death zone (uses Area 1's outer radius)
        if (deathZone_Area2 != null)
        {
            float radius = areaManager.areas[1].outerRadius;
            SetupDeathZone(deathZone_Area2, 2, radius);
            successCount++;
        }

        // Setup Area 3 death zone (uses Area 2's outer radius)
        if (deathZone_Area3 != null)
        {
            float radius = areaManager.areas[2].outerRadius;
            SetupDeathZone(deathZone_Area3, 3, radius);
            successCount++;
        }

        // Setup Area 4 death zone (uses Area 3's outer radius)
        if (deathZone_Area4 != null)
        {
            float radius = areaManager.areas[3].outerRadius;
            SetupDeathZone(deathZone_Area4, 4, radius);
            successCount++;
        }

        isSetupComplete = true;
        Debug.Log($"[DeathZoneAutoSetup] Setup complete! Configured {successCount} death zones.");
    }

    private void SetupDeathZone(AreaDeathZone deathZone, int protectedAreaId, float radius)
    {
        // Get the collider
        CircleCollider2D collider = deathZone.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            Debug.LogError($"[DeathZoneAutoSetup] Death zone for Area {protectedAreaId} is missing CircleCollider2D!");
            return;
        }

        // Set the radius
        collider.radius = radius;
        collider.isTrigger = true;

        // Ensure it's at world center
        deathZone.transform.position = Vector3.zero;

        Debug.Log($"[DeathZoneAutoSetup] Area {protectedAreaId} death zone: Radius set to {radius}");
    }

    [ContextMenu("Create Death Zone GameObjects")]
    public void CreateDeathZoneGameObjects()
    {
        // Create death zone game objects if they don't exist
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
        collider.radius = 10f; // Temporary, will be set by Setup

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

        // Draw each death zone radius as a colored circle
        DrawDeathZoneGizmo(deathZone_Area1, 0, Color.yellow);
        DrawDeathZoneGizmo(deathZone_Area2, 1, Color.green);
        DrawDeathZoneGizmo(deathZone_Area3, 2, new Color(1f, 0.5f, 0f));
        DrawDeathZoneGizmo(deathZone_Area4, 3, Color.red);
    }

    private void DrawDeathZoneGizmo(AreaDeathZone zone, int areaIndex, Color color)
    {
        if (zone == null || areaIndex >= areaManager.areas.Count) return;

        float radius = areaManager.areas[areaIndex].outerRadius;

        Gizmos.color = color;
        DrawCircle(Vector3.zero, radius);

        UnityEditor.Handles.Label(
            new Vector3(0, radius + 2, 0),
            $"Area {areaIndex + 1} Death Zone\nRadius: {radius}",
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

/*
 * ========================================
 * HOW TO USE THIS AUTO-SETUP
 * ========================================
 * 
 * 1. Create an empty GameObject: "DeathZoneAutoSetup"
 * 2. Add this script
 * 3. Assign your AreaManager
 * 4. Right-click the script in Inspector  "Create Death Zone GameObjects"
 * 5. For each created death zone, configure in Inspector:
 *    - Protected Area Id (1, 2, 3, or 4)
 *    - Visual Style (Particles, Barrier, or Both)
 *    - Particle Indicator Prefab (if using particles)
 *    - Barrier Indicator Prefab (if using barrier)
 * 6. Right-click the script  "Setup Death Zones"
 * 7. Done! All radii are automatically set from your AreaManager!
 * 
 * GIZMOS:
 * - Yellow circle = Area 1 death zone
 * - Orange circle = Area 2 death zone
 * - Dark orange = Area 3 death zone
 * - Red circle = Area 4 death zone
 * 
 * You can see exactly where each death zone will be!
 */