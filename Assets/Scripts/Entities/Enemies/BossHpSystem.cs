using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHpSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SeekerBoss boss;
    [SerializeField] private Image healthBar;

    [Header("Settings")]
    [SerializeField] private int maxHealth = 150;
    private int currentHealth;
    public bool isDead = false;

    void Start()
    {
        if (boss == null)
            boss = FindFirstObjectByType<SeekerBoss>();

        currentHealth = maxHealth;
        healthBar.fillAmount = (float)currentHealth / maxHealth;

    }

    void Update()
    {
    }

    public void TakeHit(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (currentHealth < 0)
            currentHealth = 0;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (boss != null)
            boss.OnDeath();
    }
}