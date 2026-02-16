using UnityEngine;
using System.Collections.Generic;

public class VisualIndicatorTester : MonoBehaviour
{
    [Header("Quick Test Settings")]
    [Tooltip("Test at what radius?")]
    [SerializeField] private float testRadius = 15f;

    [Tooltip("Which style to preview?")]
    [SerializeField] private StylePreview previewStyle = StylePreview.Particles;

    [Header("Test Objects")]
    [SerializeField] private GameObject particleTestPrefab;
    [SerializeField] private GameObject barrierTestPrefab;

    private GameObject activeParticleTest;
    private GameObject activeBarrierTest;

    public enum StylePreview
    {
        Particles,
        None
    }

    private void Start()
    {
        SpawnTestVisuals();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            RefreshTestVisuals();
        }
    }

    [ContextMenu("Spawn Test Visuals")]
    private void SpawnTestVisuals()
    {
        CleanupTestVisuals();

        switch (previewStyle)
        {
            case StylePreview.Particles:
                SpawnParticleTest();
                break;

            case StylePreview.None:
                break;
        }
    }

    private void SpawnParticleTest()
    {
        if (particleTestPrefab != null)
        {
            activeParticleTest = Instantiate(particleTestPrefab, transform);
            activeParticleTest.name = "ParticleTest";
            
            var particleScript = activeParticleTest.GetComponent<AreaDangerParticles>();
            if (particleScript != null)
            {
                particleScript.SetRadius(testRadius);
            }
            
            Debug.Log($"[VisualTester] Spawned particle indicator at radius {testRadius}");
        }
        else
        {
            Debug.LogWarning("[VisualTester] Particle prefab not assigned!");
        }
    }

    [ContextMenu("Cleanup Test Visuals")]
    private void CleanupTestVisuals()
    {
        if (activeParticleTest != null)
        {
            DestroyImmediate(activeParticleTest);
        }
        
        if (activeBarrierTest != null)
        {
            DestroyImmediate(activeBarrierTest);
        }
    }

    [ContextMenu("Refresh Test Visuals")]
    private void RefreshTestVisuals()
    {
        CleanupTestVisuals();
        SpawnTestVisuals();
    }

    private void OnDestroy()
    {
        CleanupTestVisuals();
    }

    private void OnDrawGizmos()
    {
        // Draw the test radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        DrawCircle(transform.position, testRadius);
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
}

/*
 * ========================================
 * HOW TO USE THIS TESTER
 * ========================================
 * 
 * 1. Create empty GameObject: "VisualTester"
 * 2. Add this script
 * 3. Create your particle and barrier prefabs
 * 4. Assign them to this script
 * 5. In Inspector, change:
 *    - Test Radius (15, 30, 50, 75 for different areas)
 *    - Preview Style (Particles, Barrier, Both)
 * 6. Hit Play to see them in action!
 * 
 * COMPARISON TIPS:
 * - Particles look better at small-medium radius (15-30)
 * - Barriers look better at large radius (50-75)
 * - "Both" gives maximum visibility
 * - Adjust colors and speeds to match your game's aesthetic
 * 
 * When you've decided, set your AreaDeathZones to use that style!
 */
