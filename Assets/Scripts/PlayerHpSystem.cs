using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpSystem : MonoBehaviour
{
    PlayerController playerController;

    [SerializeField] Image hpBar;
    [SerializeField] Image shieldsBar;
    [SerializeField] TMP_Text hpTMP;
    [SerializeField] TMP_Text shieldsTMP;
    //[SerializeField] TMP_Text damageNumber;
    GameObject deathScreen;
    Camera cam;

    /*[HideInInspector]*/ public float currentHp;
    /*[HideInInspector]*/ public float currentShields;
    public float maxHp;
    public float maxShields;

    private float wasntHit = 0f;
    private bool isRegeneratingShields = false;

    private float startShieldRegenTime = 5f;
    private float shieldRegenTime = 2f;

    public bool isDead;
    public bool isImmune;

    private void Awake()
    {
        currentHp = maxHp;
        currentShields = maxShields;

        isDead = false;
    }
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        UpdateUI();
    }
    public void UpdateUI()
    {
        if (hpBar != null && shieldsBar != null && hpTMP != null && shieldsTMP != null)
        {
            hpBar.fillAmount = currentHp / maxHp;
            hpTMP.text = $"{currentHp}/{maxHp}";
            shieldsBar.fillAmount = currentShields / maxShields;
            shieldsTMP.text = $"{currentShields}/{maxShields}";
            Debug.Log("UI elements updated");
        }
        else
        {
            Debug.LogError("UI Elements are not assigned in PlayerHpSystem!");
        }
    }

    private void Update()
    {
        wasntHit += Time.deltaTime;

        if (currentShields < maxShields && wasntHit >= startShieldRegenTime && !isRegeneratingShields && !isDead)
        {
            StartCoroutine(RegenerateShields());
        }
    }

    public void TakeHit(int damage)
    {
        if (isImmune || isDead) return;

        (AudioManager.Instance)?.PlaySFX("Hit");
        wasntHit = 0;

        if (currentShields > 0)
        {
            float shieldDamage = Mathf.Min(currentShields, damage);
            currentShields -= shieldDamage;
            damage -= (int)shieldDamage;
        }

        if (damage > 0)
        {
            currentHp -= damage;
        }

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }

        UpdateUI();
    }


    private void Die()
    {
        (AudioManager.Instance)?.PlaySFX("PlayerDeath");
        
        isDead = true;
        playerController.canMove = false;
        playerController.canAttack = false;


        //cam.gameObject.SetActive(true);
        //deathScreen.SetActive(true);
    }

    private IEnumerator PlayDeathAnimation()
    {
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator RegenerateShields()
    {
        isRegeneratingShields = true;

        while (currentShields < maxShields && !isDead && wasntHit >= startShieldRegenTime)
        {
            currentShields += 1;
            UpdateUI();

            yield return new WaitForSeconds(shieldRegenTime);
        }

        currentShields = Mathf.Min(currentShields, maxShields);
        isRegeneratingShields = false;
    }
    /*public void ApplyShieldRechargeUpgrade(float multiplier)
    {
        shieldRegenTime *= 1f / multiplier;
        startShieldRegenTime *= 1f / multiplier * 2.5f;
    }*/

    public void ApplyMaxHealthUpgrade(float amount)
    {
        maxHp += amount;
        currentHp += amount;
        UpdateUI();
    }
    public void ApplyMaxShieldUpgrade(float amount)
    {
        maxShields += amount;
        currentShields += amount;
        UpdateUI();
    }

}