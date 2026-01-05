using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private PlayerController player;
    private PlayerHpSystem hp;
    private Laser laser;

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
    }

    public void ApplyUpgrade(ItemSO item)
    {
        if (item.type != ItemType.Upgrade)
        {
            Debug.LogWarning($"Tried to use item '{item.name}' but it's not an upgrade.");
            return;
        }

        Debug.Log($"Applying upgrade: {item.itemName}");

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
            hp.maxHp += item.maxHealthBonus;
            hp.currentHp += item.maxHealthBonus;
            Debug.Log($"Max HP + {item.maxHealthBonus}");
        }

        if (item.maxShieldBonus != 0)
        {
            hp.maxShields += item.maxShieldBonus;
            hp.currentShields += item.maxShieldBonus;
            Debug.Log($"Max Shields + {item.maxShieldBonus}");
        }

        if (item.damageBonus != 0)
        {
            laser.laserDamage += item.damageBonus;
            Debug.Log($"Laser damage + {item.damageBonus}");
        }

        Debug.Log("Upgrade applied successfully!");
    }
}
