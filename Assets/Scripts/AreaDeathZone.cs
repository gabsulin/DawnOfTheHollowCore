using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class AreaDeathZone : MonoBehaviour
{
    [Header("Zone Configuration")]
    [Tooltip("Which area this death zone protects (1, 2, 3, or 4)")]
    [SerializeField] private int protectedAreaId = 1;

    [Tooltip("Buffer distance INSIDE the area boundary (prevents NPC spawn before death)")]
    [SerializeField] private float radiusOffset = 2f;

    [Tooltip("Instant death or damage over time?")]
    [SerializeField] private bool instantDeath = true;

    [Tooltip("Damage per second if not instant death")]
    [SerializeField] private int damagePerSecond = 50;

    public int ProtectedAreaId => protectedAreaId;

    [Header("Particle Settings")]
    [Tooltip("Particle system prefab")]
    [SerializeField] private GameObject particleIndicatorPrefab;

    [Tooltip("Base particle count (multiplied by area ID)")]
    [SerializeField] private int baseParticleCount = 100;

    [Header("Warning Settings")]
    [Tooltip("Show warning message when entering")]
    [SerializeField] private bool showWarningMessage = true;

    [Tooltip("Warning message to display")]
    [SerializeField] private string warningMessage = "This area is locked!";

    [Header("Audio")]
    [SerializeField] private string deathZoneSoundName = "ZapDeath";

    private CircleCollider2D zoneCollider;
    private GameObject particleVisual;
    private PlayerHpSystem playerInZone;
    private float damageTimer = 0f;
    private bool isPlayerInside = false;
    private bool zoneDeactivated = false;

    private static AreaDeathZone[] allDeathZones;

    private void Awake()
    {
        zoneCollider = GetComponent<CircleCollider2D>();
        zoneCollider.isTrigger = true;

        if (allDeathZones == null)
        {
            allDeathZones = FindObjectsByType<AreaDeathZone>(FindObjectsSortMode.None);
            System.Array.Sort(allDeathZones, (a, b) => a.protectedAreaId.CompareTo(b.protectedAreaId));
        }

        CreateParticleIndicator();
    }

    private void Start()
    {
        if (AreaUnlockManager.Instance != null)
        {
            if (AreaUnlockManager.Instance.IsAreaPermanentlyUnlocked(protectedAreaId))
            {
                Debug.Log($"[AreaDeathZone] Area {protectedAreaId} already unlocked - deactivating zone");
                DeactivateZone();
                return;
            }
        }

        UpdateParticleVisibility();
    }

    private void Update()
    {
        if (zoneDeactivated) return;

        if (protectedAreaId == 1 && particleVisual != null && particleVisual.activeSelf)
        {
            if (AreaUnlockManager.Instance != null)
            {
                if (AreaUnlockManager.Instance.IsNPCDialogueCompleted(1))
                {
                    particleVisual.SetActive(false);
                    Debug.Log($"[AreaDeathZone] Area 1 particles hidden (dialogue complete)");

                    RefreshAllDeathZoneVisuals();
                    return;
                }
            }
        }

        UpdateParticleVisibility();

        if (!instantDeath && !isPlayerInside && playerInZone != null)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 1f)
            {
                playerInZone.TakeHit(1);
                damageTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (AreaUnlockManager.Instance != null)
            {
                if (AreaUnlockManager.Instance.TryUnlockArea(protectedAreaId))
                {
                    Debug.Log($"[AreaDeathZone] Area {protectedAreaId} unlocked! Player may pass.");
                    DeactivateZone();

                    RefreshAllDeathZoneVisuals();
                    return;
                }
            }

            playerInZone = collision.GetComponent<PlayerHpSystem>();

            if (playerInZone != null)
            {
                if (instantDeath)
                {
                    Debug.LogWarning($"[AreaDeathZone] Player tried to exit into locked Area {protectedAreaId} - DEATH!");

                    playerInZone.currentHp = 0;
                    playerInZone.Die();

                    AudioManager.Instance?.PlaySFX(deathZoneSoundName);

                    if (RespawnManager.Instance != null)
                    {
                        RespawnManager.Instance.TriggerRespawn();
                    }
                }
                else
                {
                    Debug.Log($"[AreaDeathZone] Player exited into locked Area {protectedAreaId} - taking damage!");
                }

                if (showWarningMessage)
                {
                    ShowWarning();
                }
            }
        }
    }

    private void CreateParticleIndicator()
    {
        if (particleIndicatorPrefab == null)
        {
            Debug.LogWarning($"[AreaDeathZone] Area {protectedAreaId} - Particle indicator prefab not assigned!");
            return;
        }

        float radius = zoneCollider != null ? zoneCollider.radius : 10f;

        particleVisual = Instantiate(particleIndicatorPrefab, transform);
        particleVisual.transform.localPosition = Vector3.zero;
        particleVisual.name = $"ParticleIndicator_Area{protectedAreaId}";

        AreaDangerParticles particleScript = particleVisual.GetComponent<AreaDangerParticles>();
        if (particleScript != null)
        {
            particleScript.SetRadius(radius);

            int particleCount = baseParticleCount * protectedAreaId;
            particleScript.SetParticleCount(particleCount);

            Debug.Log($"[AreaDeathZone] Area {protectedAreaId} particles created: {particleCount} particles at radius {radius}");
        }
        else
        {
            Debug.LogError($"[AreaDeathZone] Particle prefab missing AreaDangerParticles component!");
        }

        particleVisual.SetActive(false);
    }
    private void UpdateParticleVisibility()
    {
        if (particleVisual == null || zoneDeactivated) return;
        if (AreaUnlockManager.Instance == null) return;

        bool shouldShow = IsNextLockedArea();

        if (protectedAreaId == 1 && shouldShow)
        {
            if (AreaUnlockManager.Instance.IsNPCDialogueCompleted(1))
            {
                shouldShow = false;
                Debug.Log($"[AreaDeathZone] Area 1 dialogue complete - hiding particles (no key required)");
            }
        }

        if (particleVisual.activeSelf != shouldShow)
        {
            particleVisual.SetActive(shouldShow);

            if (shouldShow)
            {
                Debug.Log($"[AreaDeathZone] Area {protectedAreaId} particles now VISIBLE (next locked area)");
            }
            else
            {
                Debug.Log($"[AreaDeathZone] Area {protectedAreaId} particles now HIDDEN");
            }
        }
    }

    private bool IsNextLockedArea()
    {
        if (AreaUnlockManager.Instance == null) return false;

        if (AreaUnlockManager.Instance.IsAreaPermanentlyUnlocked(protectedAreaId))
        {
            return false;
        }

        for (int i = 1; i < protectedAreaId; i++)
        {
            if (!AreaUnlockManager.Instance.IsAreaPermanentlyUnlocked(i))
            {
                return false;
            }
        }

        return true;
    }

    private static void RefreshAllDeathZoneVisuals()
    {
        if (allDeathZones == null) return;

        foreach (var zone in allDeathZones)
        {
            if (zone != null && !zone.zoneDeactivated)
            {
                zone.UpdateParticleVisibility();
            }
        }

        Debug.Log("[AreaDeathZone] All death zone visuals refreshed");
    }
    public void DeactivateZone()
    {
        if (zoneDeactivated) return;

        if (zoneCollider != null)
        {
            zoneCollider.enabled = false;
        }

        if (particleVisual != null)
        {
            particleVisual.SetActive(false);
        }

        zoneDeactivated = true;

        Debug.Log($"[AreaDeathZone] Area {protectedAreaId} death zone deactivated");
    }

    private void ShowWarning()
    {
        if (AreaUnlockManager.Instance != null)
        {
            string requirement = AreaUnlockManager.Instance.GetAreaRequirementText(protectedAreaId);
            Debug.LogWarning($"[AreaDeathZone] {warningMessage} - {requirement}");
        }
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
            int particleCount = baseParticleCount * protectedAreaId;
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * col.radius,
                $"Death Zone - Area {protectedAreaId}\n(Kills on EXIT)\nRadius: {col.radius}\nParticles: {particleCount}",
                new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } }
            );
#endif
        }
    }
}