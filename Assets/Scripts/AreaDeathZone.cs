using UnityEngine;

/// <summary>
/// Death zone placed at area boundaries (INSIDE current safe area)
/// Kills player when they try to EXIT into a locked area
/// Position this at the INNER edge of the LOCKED area (same as current area's outer radius)
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class AreaDeathZone : MonoBehaviour
{
    [Header("Zone Configuration")]
    [Tooltip("Which area this death zone protects (2, 3, or 4)")]
    [SerializeField] private int protectedAreaId = 2;

    [Tooltip("Instant death or damage over time?")]
    [SerializeField] private bool instantDeath = true;

    [Tooltip("Damage per second if not instant death")]
    [SerializeField] private int damagePerSecond = 50;

    // Public getter for the protected area ID
    public int ProtectedAreaId => protectedAreaId;

    [Header("Visual Indicator")]
    [Tooltip("Which visual style to use")]
    [SerializeField] private VisualStyle visualStyle = VisualStyle.Particles;

    [Tooltip("Particle system for danger indicator (Option 1)")]
    [SerializeField] private GameObject particleIndicatorPrefab;

    [Tooltip("Sprite-based barrier (Option 2)")]
    [SerializeField] private GameObject barrierIndicatorPrefab;

    [Header("Warning Settings")]
    [Tooltip("Show warning message when entering")]
    [SerializeField] private bool showWarningMessage = true;

    [Tooltip("Warning message to display")]
    [SerializeField] private string warningMessage = "This area is locked!";

    [Header("Audio")]
    [SerializeField] private string deathZoneSoundName = "ZapDeath";

    public enum VisualStyle
    {
        Particles,
        Barrier,
        Both,
        None
    }

    private CircleCollider2D zoneCollider;
    private GameObject activeVisual;
    private PlayerHpSystem playerInZone;
    private float damageTimer = 0f;
    private bool isPlayerInside = false;

    private void Awake()
    {
        zoneCollider = GetComponent<CircleCollider2D>();
        zoneCollider.isTrigger = true;

        // Start with visual active (will hide if area is unlocked)
        CreateVisualIndicator();
    }

    private void Start()
    {
        // Check if area is already unlocked
        if (AreaUnlockManager.Instance != null)
        {
            if (AreaUnlockManager.Instance.IsAreaPermanentlyUnlocked(protectedAreaId))
            {
                DeactivateZone();
            }
        }
    }

    private void Update()
    {
        // Check if area requirements are met and update visuals
        if (AreaUnlockManager.Instance != null && activeVisual != null && activeVisual.activeSelf)
        {
            // For Area 1: Hide visual when NPC dialogue complete (no key needed)
            if (protectedAreaId == 1)
            {
                if (AreaUnlockManager.Instance.CanEnterArea(protectedAreaId))
                {
                    HideVisuals();
                }
            }
            // For Area 2+: Hide visual when NPC dialogue complete (visual feedback that you're making progress)
            // Death zone stays active until key is used
            else
            {
                // Check if NPC dialogue requirement is met
                if (AreaUnlockManager.Instance.CanEnterArea(protectedAreaId) ||
                    AreaUnlockManager.Instance.IsAreaPermanentlyUnlocked(protectedAreaId))
                {
                    HideVisuals();
                }
            }
        }

        // Fully deactivate death zone when area is permanently unlocked
        if (AreaUnlockManager.Instance != null && zoneCollider != null && zoneCollider.enabled)
        {
            if (AreaUnlockManager.Instance.IsAreaPermanentlyUnlocked(protectedAreaId))
            {
                DeactivateZone();
            }
        }

        // Continuous damage (if not instant death and player is OUTSIDE the safe zone)
        if (!instantDeath && !isPlayerInside && playerInZone != null)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 1f / damagePerSecond)
            {
                playerInZone.TakeHit(1); // Deal 1 damage repeatedly
                damageTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Hide visual indicators (called when NPC dialogue is complete)
    /// </summary>
    private void HideVisuals()
    {
        if (activeVisual != null)
        {
            activeVisual.SetActive(false);
            Debug.Log($"[AreaDeathZone] Visuals hidden for Area {protectedAreaId} (dialogue complete)");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player enters the safe zone (good!)
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerInZone = collision.GetComponent<PlayerHpSystem>();
            damageTimer = 0f;

            Debug.Log($"[AreaDeathZone] Player entered safe zone for Area {protectedAreaId - 1}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Player is trying to LEAVE the safe zone into locked area!
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Check if area is unlocked
            if (AreaUnlockManager.Instance != null)
            {
                // Try to unlock the area (this will consume key if player has it)
                if (AreaUnlockManager.Instance.TryUnlockArea(protectedAreaId))
                {
                    Debug.Log($"[AreaDeathZone] Area {protectedAreaId} unlocked! Player may pass.");
                    DeactivateZone();
                    return;
                }
            }

            // Area is still locked - kill or damage player
            playerInZone = collision.GetComponent<PlayerHpSystem>();

            if (playerInZone != null)
            {
                if (instantDeath)
                {
                    // INSTANT DEATH
                    playerInZone.currentHp = 0;
                    playerInZone.Die();

                    AudioManager.Instance?.PlaySFX(deathZoneSoundName);

                    // Trigger respawn
                    if (RespawnManager.Instance != null)
                    {
                        RespawnManager.Instance.TriggerRespawn();
                    }

                    Debug.Log($"[AreaDeathZone] Player tried to exit into locked Area {protectedAreaId} - instant death!");
                }
                else
                {
                    Debug.Log($"[AreaDeathZone] Player exited into locked Area {protectedAreaId} - taking damage!");
                }

                // Show warning message
                if (showWarningMessage)
                {
                    ShowWarning();
                }
            }
        }
    }

    private void CreateVisualIndicator()
    {
        if (visualStyle == VisualStyle.None) return;

        // Get the radius from our collider
        float radius = zoneCollider != null ? zoneCollider.radius : 10f;

        Debug.Log($"[AreaDeathZone] Creating visuals for Area {protectedAreaId} at radius {radius}");

        if (visualStyle == VisualStyle.Particles || visualStyle == VisualStyle.Both)
        {
            if (particleIndicatorPrefab != null)
            {
                activeVisual = Instantiate(particleIndicatorPrefab, transform);
                activeVisual.transform.localPosition = Vector3.zero;
                activeVisual.name = $"ParticleIndicator_Area{protectedAreaId}";

                // Set the radius to match the death zone
                AreaDangerParticles particleScript = activeVisual.GetComponent<AreaDangerParticles>();
                if (particleScript != null)
                {
                    particleScript.SetRadius(radius);
                    Debug.Log($"[AreaDeathZone] Particle indicator created for Area {protectedAreaId} at radius {radius}");
                }
                else
                {
                    Debug.LogError($"[AreaDeathZone] Particle prefab missing AreaDangerParticles component!");
                }
            }
            else
            {
                Debug.LogWarning("[AreaDeathZone] Particle indicator prefab not assigned!");
            }
        }

        if (visualStyle == VisualStyle.Barrier || visualStyle == VisualStyle.Both)
        {
            if (barrierIndicatorPrefab != null)
            {
                GameObject barrier = Instantiate(barrierIndicatorPrefab, transform);
                barrier.transform.localPosition = Vector3.zero;
                barrier.name = $"BarrierIndicator_Area{protectedAreaId}";

                // Set the radius to match the death zone
                AreaEnergyBarrier barrierScript = barrier.GetComponent<AreaEnergyBarrier>();
                if (barrierScript != null)
                {
                    barrierScript.SetRadius(radius);
                    Debug.Log($"[AreaDeathZone] Barrier indicator created for Area {protectedAreaId} at radius {radius}");
                }
                else
                {
                    Debug.LogError($"[AreaDeathZone] Barrier prefab missing AreaEnergyBarrier component!");
                }

                if (activeVisual == null)
                {
                    activeVisual = barrier;
                }
            }
            else
            {
                Debug.LogWarning("[AreaDeathZone] Barrier indicator prefab not assigned!");
            }
        }
    }

    /// <summary>
    /// Deactivate the death zone when area is unlocked
    /// </summary>
    public void DeactivateZone()
    {
        // Disable collider
        if (zoneCollider != null)
        {
            zoneCollider.enabled = false;
        }

        // Hide visual indicator
        if (activeVisual != null)
        {
            activeVisual.SetActive(false);
        }

        Debug.Log($"[AreaDeathZone] Area {protectedAreaId} death zone deactivated");
    }

    /// <summary>
    /// Show warning message to player
    /// </summary>
    private void ShowWarning()
    {
        // You can integrate this with your UI system
        // For now just logging

        if (AreaUnlockManager.Instance != null)
        {
            string requirement = AreaUnlockManager.Instance.GetAreaRequirementText(protectedAreaId);
            Debug.LogWarning($"[AreaDeathZone] {warningMessage} - {requirement}");
        }

        // TODO: Show UI popup with warning message
        // Example: UIManager.Instance?.ShowWarning(warningMessage);
    }

    private void OnDrawGizmos()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, col.radius);

            // Draw label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * col.radius,
                $"Death Zone - Protects Area {protectedAreaId}\n(Kills on EXIT)",
                new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } }
            );
#endif
        }
    }
}