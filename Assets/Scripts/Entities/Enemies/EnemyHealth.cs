using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    //private MapFunctionality manager;

    Enemy enemy;
    Animator anim;
    Knockback knockback;

    public int maxHealth = 100;
    public int currentHealth;

    int coinsDrop;
    int coinsAmount;

    bool hasDroppedCoins = false;
    bool isAddedToGameStats = false;

    [SerializeField] GameObject coinPrefab;
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    private void Start()
    {
        enemy = GetComponent<Enemy>();
        anim = GetComponentInChildren<Animator>();
        knockback = GetComponent<Knockback>();

        coinsDrop = Random.Range(0, 10);
        Debug.Log(coinsDrop);
    }
    public void TakeDamage(int damage)
    {
        (AudioManager.Instance)?.PlaySFX("EnemyHit");
        currentHealth -= damage;
        /*Weapon weapon = PlayerController.Instance?.GetCurrentWeapon();
        bool isMelee = weapon is Melee;
        if (isMelee)
            knockback.GetKnockback(PlayerController.Instance.transform, 5f);
        */
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        (AudioManager.Instance)?.PlaySFX("EnemyDeath");
        if (!isAddedToGameStats)
        {
            isAddedToGameStats = true;
            //GameStats.Instance.AddEnemyKill();
        }

        enemy.currentState = EnemyState.Death;
        anim.SetBool("Die", true);
        anim.SetBool("IsMoving", false);
        anim.ResetTrigger("Attack");

        var vampire = GetComponent<VampireEnemy>();
        /*if (vampire != null && vampire.hasRevived)
        {
            RemoveFromList();
        }
        else
        {
            RemoveFromList();
        }*/

        if (!hasDroppedCoins && coinsDrop == 9)
        {
            hasDroppedCoins = true;
            StartCoroutine(DropCoins());
        }
    }

    public void DestroyAfterDeath()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
    }

    IEnumerator DropCoins()
    {
        coinsAmount = Random.Range(3, 8);
        Debug.Log($"Dropping {coinsAmount} coins!");
        for (int i = 0; i < coinsAmount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = coin.AddComponent<Rigidbody2D>();

            rb.gravityScale = 1.0f;

            Vector2 randomDirection = Random.insideUnitCircle + Vector2.up;
            float randomForce = Random.Range(1, 3);
            rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.25f);

            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            StartCoroutine(AnimateCoinMovement(coin));
        }
    }

    IEnumerator AnimateCoinMovement(GameObject coin)
    {
        float duration = 1f;

        while (coin != null)
        {
            float elapsedTime = 0f;
            Vector2 startPos = coin.transform.position;
            Vector2 midPos = startPos + new Vector2(0, -0.25f);
            Vector2 endPos = startPos;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;

                if (coin == null)
                    yield break;

                if (t < 0.5f)
                    coin.transform.position = Vector3.Lerp(startPos, midPos, t * 2);
                else
                    coin.transform.position = Vector3.Lerp(midPos, endPos, (t - 0.5f) * 2);

                elapsedTime += Time.deltaTime;
                yield return null;

            }

            if (coin != null)
                coin.transform.position = endPos;
        }
    }

    /*private void RemoveFromList()
    {
        MapFunctionality manager = FindFirstObjectByType<MapFunctionality>();

        if (manager != null && manager.enemies.Contains(this))
        {
            manager.enemies.Remove(this);
        }
    }

    public void SetManager(MapFunctionality manager)
    {
        this.manager = manager;
    }*/
}
