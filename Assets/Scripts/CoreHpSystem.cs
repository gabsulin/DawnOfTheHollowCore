using UnityEngine;
using UnityEngine.UI;

public class CoreHpSystem : MonoBehaviour
{
    [SerializeField] Image hpBar;

    public int maxHp = 100;
    public int currentHp;

    void Start()
    {
        currentHp = maxHp;
        hpBar.fillAmount = 1;
    }
    public void TakeHit(int damage)
    {
        currentHp -= damage;
        hpBar.fillAmount = (float)currentHp / maxHp;
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
