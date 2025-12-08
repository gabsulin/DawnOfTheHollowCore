using System.Collections;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpSystem : MonoBehaviour
{
    PlayerController playerController;

    /*[SerializeField] Image hpBar;
    [SerializeField] Image shieldsBar;
    [SerializeField] TMP_Text hpTMP;
    [SerializeField] TMP_Text shieldsTMP;
    [SerializeField] TMP_Text damageNumber;
    GameObject deathScreen;
    Camera cam;*/

    [HideInInspector] public float currentHp;
    [HideInInspector] public float currentShields;
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
    }

    /*public void AssignUIElements()
    {
        cam = GameObject.Find("DeathCamera").GetComponent<Camera>();
        cam.gameObject.SetActive(false);
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas != null)
        {
            hpBar = canvas.transform.Find("HealthBar/Health").GetComponent<Image>();
            hpTMP = canvas.transform.Find("HealthBar/HpAmount").GetComponent<TMP_Text>();
            shieldsBar = canvas.transform.Find("Shieldbar/Shields").GetComponent<Image>();
            shieldsTMP = canvas.transform.Find("Shieldbar/ShieldsAmount").GetComponent<TMP_Text>();
            deathScreen = canvas.transform.Find("DeathScreen").gameObject;
            deathScreen.SetActive(false);
            Debug.Log("nasel se canvas a priradily se gameobjecty");
        }
        else
        {
            Debug.LogError("Canvas not found! Make sure your Canvas is named correctly.");
        }
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
    }*/

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
        (AudioManager.Instance)?.PlaySFX("Hit");
        wasntHit = 0;
        if (!isImmune)
        {
            if (currentShields > 0)
            {
                int overflowDmg = damage - (int)currentShields;

                currentShields -= damage;

                if (currentShields <= 0) currentShields = 0;

                //shieldsBar.fillAmount = currentShields / maxShields;
                //shieldsTMP.text = $"{currentShields.ToString()}/{maxShields.ToString()}";

                if (overflowDmg > 0)
                {
                    currentHp -= overflowDmg;

                    //hpBar.fillAmount = currentHp / maxHp;
                    //hpTMP.text = $"{currentHp.ToString()}/{maxHp.ToString()}";

                    if (currentHp <= 0)
                    {
                        currentHp = 0;
                        //hpTMP.text = $"{currentHp.ToString()}/{maxHp.ToString()}";
                        Die();
                    }
                }
            }
            else
            {
                currentHp -= damage;
                //hpBar.fillAmount = currentHp / maxHp;
                //hpTMP.text = $"{currentHp.ToString()}/{maxHp.ToString()}";
                if (currentHp <= 0)
                {
                    currentHp = 0;
                    //hpTMP.text = $"{currentHp.ToString()}/{maxHp.ToString()}";
                    Die();
                }
            }
        }
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
            //shieldsBar.fillAmount = currentShields / maxShields;
            //shieldsTMP.text = $"{currentShields.ToString()}/{maxShields.ToString()}";

            yield return new WaitForSeconds(shieldRegenTime);
        }

        currentShields = Mathf.Min(currentShields, maxShields);
        isRegeneratingShields = false;
    }
    public void ApplyShieldRechargeUpgrade(float multiplier)
    {
        shieldRegenTime *= 1f / multiplier;
        startShieldRegenTime *= 1f / multiplier * 2.5f;
    }
}