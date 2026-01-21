using UnityEngine;
using UnityEngine.UI;

public class CoreHpSystem : MonoBehaviour
{
    [SerializeField] Image hpBar;
    [SerializeField] Animator animator;
    public int maxHp = 100;
    public int currentHp;
    public bool isDead = false;

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
        animator.SetBool("isDead", true);
        isDead = true;
        EndGame(1);
    }

    private void EndGame(int reason)
    {
        //1 - Core destroyed 2 - Player merged with core
        //GameManager.Instance.EndGame(reason);
    }
}
