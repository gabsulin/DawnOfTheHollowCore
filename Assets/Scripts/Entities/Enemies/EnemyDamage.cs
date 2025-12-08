using System.Collections;
using UnityEngine;
using TMPro;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private ParticleSystem boomParticles;
    [SerializeField] public int damage;

    [Header("Damage Text Settings")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private float textSpawnRadius = 0.5f;
    [SerializeField] private Transform uiCanvasTransform;
    [SerializeField] private Camera mainCamera;

    private bool hasHitEnemy = false;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) Debug.LogError("Main Camera not found!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (uiCanvasTransform == null)
        {
            Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            if (canvas != null) uiCanvasTransform = canvas.transform;
            if (uiCanvasTransform == null) Debug.LogError("UI Canvas Transform not assigned!");
        }
        HandleCollision(collision.collider, collision.contacts[0].point);
        //PlayerController.Instance.isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (uiCanvasTransform == null)
        {
            Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            if (canvas != null) uiCanvasTransform = canvas.transform;
            if (uiCanvasTransform == null) Debug.LogError("UI Canvas Transform not assigned!");
        }
        HandleCollision(collider, collider.transform.position);
        PlayerController.Instance.isAttacking = false;
    }

    private void HandleCollision(Collider2D collider, Vector3 hitPosition)
    {
        if (hasHitEnemy) return;
        if (!enabled) return;

        /*Weapon weapon = PlayerController.Instance?.GetCurrentWeapon();
        bool isMelee = weapon is Melee;
        var state = player._playerState;*/

        /*if(isMelee)
        {
            if ((collider.CompareTag("Enemy") && state == PlayerObj.PlayerState.attack) ||
            (collider.CompareTag("MiniBoss") && state == PlayerObj.PlayerState.attack))
            {
                HandleAttack(collider, isMelee);
            }
        } else
        {
            HandleAttack(collider, isMelee);
        }*/
    }

    private void HandleAttack(Collider2D collider, bool isMelee)
    {
        int actualDamageDealt = damage;
        Vector3 enemyCenterPosition = collider.bounds.center;

        /*if (collider.CompareTag("MiniBoss"))
        {
            var bossHp = collider.GetComponent<BossHpSystem>();
            if (bossHp != null)
            {
                bossHp.TakeDamage(actualDamageDealt, isMelee);
                ShowDamageNumber(actualDamageDealt, enemyCenterPosition);
            }
        }
        else
        {
            if (TryDealDamageToEnemy(collider, actualDamageDealt))
            {
                ShowDamageNumber(actualDamageDealt, enemyCenterPosition);
            }
        }*/
    }

    private bool TryDealDamageToEnemy(Component target, int damageToDeal)
    {
        var enemyHp = target.GetComponent<EnemyHealth>();
        if (enemyHp != null)
        {
            enemyHp.TakeDamage(damageToDeal);
            return true;
        }

        return false;
    }

    private void ShowDamageNumber(int damageAmount, Vector3 enemyWorldPosition)
    {
        float randomX = Random.Range(-textSpawnRadius, textSpawnRadius);
        float randomY = Random.Range(-textSpawnRadius, textSpawnRadius);
        Vector3 textWorldPosition = enemyWorldPosition + new Vector3(randomX, randomY, 0f);
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(textWorldPosition);

        GameObject damageTextInstance = Instantiate(damageTextPrefab, uiCanvasTransform);
        RectTransform canvasRect = uiCanvasTransform.GetComponent<RectTransform>();
        RectTransform damageRect = damageTextInstance.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 localPoint))
        {
            damageRect.anchoredPosition = localPoint;
        }

        DamageText animator = damageTextInstance.GetComponent<DamageText>();
        if (animator != null)
        {
            animator.SetText(damageAmount.ToString());
        }
        else
        {
            TextMeshProUGUI tmpText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = damageAmount.ToString();
            }
        }
    }
}
