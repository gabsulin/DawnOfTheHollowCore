using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Death
}

public abstract class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform aimTarget;
    public GridManager grid;
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Attack Settings")]
    public Collider2D attackHitbox;
    public float attackCooldown = 1.5f;
    public int damage = 1;

    public EnemyState currentState = EnemyState.Idle;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = FindFirstObjectByType<PlayerController>().transform;
        grid = FindFirstObjectByType<GridManager>();

        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    public abstract void Attack();

    public virtual void ActivateAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = true;
    }

    protected virtual void DeactivateAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    public virtual void ExecuteIdleState()
    {
        currentState = EnemyState.Idle;
    }

    public abstract void OnCollisionEnter2D(Collision2D collision);

    public virtual void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (rb != null)
        {
            Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
            rb.AddForce(dir * force, ForceMode2D.Impulse);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (attackHitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackHitbox.bounds.center, attackHitbox.bounds.size);
        }
    }
}
