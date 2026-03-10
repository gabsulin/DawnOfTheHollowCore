using System.Collections;
using UnityEngine;

public class SlimeEnemy : Enemy
{
    [Header("Slime Jump Settings")]
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;
    private bool isJumping = false;

    private Collider2D coreCollider;

    protected override void Start()
    {
        base.Start();

        if (coreObject != null)
            coreCollider = coreObject.GetComponent<Collider2D>();
    }

    private void Update()
    {
    }

    public override void Attack()
    {
        currentState = EnemyState.Attack;
        if (!isJumping)
            StartCoroutine(JumpAttackRoutine());
    }

    IEnumerator JumpAttackRoutine()
    {
        isJumping = true;
        Vector2 startPos = transform.position;

        Transform currentTarget = GetCurrentTarget();

        Vector2 targetPos;
        if (currentTarget == core && coreCollider != null)
            targetPos = coreCollider.ClosestPoint(startPos);
        else
            targetPos = currentTarget.position;

        float timer = 0f;
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;
            float height = 4 * jumpHeight * t * (1 - t);
            transform.position = Vector3.Lerp(startPos, targetPos + new Vector2(0.1f, 0.1f), t) + Vector3.up * height;
            yield return null;
        }

        isJumping = false;
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.Attack && (attackHitbox != null || collision.collider.IsTouching(attackHitbox)))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHpSystem playerHp = collision.collider.GetComponent<PlayerHpSystem>();
                playerHp.TakeHit(damage);
                rb.linearVelocity = Vector3.zero;
                player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
                currentState = EnemyState.Idle;
                ExecuteIdleState();
            }
            if (collision.gameObject.GetComponent<Core>() != null)
            {
                var hp = collision.gameObject.GetComponent<CoreHpSystem>();
                if (hp != null) hp.TakeHit(damage);
                DeactivateAttackHitbox();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == EnemyState.Attack && (attackHitbox != null || collision.IsTouching(attackHitbox)))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHpSystem playerHp = collision.GetComponent<PlayerHpSystem>();
                playerHp.TakeHit(damage);
                rb.linearVelocity = Vector3.zero;
                player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
                currentState = EnemyState.Idle;
                ExecuteIdleState();
            }
            if (collision.gameObject.GetComponent<Core>() != null)
            {
                var hp = collision.gameObject.GetComponent<CoreHpSystem>();
                if (hp != null) hp.TakeHit(damage);
                DeactivateAttackHitbox();
            }
        }
    }
}