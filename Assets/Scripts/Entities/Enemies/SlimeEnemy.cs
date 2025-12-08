using System.Collections;
using UnityEngine;

public class SlimeEnemy : Enemy
{
    [Header("Slime Jump Settings")]
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;
    private bool isJumping = false;

    //CameraShake shake;
    protected override void Start()
    {
        attackCooldown = animator.GetFloat("AttackCooldown");
        player = FindFirstObjectByType<PlayerController>().transform;
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
        if(!isJumping)
            StartCoroutine(JumpAttackRoutine());
    }

    IEnumerator JumpAttackRoutine()
    {
        isJumping = true;
        Vector2 startPos = transform.position;
        Vector2 targetPos = player.position;

        float timer = 0f;
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;
            float height = 4 * jumpHeight * t * (1 - t); // parabolická výška
            transform.position = Vector3.Lerp(startPos, targetPos + new Vector2(0.1f, 0.1f), t) + Vector3.up * height;
            yield return null;
        }

        isJumping = false;
        attackCooldown = 0f;
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.Attack && collision.gameObject.CompareTag("Player") && (attackHitbox != null || collision.collider.IsTouching(attackHitbox)))
        {
            PlayerHpSystem playerHp = collision.collider.GetComponent<PlayerHpSystem>();
            playerHp.TakeHit(damage);
            rb.linearVelocity = Vector3.zero;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
            currentState = EnemyState.Idle;
            ExecuteIdleState();
            attackCooldown = 0f;
            //shake.StartShake(force: 0.1f);
        }
    }
}
