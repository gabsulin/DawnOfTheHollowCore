using UnityEngine;
using UnityEngine.UI;

public class AbilityHolder : MonoBehaviour
{
    public Ability ability;
    [HideInInspector] public float cooldownTime;
    float activeTime;
    [SerializeField] Image abilityBar;

    Rigidbody2D rb;

    private InventoryController inventoryController;
    enum AbilityState
    {
        ready,
        active,
        cooldown
    }

    AbilityState state = AbilityState.ready;

    public KeyCode key;

    public bool isReset = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    public void AssignBar()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) abilityBar = canvas.transform.Find("AbilityBar/BarHolder/Bar").GetComponent<Image>();
        else Debug.Log("Smula");
    }
    public void UpdateBar()
    {
        if (abilityBar != null) abilityBar.fillAmount = 1f;
    }
    void Update()
    {
        switch (state)
        {
            case AbilityState.ready:
                if (Input.GetKeyDown(key) && !IsPlayerBlocked())
                {
                    ability.Activate(gameObject);
                    state = AbilityState.active;
                    activeTime = ability.activeTime;
                }
                break;
            case AbilityState.active:
                if (activeTime > 0)
                {
                    activeTime -= Time.deltaTime;
                    abilityBar.fillAmount = activeTime / ability.activeTime;
                }
                else
                {
                    state = AbilityState.cooldown;
                    cooldownTime = ability.coolDownTime;
                }
                break;
            case AbilityState.cooldown:
                if (cooldownTime > 0)
                {
                    cooldownTime -= Time.deltaTime;

                    float elapsedCooldown = ability.coolDownTime - cooldownTime;
                    abilityBar.fillAmount = elapsedCooldown / ability.coolDownTime;

                    if (!isReset)
                    {
                        ResetAbility();
                        isReset = true;
                    }
                }
                else
                {
                    state = AbilityState.ready;
                    abilityBar.fillAmount = 1f;
                }
                break;
        }
    }
    private bool IsPlayerBlocked()
    {
        if (inventoryController != null && inventoryController.open)
        {
            return true;
        }

        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowingDialogue())
        {
            return true;
        }

        return false;
    }
    private void ResetAbility()
    {
        if (ability is 
            Ability)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
