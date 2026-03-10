using UnityEngine;

public class OrcEnemy : Enemy
{
    [Header("Orc Attack Settings")]
    [SerializeField] private Transform centerOfAttack;
    [SerializeField] private float attackRadius = 1f;

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
        currentState = EnemyState.Attack;

        Collider2D[] hits = Physics2D.OverlapCircleAll(centerOfAttack.position, attackRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHpSystem playerHp = hit.GetComponent<PlayerHpSystem>();
                if (playerHp != null) playerHp.TakeHit(damage);
                cameraShake.StartShake(force: 0.1f);
            }

            if (hit.GetComponent<Core>() != null)
            {
                CoreHpSystem coreHp = hit.GetComponent<CoreHpSystem>();
                if (coreHp != null) coreHp.TakeHit(damage);
            }
        }
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