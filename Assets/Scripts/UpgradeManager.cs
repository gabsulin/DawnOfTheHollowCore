using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private PlayerController player;
    private PlayerHpSystem hp;
    private Laser laser;
    private AbilityHolder abilityHolder;

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
            Debug.Log($"Movement speed + {item.moveSpeedBonus}");
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
            Debug.Log($"Laser damage + {item.damageBonus}");
        }

        if (item.dashCooldownReduction != 0 && abilityHolder != null && abilityHolder.ability != null)
        {
            float reduction = abilityHolder.ability.baseCooldownTime * item.dashCooldownReduction;
            abilityHolder.ability.coolDownTime = Mathf.Max(abilityHolder.ability.coolDownTime - reduction, 0.1f);
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
    }
}
