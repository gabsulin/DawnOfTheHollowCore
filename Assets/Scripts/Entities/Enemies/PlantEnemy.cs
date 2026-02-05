using UnityEngine;

public class PlantEnemy : Enemy
{
    [Header("Plant Settings")]
    [SerializeField] Rigidbody2D seedPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float[] angles;

    protected override void Start()
    {
        base.Start();
        playerHp = FindFirstObjectByType<PlayerHpSystem>();

        if (player != null)
        {
            Transform aimTargetParent = player.transform;
            aimTarget = aimTargetParent.Find("AimTarget");
        }
    }

    private void Update()
    {
    }

    public override void Attack()
    {
        currentState = EnemyState.Attack;
        if (seedPrefab != null && spawnPoint != null && !playerHp.isDead)
        {
            Transform currentTarget = GetCurrentTarget();
            Vector2 targetAimPos = currentTarget == player && aimTarget != null
                ? aimTarget.position
                : currentTarget.position;

            Vector2 baseDirection = (targetAimPos - (Vector2)spawnPoint.position).normalized;

            foreach (float angle in angles)
            {
                Vector2 rotatedDirection = RotateVector(baseDirection, angle);
                var seed = Instantiate(seedPrefab, spawnPoint.position, Quaternion.identity);
                seed.AddForce(rotatedDirection * 5, ForceMode2D.Impulse);
            }
        }
        ExecuteIdleState();
    }

    private Vector2 RotateVector(Vector2 v, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.Attack && (attackHitbox != null || collision.collider.IsTouching(attackHitbox)))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHpSystem playerHp = collision.collider.GetComponent<PlayerHpSystem>();
                playerHp.TakeHit(damage);
                currentState = EnemyState.Idle;
                player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
                ExecuteIdleState();
                DeactivateAttackHitbox();
            }

            if (collision.gameObject.GetComponent<Core>() != null)
            {
                var hp = collision.gameObject.GetComponent<CoreHpSystem>();
                if (hp != null) hp.TakeHit(damage);
                DeactivateAttackHitbox();
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}