using UnityEngine;

public class PlantEnemy : Enemy
{
    [Header("Plant Settings")]
    [SerializeField] Rigidbody2D seedPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float[] angles;

    //CameraShake shake;
    PlayerHpSystem playerHp;
    protected override void Start()
    {
        attackCooldown = animator.GetFloat("AttackCooldown");
        player = FindFirstObjectByType<PlayerController>().transform;
        playerHp = FindFirstObjectByType<PlayerHpSystem>();
        Transform aimTargetParent = player.transform;
        aimTarget = aimTargetParent.Find("AimTarget");
        //shake = GetComponent<CameraShake>();
    }
    private void Update()
    {
        attackCooldown += Time.deltaTime;
        animator.SetFloat("AttackCooldown", attackCooldown);
    }
    public override void Attack()
    {
        currentState = EnemyState.Attack;
        if (seedPrefab != null && spawnPoint != null && !playerHp.isDead)
        {
            Vector2 baseDirection = (aimTarget.transform.position - spawnPoint.position).normalized;

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
        if (currentState == EnemyState.Attack && collision.gameObject.CompareTag("Player") && (attackHitbox != null || collision.collider.IsTouching(attackHitbox)))
        {
            PlayerHpSystem playerHp = collision.collider.GetComponent<PlayerHpSystem>();
            playerHp.TakeHit(damage);
            currentState = EnemyState.Idle;
            attackCooldown = 0f;
            //shake.StartShake(force: 0.1f);
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
            ExecuteIdleState();
            DeactivateAttackHitbox();
        }
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
