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

    bool isAddedToGameStats = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
    private void Start()
    {
        enemy = GetComponent<Enemy>();
        anim = GetComponentInChildren<Animator>();
        knockback = GetComponent<Knockback>();
    }
    public void TakeDamage(int damage)
    {
        (AudioManager.Instance)?.PlaySFX("EnemyHit");
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
        if (!isAddedToGameStats)
        {
            isAddedToGameStats = true;
            //GameStats.Instance.AddEnemyKill();
        }

        enemy.currentState = EnemyState.Death;
        anim.SetBool("Die", true);
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsIdle", false);
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
