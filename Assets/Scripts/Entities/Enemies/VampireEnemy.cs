using UnityEngine;

public class VampireEnemy : Enemy
{
    [Header("Vampire Settings")]
    [SerializeField] public Transform centerOfAttack;
    [SerializeField] float attackRadius;
    [SerializeField] GameObject batPrefab;
    public bool hasRevived = false;

    public void HandleDeath()
    {
        if (!hasRevived)
        {
            VampireAbility();
        }
        else
        {
            EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.DestroyAfterDeath();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        attackCooldown = animator.GetFloat("AttackCooldown");
    }

    private void Update()
    {
        attackCooldown += Time.deltaTime;
        animator.SetFloat("AttackCooldown", attackCooldown);
    }

    public override void Attack()
    {
        currentState = EnemyState.Attack;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerOfAttack.position, attackRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                var playerHp = collider.GetComponent<PlayerHpSystem>();
                if (playerHp != null) playerHp.TakeHit(damage);
            }
            if (collider.gameObject.GetComponent<Core>() != null)
            {
                var hp = collider.gameObject.GetComponent<CoreHpSystem>();
                if (hp != null) hp.TakeHit(damage);
                attackCooldown = 1.5f;
                DeactivateAttackHitbox();
            }
        }
    }

    private void VampireAbility()
    {
        GameObject batInstance = Instantiate(batPrefab, transform.position, Quaternion.identity);
        Bat batScript = batInstance.GetComponent<Bat>();

        if (batScript != null)
        {
            batScript.SetOriginalVampire(this);
        }
        else
        {
            Debug.LogError("Bat prefab doesn't have Bat script attached!");
        }

        hasRevived = true;
        gameObject.SetActive(false);
    }

    public void Reactivate(Vector2 revivalPosition)
    {
        transform.position = revivalPosition;
        gameObject.SetActive(true);

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.RestoreFullHealth();
        }

        Debug.Log(gameObject.name + " was reactivated at position " + revivalPosition);
    }

    public override void OnCollisionEnter2D(Collision2D collision) { }

    protected override void OnDrawGizmos()
    {
        if (centerOfAttack != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerOfAttack.position, attackRadius);
        }
    }
}