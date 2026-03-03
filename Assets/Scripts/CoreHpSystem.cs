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
        if (GameManager.Instance != null)
            GameManager.Instance.EndGame(reason);
        else
            Debug.LogError("[CoreHpSystem] GameManager instance not found!");
    }
}
