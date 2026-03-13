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
    public int pathfindingClearance = 1;

    [Header("Combat Settings")]
    public float attackCooldownDuration = 1.5f;

    private Rigidbody2D rb;
    private EnemyFlip flip;
    private Enemy enemy;
    private Collider2D col;

    [SerializeField] bool showGizmos;

    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex = 0;

    private float pathTimer = 0f;
    private float attackCooldown = 1.5f;

    private Transform currentTarget;
    private Vector3 lastTargetPosition;
    private Vector2Int lastEnemyGridPos;
    private bool hasCalculatedFirstPath = false;

    // --- FIX: cache the Core's collider so we can find the closest surface point ---
    private Collider2D coreCollider;

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
        col = GetComponent<Collider2D>();

        // --- FIX: grab the collider from the Core GameObject ---
        var coreObj = FindFirstObjectByType<Core>();
        if (coreObj != null)
            coreCollider = coreObj.GetComponent<Collider2D>();

        pathTimer = Random.Range(0f, pathUpdateInterval * 0.5f);

        if (enemy != null)
        {
            currentTarget = enemy.GetCurrentTarget();
            if (currentTarget != null)
                lastTargetPosition = currentTarget.position;
        }

        lastEnemyGridPos = grid != null ? grid.WorldToGrid(transform.position) : Vector2Int.zero;
    }

    private Vector2 GetEffectiveTargetPosition()
    {
        if (currentTarget == null)
            return transform.position;

        if (enemy != null && currentTarget == enemy.core && coreCollider != null)
            return coreCollider.ClosestPoint(transform.position);
            
        return currentTarget.position;
    }
    private Vector2 GetBodyCenter()
    {
        return col != null ? col.bounds.center : transform.position;
    }
    void Update()
    {
        if (enemy != null)
            currentTarget = enemy.GetCurrentTarget();

        if (playerHp == null || playerHp.isDead) return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(AttackStateName))
            return;

        float distanceToTarget = Vector2.Distance(GetBodyCenter(), GetEffectiveTargetPosition());

        attackCooldown -= Time.deltaTime;
        pathTimer -= Time.deltaTime;

        if (flip != null)
            flip.LookAtPlayer();

        if (ShouldRecalculatePath())
            RecalculatePath();

        if (ShouldMove())
        {
            animator.ResetTrigger(AnimAttack);
            animator.SetBool(AnimIsMoving, true);
            if (animator.HasState(0, AnimIsIdle))
                animator.SetBool(AnimIsIdle, false);

            MoveToNextNode();
        }
        else
        {
            animator.SetBool(AnimIsMoving, false);
            if (animator.HasState(0, AnimIsIdle))
                animator.SetBool(AnimIsIdle, true);

            if (distanceToTarget <= attackRange && attackCooldown <= 0f && !playerHp.isDead && !coreHp.isDead)
                AttackTarget();
        }
    }

    bool ShouldRecalculatePath()
    {
        if (!hasCalculatedFirstPath)
            return true;

        if (pathTimer > 0f || currentTarget == null || grid == null)
            return false;

        float distanceToTarget = Vector2.Distance(transform.position, GetEffectiveTargetPosition());
        if (distanceToTarget <= attackRange && currentPath != null && currentPath.Count > 0)
            return false;

        if (lastTargetPosition != Vector3.zero)
        {
            float targetMovedDistance = Vector3.Distance(currentTarget.position, lastTargetPosition);
            if (targetMovedDistance < targetMovementThreshold && currentPath != null && currentPath.Count > 0)
                return false;
        }

        Vector2Int currentGridPos = grid.WorldToGrid(GetBodyCenter());
        if (currentGridPos == lastEnemyGridPos && currentPath != null && currentPath.Count > 0)
            return false;

        return true;
    }

    void RecalculatePath()
    {
        Vector2Int enemyPos = grid.WorldToGrid(GetBodyCenter());
        Vector2Int targetPos = grid.WorldToGrid(GetEffectiveTargetPosition());

        if (enemyPos == targetPos)
        {
            currentPath.Clear();
            pathIndex = 0;
            pathTimer = pathUpdateInterval;
            hasCalculatedFirstPath = true;
            return;
        }

        currentPath = AStarPathFinder.FindPath(enemyPos, targetPos, grid, maxPathfindingIterations, pathfindingClearance);

        // FIX: if enemy is in a tight spot and clearance blocks all exits, retry without padding
        if (currentPath.Count == 0 && pathfindingClearance > 0)
            currentPath = AStarPathFinder.FindPath(enemyPos, targetPos, grid, maxPathfindingIterations, 0);

        pathIndex = 0;
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
            ? Vector2.Distance(GetBodyCenter(), GetEffectiveTargetPosition())
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
        if (animator.HasState(0, AnimIsIdle))
            animator.SetBool(AnimIsIdle, false);

        attackCooldown = attackCooldownDuration;

        if (enemy != null)
            enemy.ActivateAttackHitbox();
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            float cellSize = 1f;
            if (grid != null && grid.groundTilemap != null)
                cellSize = grid.groundTilemap.cellSize.x;

            if (pathfindingClearance > 0)
            {
                Collider2D col = GetComponent<Collider2D>();
                Vector3 center = col != null ? col.bounds.center : transform.position;
                float clearanceWorldSize = (pathfindingClearance * 2 + 1) * cellSize;
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
                Gizmos.DrawCube(center, new Vector3(clearanceWorldSize, clearanceWorldSize, 0f));
                Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
                Gizmos.DrawWireCube(center, new Vector3(clearanceWorldSize, clearanceWorldSize, 0f));
            }

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
}