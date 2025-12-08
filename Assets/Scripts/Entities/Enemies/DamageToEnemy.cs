using UnityEngine;

public class DamageToEnemy : MonoBehaviour
{
    [SerializeField] int damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            TryDealDamageToEnemy(collision);
        }
        else if (collision.CompareTag("MiniBoss"))
        {
            /*var bossHp = collision.GetComponent<BossHpSystem>();
            if (bossHp != null)
                bossHp.TakeDamage(damage, false);*/
        }
    }

    private bool TryDealDamageToEnemy(Component target)
    {
        var enemyHp = target.GetComponent<EnemyHealth>();
        if (enemyHp != null)
        {
            enemyHp.TakeDamage(damage);
            return true;
        }
        return false;
    }
}
