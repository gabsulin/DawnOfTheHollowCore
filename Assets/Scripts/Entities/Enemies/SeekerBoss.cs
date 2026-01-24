using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SeekerBoss : Enemy
{
    [Header("Seeker Boss Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateInterval = 0.3f;
    [SerializeField] private float targetMovementThreshold = 1f;
    [SerializeField] private int maxPathfindingIterations = 500;

    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float spawnDuration = 1.5f;
    [SerializeField] private float deathDuration = 2f;

    [Header("Attack Settings")]
    [SerializeField] private Transform centerOfAttack;
    [SerializeField] private float attackRadius = 5f;

    private SeekerState currentBossState;
    private int currentHealth;

    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex = 0;
    private float pathTimer = 0f;
    private Vector3 lastTargetPosition;
    private Vector2Int lastGridPos;
    private bool hasCalculatedFirstPath = false;

    private float attackTimer = 0f;
    private EnemyFlip flip;

    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimIsIdle = Animator.StringToHash("IsIdle");
    private static readonly int AnimDie = Animator.StringToHash("Die");
    private static readonly int AnimSpawn = Animator.StringToHash("Spawn");

    private const string AttackStateName = "Attack";
    private const string SpawnStateName = "Spawn";
    private const string DeathStateName = "Death";

    private enum SeekerState
    {
        Spawn,
        Idle,
        Walk,
        Attack,
        Death
    }

    protected override void Start()
    {
        base.Start();

        flip = GetComponent<EnemyFlip>();
        currentHealth = maxHealth;

        pathTimer = Random.Range(0f, pathUpdateInterval * 0.5f);
        lastGridPos = grid != null ? grid.WorldToGrid(transform.position) : Vector2Int.zero;

        StartCoroutine(SpawnSequence());
    }

    void Update()
    {
        if (currentBossState == SeekerState.Spawn || currentBossState == SeekerState.Death)
            return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(AttackStateName))
            return;

        Transform currentTarget = GetCurrentTarget();
        if (currentTarget == null)
            return;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        pathTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        if (flip != null)
            flip.LookAtPlayer();

        if (ShouldRecalculatePath(currentTarget))
        {
            RecalculatePath(currentTarget);
        }

        if (distanceToTarget <= attackRange && attackTimer <= 0f)
        {
            StartCoroutine(AttackSequence());
        }
        else
        {
            if (ShouldMove())
            {
                ChangeState(SeekerState.Walk);
                MoveAlongPath();
            }
            else
            {
                if (currentBossState != SeekerState.Idle)
                {
                    pathTimer = 0f;
                }
            }
        }
    }
    
    bool ShouldRecalculatePath(Transform currentTarget)
    {
        if (!hasCalculatedFirstPath)
            return true;

        if (pathTimer > 0f || currentTarget == null || grid == null)
            return false;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= attackRange && currentPath != null && currentPath.Count > 0)
            return false;

        if (lastTargetPosition != Vector3.zero)
        {
            float targetMovedDistance = Vector3.Distance(currentTarget.position, lastTargetPosition);

            if (targetMovedDistance < targetMovementThreshold && currentPath != null && currentPath.Count > 0)
                return false;
        }

        Vector2Int currentGridPos = grid.WorldToGrid(transform.position);
        if (currentGridPos == lastGridPos && currentPath != null && currentPath.Count > 0)
            return false;

        return true;
    }

    void RecalculatePath(Transform currentTarget)
    {
        Vector2Int startPos = grid.WorldToGrid(transform.position);
        Vector2Int targetPos = grid.WorldToGrid(currentTarget.position);

        if (startPos == targetPos)
        {
            currentPath.Clear();
            pathIndex = 0;
            pathTimer = pathUpdateInterval;
            hasCalculatedFirstPath = true;
            return;
        }

        currentPath = AStarPathFinder.FindPath(startPos, targetPos, grid, maxPathfindingIterations);
        pathIndex = 0;

        lastTargetPosition = currentTarget.position;
        lastGridPos = startPos;
        pathTimer = pathUpdateInterval;
        hasCalculatedFirstPath = true;
    }

    bool ShouldMove()
    {
        if (currentPath == null || currentPath.Count <= 1 || pathIndex >= currentPath.Count - 1)
            return false;

        Transform currentTarget = GetCurrentTarget();
        float distanceToTarget = currentTarget != null
            ? Vector2.Distance(transform.position, currentTarget.position)
            : float.MaxValue;

        if (distanceToTarget <= attackRange)
            return false;

        return true;
    }

    void MoveAlongPath()
    {
        if (pathIndex + 1 >= currentPath.Count)
            return;

        Vector2Int nextStep = currentPath[pathIndex + 1];
        Vector2 targetWorld = grid.GridToWorld(nextStep);

        rb.MovePosition(Vector2.MoveTowards(rb.position, targetWorld, chaseSpeed * Time.deltaTime));

        if (Vector2.Distance(transform.position, targetWorld) < 0.05f)
            pathIndex++;
    }

    IEnumerator SpawnSequence()
    {
        ChangeState(SeekerState.Spawn);

        if (animator != null)
            animator.SetTrigger(AnimSpawn);

        yield return new WaitForSeconds(spawnDuration);

        ChangeState(SeekerState.Walk);
    }

    IEnumerator AttackSequence()
    {
        ChangeState(SeekerState.Attack);

        if (animator != null)
        {
            animator.SetTrigger(AnimAttack);
            animator.SetBool(AnimIsMoving, false);
            animator.SetBool(AnimIsIdle, false);
        }

        attackTimer = attackCooldown;

        yield return new WaitForSeconds(attackCooldown);

        ChangeState(SeekerState.Idle);
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentBossState == SeekerState.Death || currentBossState == SeekerState.Spawn)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        ChangeState(SeekerState.Death);

        if (animator != null)
        {
            animator.SetBool(AnimDie, true);
            animator.SetBool(AnimIsMoving, false);
            animator.SetBool(AnimIsIdle, false);
        }

        // Disable collision
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, deathDuration);
    }

    void ChangeState(SeekerState newState)
    {
        if (currentBossState == newState)
            return;

        currentBossState = newState;

        if (animator != null)
        {
            switch (newState)
            {
                case SeekerState.Idle:
                    animator.SetBool(AnimIsIdle, true);
                    animator.SetBool(AnimIsMoving, false);
                    break;

                case SeekerState.Walk:
                    animator.SetBool(AnimIsMoving, true);
                    animator.SetBool(AnimIsIdle, false);
                    break;

                case SeekerState.Attack:
                    animator.SetBool(AnimIsMoving, false);
                    animator.SetBool(AnimIsIdle, false);
                    break;
            }
        }

        Debug.Log($"Seeker Boss: {newState}");
    }

    public override void Attack()
    {
        currentBossState = SeekerState.Attack;
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
            }
        }
    }

    public override void OnCollisionEnter2D(Collision2D collision) { }

    void OnDrawGizmos()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Draw current path
        if (currentPath != null && currentPath.Count > 0 && grid != null)
        {
            Gizmos.color = Color.green;
            for (int i = pathIndex; i < currentPath.Count - 1; i++)
            {
                Vector2 start = grid.GridToWorld(currentPath[i]);
                Vector2 end = grid.GridToWorld(currentPath[i + 1]);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(start, 0.1f);
            }
        }
    }
}