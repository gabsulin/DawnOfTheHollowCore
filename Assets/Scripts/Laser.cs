using UnityEngine;
using System.Collections.Generic;

public class Laser : MonoBehaviour
{
    public enum LaserMode { Off, Attack, Mine }
    public LaserMode mode = LaserMode.Off;

    [Header("References")]
    public Camera cam;
    public LineRenderer lineRenderer;
    public Transform firePoint;
    public GameObject startVFX;
    public GameObject endVFX;
    private TutorialScene tutorialManager;

    private InventoryController inventoryController;

    [Header("Layer Settings")]
    public LayerMask ignorePlayerLayer;
    public LayerMask ignoreAttackHitboxLayer;

    [Header("Damage Settings")]
    public int laserDamage = 1;

    [Header("Laser Colors")]
    public Color attackColor = Color.red;
    public Color miningColor = Color.cyan;

    private List<ParticleSystem> particles = new List<ParticleSystem>();
    private Quaternion rotation;

    private float miningCooldown = 0.08f;
    private float miningTimer = 0f;

    private MaterialPropertyBlock mpb;

    void Start()
    {
        FillLists();
        DisableLaser();
        inventoryController = FindFirstObjectByType<InventoryController>();
        tutorialManager = FindFirstObjectByType<TutorialScene>();
        mpb = new MaterialPropertyBlock();

        Physics2D.queriesHitTriggers = false;
    }

    void Update()
    {
        if (IsPlayerBlocked())
        {
            if (mode != LaserMode.Off)
            {
                DisableLaser();
                mode = LaserMode.Off;
            }
            return;
        }

        if (Input.GetButton("Fire1") && Input.GetButton("Fire2"))
        {
            DisableLaser();
            mode = LaserMode.Off;
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            mode = LaserMode.Attack;
            EnableLaser();
        }

        if (Input.GetButton("Fire1") && mode == LaserMode.Attack)
            UpdateLaser_Attack();

        if (Input.GetButtonUp("Fire1") && mode == LaserMode.Attack)
        {
            DisableLaser();
            mode = LaserMode.Off;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            mode = LaserMode.Mine;
            EnableLaser();
        }

        if (Input.GetButton("Fire2") && mode == LaserMode.Mine)
            UpdateLaser_Mine();

        if (Input.GetButtonUp("Fire2") && mode == LaserMode.Mine)
        {
            DisableLaser();
            mode = LaserMode.Off;
        }

        RotateToMouse();
    }
    private bool IsPlayerBlocked()
    {
        if (inventoryController != null && inventoryController.open)
            return true;

        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowingDialogue())
            return true;

        if (tutorialManager != null && tutorialManager.IsShowingTutorial())
            return true;

        return false;
    }

    private void UpdateLaser_Attack()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        lineRenderer.SetPosition(0, firePoint.position);

        int finalMask = ~(ignorePlayerLayer | ignoreAttackHitboxLayer);

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            Vector2.Distance(firePoint.position, mousePos),
            finalMask
        );

        if (hit)
        {
            lineRenderer.SetPosition(1, hit.point);
            endVFX.transform.position = hit.point;

            if (hit.collider.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                enemy.TakeDamage(laserDamage);
            if (hit.collider.TryGetComponent<BossHpSystem>(out BossHpSystem boss))
                boss.TakeHit(laserDamage);
        }
        else
        {
            lineRenderer.SetPosition(1, mousePos);
            endVFX.transform.position = mousePos;
        }
    }

    // ============================================================
    // MINING LASER
    // ============================================================
    private void UpdateLaser_Mine()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        lineRenderer.SetPosition(0, firePoint.position);

        int finalMask = ~(ignorePlayerLayer | ignoreAttackHitboxLayer);

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            Vector2.Distance(firePoint.position, mousePos),
            finalMask
        );

        if (hit)
        {
            lineRenderer.SetPosition(1, hit.point);
            endVFX.transform.position = hit.point;

            if (hit.collider.TryGetComponent<OreNode>(out OreNode ore))
            {
                miningTimer -= Time.deltaTime;
                if (miningTimer <= 0f)
                {
                    ore.TakeDamage(laserDamage);
                    miningTimer = miningCooldown;
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(1, mousePos);
            endVFX.transform.position = mousePos;
        }
    }

    // ============================================================
    // VISUALS
    // ============================================================
    void EnableLaser()
    {
        lineRenderer.enabled = true;
        SetLaserModeVisuals();

        foreach (var ps in particles)
            ps.Play();
    }

    private void DisableLaser()
    {
        lineRenderer.enabled = false;

        foreach (var ps in particles)
            ps.Stop();
    }

    private void SetLaserModeVisuals()
    {
        if (!lineRenderer) return;

        Color color = mode switch
        {
            LaserMode.Attack => attackColor,
            LaserMode.Mine => miningColor,
            _ => Color.white
        };

        lineRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        lineRenderer.SetPropertyBlock(mpb);

        ApplyColorToVFX(startVFX, color);
        ApplyColorToVFX(endVFX, color);
    }

    private void ApplyColorToVFX(GameObject vfxObject, Color color)
    {
        if (!vfxObject) return;

        foreach (Transform child in vfxObject.transform)
        {
            if (child.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = color;
            }
        }
    }

    // ============================================================
    // UTILS
    // ============================================================
    private void RotateToMouse()
    {
        Vector2 direction = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation.eulerAngles = new Vector3(0, 0, angle);
        transform.rotation = rotation;
    }

    private void FillLists()
    {
        particles.Clear();

        foreach (Transform child in startVFX.transform)
            if (child.TryGetComponent(out ParticleSystem ps1)) particles.Add(ps1);

        foreach (Transform child in endVFX.transform)
            if (child.TryGetComponent(out ParticleSystem ps2)) particles.Add(ps2);
    }

    // ============================================================
    // UPGRADES
    // ============================================================
    public void ApplyDamageUpgrade(int amount)
    {
        laserDamage += amount;
    }

    public void ApplyMiningSpeedUpgrade(float multiplier)
    {
        miningCooldown *= 1f / multiplier;
    }
}