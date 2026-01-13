using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Dash Ability", menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
    [Header("Dash Settings")]
    [SerializeField] private float dashVelocity = 20f;
    [SerializeField] private float dashDuration = 0.2f;

    [Header("Control Settings")]
    [SerializeField] private bool lockMovementDuringDash = true;
    [SerializeField] private bool stopDashOnWallHit = true;

    [Header("Visual Feedback")]
    [SerializeField] private bool createAfterimage = false;
    [SerializeField] private float afterimageInterval = 0.05f;
    [SerializeField] private string dashSFX = "Dash";

    [Header("Invincibility")]
    [SerializeField] private bool grantInvincibility = false;
    [SerializeField] private float invincibilityDuration = 0.2f;

    private Coroutine dashCoroutine;

    public override void Activate(GameObject parent)
    {
        AbilityHolder abilityHolder = parent.GetComponent<AbilityHolder>();
        PlayerController player = parent.GetComponent<PlayerController>();
        Rigidbody2D rb = parent.GetComponent<Rigidbody2D>();

        if (abilityHolder == null || player == null || rb == null)
        {
            Debug.LogWarning("DashAbility: Missing required components!");
            return;
        }

        Vector2 dashDirection = GetDashDirection(player, parent.transform);

        if (!string.IsNullOrEmpty(dashSFX))
        {
            AudioManager.Instance?.PlaySFX(dashSFX);
        }

        if (dashCoroutine != null)
        {
            parent.GetComponent<MonoBehaviour>().StopCoroutine(dashCoroutine);
        }

        dashCoroutine = parent.GetComponent<MonoBehaviour>().StartCoroutine(
            DashCoroutine(parent, rb, player, abilityHolder, dashDirection)
        );
    }

    private Vector2 GetDashDirection(PlayerController player, Transform playerTransform)
    {
        if (player.input.magnitude > 0.1f)
        {
            return player.input.normalized;
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 directionToMouse = (mouseWorldPos - playerTransform.position);

            if (directionToMouse.magnitude > 0.1f)
            {
                return directionToMouse.normalized;
            }
        }

        return player.GetLastMovementDirection();
    }

    private IEnumerator DashCoroutine(GameObject parent, Rigidbody2D rb,
        PlayerController player, AbilityHolder abilityHolder, Vector2 direction)
    {
        if (lockMovementDuringDash)
        {
            player.canMove = false;
        }

        if (grantInvincibility)
        {
            EnableInvincibility(parent);
        }

        float elapsed = 0f;
        Vector2 dashVelocityVector = direction * dashVelocity;
        bool dashCancelled = false;

        float nextAfterimageTime = 0f;

        while (elapsed < dashDuration && !dashCancelled)
        {
            elapsed += Time.deltaTime;

            rb.linearVelocity = dashVelocityVector;

            if (createAfterimage && elapsed >= nextAfterimageTime)
            {
                CreateAfterimage(parent);
                nextAfterimageTime = elapsed + afterimageInterval;
            }

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (lockMovementDuringDash)
        {
            player.canMove = true;
        }
        if (grantInvincibility && invincibilityDuration > dashDuration)
        {
            yield return new WaitForSeconds(invincibilityDuration - dashDuration);
            DisableInvincibility(parent);
        }
        else if (grantInvincibility)
        {
            DisableInvincibility(parent);
        }

        abilityHolder.isReset = false;
    }

    private void EnableInvincibility(GameObject parent)
    {
        var hpSystem = parent.GetComponent<PlayerHpSystem>();
        if (hpSystem != null)
        {
            hpSystem.isImmune = true;
        }
    }

    private void DisableInvincibility(GameObject parent)
    {
        var hpSystem = parent.GetComponent<PlayerHpSystem>();
        if (hpSystem != null)
        {
            hpSystem.isImmune = false;
        }
    }

    private void CreateAfterimage(GameObject parent)
    {
        // Implement afterimage spawning here
        // This could instantiate a sprite that fades out
        // Example:
        // GameObject afterimage = Instantiate(afterimagePrefab, parent.transform.position, parent.transform.rotation);
    }

    public void OnDashCollision(GameObject parent)
    {
        if (stopDashOnWallHit && dashCoroutine != null)
        {
            parent.GetComponent<MonoBehaviour>().StopCoroutine(dashCoroutine);
            parent.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            parent.GetComponent<PlayerController>().canMove = true;
        }
    }
}