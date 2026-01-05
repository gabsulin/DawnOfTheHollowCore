using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinder : MonoBehaviour
{
    public Transform player;
    private Transform currentTarget;
    private Core coreObject;
    public PlayerHpSystem playerHp;
    public Animator animator;
    public GridManager grid;
    Rigidbody2D rb;
    EnemyFlip flip;

    public float moveSpeed = 2f;
    public float maxDistance = 2f;

    private float distance;

    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex = 0;

    private float pathTimer = 0f;
    private float pathInterval = 0.25f;
    private float attackCooldown = 1.5f;

    Enemy enemy;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>().transform;

        var coreFound = FindFirstObjectByType<Core>();
        if (coreFound != null)
        {
            currentTarget = coreFound.transform;
            coreObject = coreFound;
        }

        // fallback
        if (currentTarget == null)
            currentTarget = player;

        playerHp = FindFirstObjectByType<PlayerHpSystem>();
        animator = GetComponent<Animator>();
        grid = FindFirstObjectByType<GridManager>();
        rb = GetComponent<Rigidbody2D>();
        flip = GetComponent<EnemyFlip>();
        enemy = GetComponent<Enemy>();
    }


    void Update()
    {
        currentTarget = enemy.GetCurrentTarget();
        if (playerHp.isDead) return;

        distance = Vector2.Distance(transform.position, currentTarget.position);
        attackCooldown -= Time.deltaTime;
        pathTimer -= Time.deltaTime;

        flip.LookAtPlayer();

        // --- PATHFIND ---
        if (pathTimer <= 0f)
        {
            Vector2Int enemyPos = grid.WorldToGrid(transform.position);
            Vector2Int playerPos = grid.WorldToGrid(currentTarget.position);

            currentPath = AStarPathFinder.FindPath(enemyPos, playerPos, grid, 1000);

            pathIndex = 0;
            for (int i = 0; i < currentPath.Count; i++)
            {
                if (currentPath[i] == enemyPos)
                {
                    pathIndex = i;
                    break;
                }
            }

            pathTimer = pathInterval;
        }

        // --- MOVEMENT & ATTACK ---
        if (ShouldMove())
        {
            animator.ResetTrigger("Attack");
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsIdle", false);

            MoveToNextNode();
        }
        else
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsIdle", true);

            if (attackCooldown <= 0f)
            {
                AttackTarget();
            }
        }
    }

    bool ShouldMove()
    {
        return currentPath.Count > 1 &&
               pathIndex < currentPath.Count - 1 &&
               distance > maxDistance &&
               !animator.GetBool("Die");
    }

    void MoveToNextNode()
    {
        Vector2Int nextStep = currentPath[pathIndex + 1];
        Vector2 targetWorld = grid.GridToWorld(nextStep);

        rb.MovePosition(Vector2.MoveTowards(rb.position, targetWorld, moveSpeed * Time.deltaTime));

        if (Vector2.Distance(transform.position, targetWorld) < 0.05f)
            pathIndex++;
    }

    void AttackTarget()
    {
        animator.SetTrigger("Attack");
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsIdle", false);

        attackCooldown = 1.5f;

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
