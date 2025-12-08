using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    /*public enum EnemyType { Melee, Magic, Bow }

    [SerializeField] private EnemyType enemyType;

    private Transform player;
    private EnemyObj enemy;
    private EnemyHpSystem enemyHp;
    private PlayerHpSystem playerHp;
    private SPUM_Prefabs anim;
    private RangedEnemyAttack rangedEnemyAttack;
    private MeleeEnemyAttack meleeEnemyAttack;
    private BossFlip flip;
    private Rigidbody2D rb;
    public float speed;

    [HideInInspector] public float distance;

    [SerializeField] public float attackThreshold;
    [SerializeField] public float moveThreshold;

    private GridManager grid;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex = 0;
    private float pathRecalculationTimer = 0f;
    private float pathRecalculationInterval = 0.25f;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>().transform;
        enemy = GetComponent<EnemyObj>();
        anim = GetComponentInChildren<SPUM_Prefabs>();
        enemyHp = GetComponent<EnemyHpSystem>();
        playerHp = FindAnyObjectByType<PlayerHpSystem>();
        rangedEnemyAttack = GetComponent<RangedEnemyAttack>();
        meleeEnemyAttack = GetComponent<MeleeEnemyAttack>();
        flip = GetComponent<BossFlip>();
        grid = FindAnyObjectByType<GridManager>();
        rb = GetComponent<Rigidbody2D>(); // Retained for other potential physics interactions, but not movement
    }

    private void Update()
    {
        if (player == null || enemy == null || playerHp == null || grid == null || enemyHp.currentHealth <= 0 || anim == null || flip == null)
            return;

        if (anim._anim.GetBool("Die"))
            return;

        distance = Vector2.Distance(transform.position, player.transform.position);
        flip.LookAtPlayer();

        pathRecalculationTimer -= Time.deltaTime;

        if (pathRecalculationTimer <= 0f)
        {
            Vector2Int enemyPosGrid = grid.WorldToGrid(transform.position);
            Vector2Int playerPosGrid = grid.WorldToGrid(player.position);

            currentPath = AStarPathFinder.FindPath(enemyPosGrid, playerPosGrid, grid);
            pathIndex = 0;

            if (currentPath != null && currentPath.Count > 0)
            {
                for (int i = 0; i < currentPath.Count; i++)
                {
                    if (currentPath[i] == enemyPosGrid)
                    {
                        pathIndex = i;
                        break;
                    }
                }
            }
            pathRecalculationTimer = pathRecalculationInterval;
        }

        HandleBehavior();
    }

    private void HandleBehavior()
    {
        if (playerHp.currentHp <= 0)
        {
            Idle();
            return;
        }
        if (distance <= attackThreshold)
        {
            AttackPlayer();
        }
        else if (distance <= moveThreshold && distance > attackThreshold)
        {
            MoveAlongPath();
        }
        else
        {
            Idle();
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath != null && currentPath.Count > 1 && pathIndex < currentPath.Count - 1)
        {
            Vector2Int nextStepGrid = currentPath[pathIndex + 1];
            Vector2 targetWorld = grid.GridToWorld(nextStepGrid);

            if (IsValidPosition(targetWorld))
            {
                transform.position = Vector2.MoveTowards(transform.position, targetWorld, speed * Time.deltaTime);

                if (enemy._enemyState != EnemyObj.EnemyState.move)
                {
                    enemy._enemyState = EnemyObj.EnemyState.move;
                    anim.PlayAnimation(1);
                }

                if (Vector2.Distance(transform.position, targetWorld) < 0.05f)
                {
                    pathIndex++;
                }
            }
            else
            {
                pathRecalculationTimer = 0f;
            }
        }
        else if (distance > moveThreshold)
        {
            Idle();
        }
    }

    private void AttackPlayer()
    {
        enemy._enemyState = EnemyObj.EnemyState.attack;

        switch (enemyType)
        {
            case EnemyType.Melee:
                anim.PlayAnimation(4);
                break;
            case EnemyType.Bow:
                anim.PlayAnimation(5);
                break;
            case EnemyType.Magic:
                anim.PlayAnimation(6);
                break;
        }
    }

    private void Idle()
    {
        if (enemy._enemyState == EnemyObj.EnemyState.idle)
        {
            return;
        }
        enemy._enemyState = EnemyObj.EnemyState.idle;

        anim.PlayAnimation(0);
    }

    private bool IsValidPosition(Vector2 pos)
    {
        return !Physics2D.OverlapPoint(pos, LayerMask.GetMask("Collision"));
    }*/
}
