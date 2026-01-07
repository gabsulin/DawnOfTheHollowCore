using UnityEngine;

public class CoreHpSystem : MonoBehaviour
{
    public int maxHp = 100;
    public int currentHp;

    void Start()
    {
        currentHp = maxHp;
    }
    public void TakeHit(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        Destroy(gameObject);
    }
}
