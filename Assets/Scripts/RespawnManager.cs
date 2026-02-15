using UnityEngine;
using System.Collections;
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("Delay before respawning")]
    [SerializeField] private float respawnDelay = 2f;

    private PlayerController playerController;
    private Animator anim;
    private PlayerHpSystem playerHpSystem;
    private Rigidbody2D playerRb;
    private bool isRespawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        playerController = PlayerController.Instance;
        anim = playerController.GetComponent<Animator>();

        if (playerController != null)
        {
            playerHpSystem = playerController.GetComponent<PlayerHpSystem>();
            playerRb = playerController.GetComponent<Rigidbody2D>();
        }

        if (respawnPoint == null)
        {
            GameObject respawnGO = new GameObject("DefaultRespawnPoint");
            respawnPoint = respawnGO.transform;
            respawnPoint.position = Vector3.zero;
            Debug.LogWarning("[RespawnManager] No respawn point set, using origin (0,0,0)");
        }
    }
    public void TriggerRespawn()
    {
        if (isRespawning) return;

        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(respawnDelay);

        if (playerController != null)
        {
            playerController.transform.position = respawnPoint.position;

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        if (playerHpSystem != null)
        {
            playerHpSystem.currentHp = playerHpSystem.maxHp;
            playerHpSystem.currentShields = playerHpSystem.maxShields;

            playerHpSystem.isDead = false;
            anim.SetBool("IsDead", false);
            playerHpSystem.UpdateUI();
        }

        if (playerController != null)
        {
            playerController.canMove = true;
            playerController.canAttack = true;
        }

        AudioManager.Instance?.PlaySFX("Respawn");

        isRespawning = false;

        Debug.Log("[RespawnManager] Player respawned");
    }
    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
        Debug.Log($"[RespawnManager] Respawn point updated to: {newRespawnPoint.position}");
    }
    public Vector3 GetRespawnPosition()
    {
        return respawnPoint != null ? respawnPoint.position : Vector3.zero;
    }
}