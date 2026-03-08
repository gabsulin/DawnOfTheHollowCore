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
    [SerializeField] private int currentHealth;
    public bool isDead = false;

    [Header("Boss Loot")]
    [SerializeField] private GameObject worldItemPrefab;
    [SerializeField] private ItemSO bossDropItem;
    [SerializeField] private int bossDropAmount = 1;
    [SerializeField] private RecipeSO keyRecipeToUnlock;

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

        DropLoot();
        UnlockKeyRecipe();
    }

    private void DropLoot()
    {
        if (worldItemPrefab == null || bossDropItem == null || boss == null) return;

        GameObject go = Instantiate(worldItemPrefab, boss.transform.position, Quaternion.identity);

        var wi = go.GetComponent<WorldItem>();
        if (wi != null)
        {
            wi.Initialize(bossDropItem, bossDropAmount);
            wi.ApplyPickupDelay();
        }
    }

    private void UnlockKeyRecipe()
    {
        if (keyRecipeToUnlock == null) return;

        if (RecipeManager.Instance != null)
            RecipeManager.Instance.UnlockRecipe(keyRecipeToUnlock);
    }
}