using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private PlayerController player;
    private PlayerHpSystem hp;
    private Laser laser;
    private AbilityHolder abilityHolder;
    private CoreHpSystem coreHpSystem;

    // --- ADDED ---
    [Header("Stat Caps")]
    [SerializeField] private float maxMoveSpeed = 9f;
    [SerializeField] private float maxLaserDamage = 6f;
    [SerializeField] private float minDashCooldown = 1.5f;
    // --- END ADDED ---

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = FindFirstObjectByType<PlayerController>();
        hp = FindFirstObjectByType<PlayerHpSystem>();
        laser = FindFirstObjectByType<Laser>();
        abilityHolder = FindFirstObjectByType<AbilityHolder>();
        coreHpSystem = FindFirstObjectByType<CoreHpSystem>();
    }

    public void ApplyUpgrade(ItemSO item)
    {
        if (item.type != ItemType.Upgrade)
        {
            Debug.LogWarning($"Tried to use item '{item.name}' but it's not an upgrade.");
            return;
        }

        Debug.Log($"Applying upgrade: {item.itemName}");
        AudioManager.Instance?.PlaySFX("Upgrade");

        if (item.moveSpeedBonus != 0)
        {
            player.moveSpeed += item.moveSpeedBonus;
            player.moveSpeed = Mathf.Min(player.moveSpeed, maxMoveSpeed);
            Debug.Log($"Movement speed + {item.moveSpeedBonus} (capped at {maxMoveSpeed})");
        }

        if (item.miningSpeedMultiplier != 0)
        {
            laser.ApplyMiningSpeedUpgrade(item.miningSpeedMultiplier);
            Debug.Log($"Mining speed x {item.miningSpeedMultiplier}");
        }

        if (item.maxHealthBonus != 0)
        {
            hp.ApplyMaxHealthUpgrade(item.maxHealthBonus);
            Debug.Log($"Max HP + {item.maxHealthBonus}");
        }

        if (item.maxShieldBonus != 0)
        {
            hp.ApplyMaxShieldUpgrade(item.maxShieldBonus);
            Debug.Log($"Max Shields + {item.maxShieldBonus}");
        }

        if (item.damageBonus != 0)
        {
            laser.laserDamage += item.damageBonus;
            // --- ADDED ---
            laser.laserDamage = Mathf.Min(laser.laserDamage, maxLaserDamage);
            // --- END ADDED ---
            Debug.Log($"Laser damage + {item.damageBonus} (capped at {maxLaserDamage})");
        }

        if (item.dashCooldownReduction != 0 && abilityHolder != null && abilityHolder.ability != null)
        {
            float reduction = abilityHolder.ability.baseCooldownTime * item.dashCooldownReduction;
            // --- CHANGED: was Mathf.Max(..., 0.1f), now uses tunable minDashCooldown ---
            abilityHolder.ability.coolDownTime = Mathf.Max(abilityHolder.ability.coolDownTime - reduction, minDashCooldown);
            // --- END CHANGED ---
            Debug.Log($"Dash cooldown reduced by {item.dashCooldownReduction * 100f}% -> new cooldown: {abilityHolder.ability.coolDownTime:F2}s");
        }

        Debug.Log("Upgrade applied successfully!");
    }

    public void UseConsumable(ItemSO item)
    {
        if (item.type != ItemType.Consumable)
        {
            Debug.LogWarning($"Tried to consume item '{item.name}' but it's not a Consumable.");
            return;
        }

        AudioManager.Instance?.PlaySFX("Upgrade");

        if (item.healAmount != 0)
        {
            hp.Heal(item.healAmount);
            AudioManager.Instance?.PlaySFX("Heal");
            Debug.Log($"Healed for {item.healAmount}. Current HP: {hp.currentHp}/{hp.maxHp}");
        }

        if (item.coreHealAmount != 0)
        {
            coreHpSystem.HealCore(item.coreHealAmount);
            AudioManager.Instance?.PlaySFX("Heal");
            Debug.Log($"Healed Core for {item.coreHealAmount}. Current Core HP: {coreHpSystem.currentHp}/{coreHpSystem.maxHp}");
        }
    }
}