using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinder : MonoBehaviour
{
    [Header("References")]
    public PlayerHpSystem playerHp;
    public CoreHpSystem coreHp;
    public Animator animator;
    public GridManager grid;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 2f;

    [Header("Pathfinding Settings")]
    public float pathUpdateInterval = 0.5f;
    public float targetMovementThreshold = 1.5f;
    public int maxPathfindingIterations = 500;

    [Header("Combat Settings")]
    public float attackCooldownDuration = 1.5f;

    private Rigidbody2D rb;
    private EnemyFlip flip;
    private Enemy enemy;

    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex = 0;

    private float pathTimer = 0f;
    private float attackCooldown = 1.5f;

    private Transform currentTarget;
    private Vector3 lastTargetPosition;
    private Vector2Int lastEnemyGridPos;
    private bool hasCalculatedFirstPath = false;

    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimIsIdle = Animator.StringToHash("IsIdle");
    private static readonly int AnimDie = Animator.StringToHash("Die");
    private const string AttackStateName = "Attack";

    private void Start()
    {
        playerHp = FindFirstObjectByType<PlayerHpSystem>();
        coreHp = FindFirstObjectByType<CoreHpSystem>();
        animator = GetComponent<Animator>();
        grid = FindFirstObjectByType<GridManager>();
        rb = GetComponent<Rigidbody2D>();
        flip = GetComponent<EnemyFlip>();
        enemy = GetComponent<Enemy>();

        pathTimer = Random.Range(0f, pathUpdateInterval * 0.5f);

        if (enemy != null)
        {
            currentTarget = enemy.GetCurrentTarget();
            if (currentTarget != null)
            {
                lastTargetPosition = currentTarget.position;
            }
        }

        lastEnemyGridPos = grid != null ? grid.WorldToGrid(transform.position) : Vector2Int.zero;
    }

    void Update()
    {
        if (enemy != null)
        {
            currentTarget = enemy.GetCurrentTarget();
        }

        if (playerHp == null || playerHp.isDead) return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(AttackStateName))
            return;

        float distanceToTarget = currentTarget != null
            ? Vector2.Distance(transform.position, currentTarget.position)
            : float.MaxValue;

        attackCooldown -= Time.deltaTime;
        pathTimer -= Time.deltaTime;

        if (flip != null)
            flip.LookAtPlayer();

        if (ShouldRecalculatePath())
        {
            RecalculatePath();
        }

        if (ShouldMove())
        {
            animator.ResetTrigger(AnimAttack);
            animator.SetBool(AnimIsMoving, true);
            animator.SetBool(AnimIsIdle, false);

            MoveToNextNode();
        }
        else
        {
            animator.SetBool(AnimIsMoving, false);
            animator.SetBool(AnimIsIdle, true);

            if (distanceToTarget <= attackRange && attackCooldown <= 0f && !playerHp.isDead && !coreHp.isDead)
            {
                AttackTarget();
            }
        }
    }

    bool ShouldRecalculatePath()
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
        if (currentGridPos == lastEnemyGridPos && currentPath != null && currentPath.Count > 0)
            return false;

        return true;
    }

    void RecalculatePath()
    {
        Vector2Int enemyPos = grid.WorldToGrid(transform.position);
        Vector2Int targetPos = grid.WorldToGrid(currentTarget.position);

        if (enemyPos == targetPos)
        {
            currentPath.Clear();
            pathIndex = 0;
            pathTimer = pathUpdateInterval;
            hasCalculatedFirstPath = true;
            return;
        }

        currentPath = AStarPathFinder.FindPath(enemyPos, targetPos, grid, maxPathfindingIterations);

        pathIndex = 0;

        if (currentPath != null && currentPath.Count > 0)
        {
            pathIndex = 0;
        }

        lastTargetPosition = currentTarget.position;
        lastEnemyGridPos = enemyPos;
        pathTimer = pathUpdateInterval;
        hasCalculatedFirstPath = true;
    }

    bool ShouldMove()
    {
        if (currentPath == null || currentPath.Count <= 1 || pathIndex >= currentPath.Count - 1)
            return false;

        if (animator.GetBool(AnimDie))
            return false;

        float distanceToTarget = currentTarget != null
            ? Vector2.Distance(transform.position, currentTarget.position)
            : float.MaxValue;

        if (distanceToTarget <= attackRange)
            return false;

        return true;
    }

    void MoveToNextNode()
    {
        if (pathIndex + 1 >= currentPath.Count)
            return;

        Vector2Int nextStep = currentPath[pathIndex + 1];
        Vector2 targetWorld = grid.GridToWorld(nextStep);

        rb.MovePosition(Vector2.MoveTowards(rb.position, targetWorld, moveSpeed * Time.deltaTime));

        if (Vector2.Distance(transform.position, targetWorld) < 0.05f)
            pathIndex++;
    }

    void AttackTarget()
    {
        animator.SetTrigger(AnimAttack);
        animator.SetBool(AnimIsMoving, false);
        animator.SetBool(AnimIsIdle, false);

        attackCooldown = attackCooldownDuration;

        if (enemy != null)
            enemy.ActivateAttackHitbox();
    }

    void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0 || grid == null) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector2 worldPos = grid.GridToWorld(currentPath[i]);
            Gizmos.DrawSphere(worldPos, 0.1f);

            if (i < currentPath.Count - 1)
            {
                Vector2 nextWorld = grid.GridToWorld(currentPath[i + 1]);
                Gizmos.DrawLine(worldPos, nextWorld);
            }
        }
    }
}