using UnityEngine;

/// <summary>
/// Creates a rotating particle ring indicator for locked areas
/// Attach to a prefab with ParticleSystem component
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class AreaDangerParticles : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private Color dangerColor = new Color(1f, 0.3f, 0f, 1f); // Orange-red
    [SerializeField] private float particleSize = 0.5f;
    [SerializeField] private int baseParticleCount = 100; // Can be overridden
    [SerializeField] private float ringRadius = 10f;

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = 10;
    [SerializeField] private string sortingLayerName = "Default";

    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 30f; // degrees per second
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    private ParticleSystem ps;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystemRenderer psRenderer;
    private float originalSize;
    private int currentParticleCount;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        currentParticleCount = baseParticleCount; // Initialize with base count
        ConfigureParticleSystem();
    }

    private void ConfigureParticleSystem()
    {
        mainModule = ps.main;
        shapeModule = ps.shape;
        emissionModule = ps.emission;
        var colorOverLifetime = ps.colorOverLifetime;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // Main module settings
        mainModule.startColor = dangerColor;
        mainModule.startSize = particleSize;
        mainModule.startLifetime = 2f;
        mainModule.startSpeed = 0f;
        mainModule.maxParticles = currentParticleCount;
        mainModule.loop = true;
        mainModule.playOnAwake = true;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

        // Shape module - circle ring
        shapeModule.shapeType = ParticleSystemShapeType.Circle;
        shapeModule.radius = ringRadius;
        shapeModule.radiusThickness = 0.05f; // Very thin ring
        shapeModule.arc = 360f;
        shapeModule.arcMode = ParticleSystemShapeMultiModeValue.Random;

        // Emission
        emissionModule.rateOverTime = currentParticleCount / 2f;

        // Color over lifetime for pulsing effect
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(dangerColor, 0.0f),
                new GradientColorKey(dangerColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // Renderer settings - make sure it's visible!
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = sortingOrder; // Use configured sorting order
            renderer.sortingLayerName = sortingLayerName;
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetColor("_Color", dangerColor);
        }

        // Start the system
        ps.Clear();
        ps.Play();

        originalSize = particleSize;

        Debug.Log($"[AreaDangerParticles] Particle system configured and playing. Radius: {ringRadius}");
    }

    private void Update()
    {
        // Rotate the entire particle system
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Pulse effect
        if (pulseEffect)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            mainModule.startSize = originalSize * pulse;
        }
    }

    /// <summary>
    /// Set the radius of the danger ring (should match death zone radius)
    /// </summary>
    public void SetRadius(float radius)
    {
        ringRadius = radius;

        // Force reconfiguration with new radius
        if (ps != null)
        {
            // Update shape module
            shapeModule = ps.shape;
            shapeModule.radius = radius;
            shapeModule.radiusThickness = 0.05f;

            // Clear and restart to apply changes
            ps.Clear();
            ps.Play();

            Debug.Log($"[AreaDangerParticles] Radius updated to {radius}");
        }
        else
        {
            Debug.LogWarning("[AreaDangerParticles] Cannot set radius - particle system not found!");
        }
    }

    /// <summary>
    /// Set the number of particles (scales by area difficulty)
    /// </summary>
    public void SetParticleCount(int count)
    {
        currentParticleCount = count;

        if (ps != null)
        {
            mainModule = ps.main;
            emissionModule = ps.emission;

            mainModule.maxParticles = currentParticleCount;
            emissionModule.rateOverTime = currentParticleCount / 2f;

            // Clear and restart to apply changes
            ps.Clear();
            ps.Play();

            Debug.Log($"[AreaDangerParticles] Particle count set to {currentParticleCount}");
        }
    }

    /// <summary>
    /// Change the danger color
    /// </summary>
    public void SetColor(Color color)
    {
        dangerColor = color;
        mainModule.startColor = color;
    }

    private void OnDisable()
    {
        if (ps != null && ps.isPlaying)
        {
            ps.Stop();
        }
    }
}