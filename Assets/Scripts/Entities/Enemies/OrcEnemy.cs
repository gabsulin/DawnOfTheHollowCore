using UnityEngine;

public class OrcEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
        player = FindFirstObjectByType<PlayerController>().transform;
    }

    private void Update()
    {
        // nothing animation-related here anymore
    }

    public override void Attack()
    {
        // handled by EnemyPathfinder
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && attackHitbox != null)
        {
            if (collision.collider.IsTouching(attackHitbox))
            {
                PlayerHpSystem playerHp = collision.collider.GetComponent<PlayerHpSystem>();
                playerHp.TakeHit(damage);


                // knockback
                player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
                DeactivateAttackHitbox();
            }
        }
        if (collision.gameObject.GetComponent<Core>() != null)
        {
            var hp = collision.gameObject.GetComponent<CoreHpSystem>();
            if (hp != null) hp.TakeHit(damage);

            DeactivateAttackHitbox();
        }

    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1);
    }
}
