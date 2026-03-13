using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    Enemy enemy;
    Animator anim;
    EnemyHitFlash hitFlash;

    public int maxHealth = 100;
    public int currentHealth;

    bool isAddedToGameStats = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        anim = GetComponentInChildren<Animator>();
        hitFlash = GetComponent<EnemyHitFlash>();
    }

    public void TakeDamage(int damage)
    {
        (AudioManager.Instance)?.PlaySFX("EnemyHit");
        hitFlash?.Flash();
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("chcip");
        (AudioManager.Instance)?.PlaySFX("EnemyDeath");
        GetComponent<Collider2D>().enabled = false;
        if (!isAddedToGameStats)
        {
            isAddedToGameStats = true;
        }

        enemy.currentState = EnemyState.Death;
        anim.SetBool("Die", true);
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsIdle", false);
        anim.ResetTrigger("Attack");
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
}