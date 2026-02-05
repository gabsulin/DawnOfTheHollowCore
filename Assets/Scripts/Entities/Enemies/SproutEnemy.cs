using UnityEngine;

public class SproutEnemy : Enemy
{
    [Header("Sprout Settings")]
    [SerializeField] private Transform centerOfAttack;
    [SerializeField] private float attackRadius = 1.5f;

    protected override void Start()
    {
        base.Start();

        if (centerOfAttack == null)
        {
            centerOfAttack = transform;
        }
    }

    void Update()
    {
        if (currentState == EnemyState.Death)
            return;
    }

    public override void Attack()
    {
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (centerOfAttack == null)
            return;

        currentState = EnemyState.Attack;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerOfAttack.position, attackRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerHpSystem playerHp = col.GetComponent<PlayerHpSystem>();
                if (playerHp != null && !playerHp.isDead)
                {
                    playerHp.TakeHit(damage);
                    Debug.Log($"Sprout hit player for {damage} damage!");
                }
            }

            Core coreComponent = col.GetComponent<Core>();
            if (coreComponent != null)
            {
                CoreHpSystem coreHp = col.GetComponent<CoreHpSystem>();
                if (coreHp != null && !coreHp.isDead)
                {
                    coreHp.TakeHit(damage);
                    Debug.Log($"Sprout hit core for {damage} damage!");
                }
            }
        }
    }

    public override void OnCollisionEnter2D(Collision2D collision) { }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (centerOfAttack != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerOfAttack.position, attackRadius);
        }
        else if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, playerAggroRange);
    }
}