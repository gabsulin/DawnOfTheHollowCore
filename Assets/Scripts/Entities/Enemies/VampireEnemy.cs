using UnityEngine;

public class VampireEnemy : Enemy
{
    [Header("Vampire Settings")]
    [SerializeField] public Transform centerOfAttack;
    [SerializeField] float attackRadius;

    [SerializeField] GameObject batPrefab;
    public bool hasRevived = false;
    //CameraShake shake;

    public void HandleDeath()
    {
        if (!hasRevived)
        {
            VampireAbility();
        }
        else
        {
            EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.DestroyAfterDeath();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    protected override void Start()
    {
        attackCooldown = animator.GetFloat("AttackCooldown");
        player = FindFirstObjectByType<PlayerController>().transform;
        //shake = GetComponent<CameraShake>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        attackCooldown += Time.deltaTime;
        animator.SetFloat("AttackCooldown", attackCooldown);
    }

    public override void Attack()
    {
        currentState = EnemyState.Attack;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerOfAttack.position, attackRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                var playerHp = collider.GetComponent<PlayerHpSystem>();
                if (playerHp != null) playerHp.TakeHit(1);
                //if (shake != null) shake.StartShake(force: 0.2f);
            }
        }
    }

    private void VampireAbility()
    {
        GameObject batInstance = Instantiate(batPrefab, transform.position, Quaternion.identity);
        Bat batScript = batInstance.GetComponent<Bat>();

        if (batScript != null)
        {
            batScript.SetOriginalVampire(this);
        }
        else
        {
            Debug.LogError("Bat prefab nemá pøipojený Bat skript!");
        }

        hasRevived = true;
        gameObject.SetActive(false);
    }

    public void Reactivate(Vector2 revivalPosition)
    {
        transform.position = revivalPosition;
        gameObject.SetActive(true);

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.RestoreFullHealth();
        }

        Debug.Log(gameObject.name + " byl reaktivován na pozici " + revivalPosition);
    }

    public override void OnCollisionEnter2D(Collision2D collision) { }
    protected override void OnDrawGizmos()
    {
        if (centerOfAttack != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerOfAttack.position, attackRadius);
        }
    }
}