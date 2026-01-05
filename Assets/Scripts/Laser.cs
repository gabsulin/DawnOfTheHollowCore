using UnityEngine;
using System.Collections.Generic;

public class Laser : MonoBehaviour
{
    public enum LaserMode { Off, Attack, Mine }
    public LaserMode mode = LaserMode.Off;

    public Camera cam;
    public LineRenderer lineRenderer;
    public Transform firePoint;
    public GameObject startVFX;
    public GameObject endVFX;

    private InventoryController inventoryController;

    public LayerMask ignoreLayers;

    public int laserDamage = 1;

    private List<ParticleSystem> particles = new List<ParticleSystem>();
    private Quaternion rotation;

    private float miningCooldown = 0.08f;
    private float miningTimer = 0f;

    void Start()
    {
        FillLists();
        DisableLaser();
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    void Update()
    {
        if (inventoryController.open) return;

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
    private void UpdateLaser_Attack()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        lineRenderer.SetPosition(0, firePoint.position);

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            Vector2.Distance(firePoint.position, mousePos),
            ~ignoreLayers
        );

        if (hit)
        {
            lineRenderer.SetPosition(1, hit.point);
            endVFX.transform.position = hit.point;

            if (hit.collider.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                enemy.TakeDamage(laserDamage);
        }
        else
        {
            lineRenderer.SetPosition(1, mousePos);
            endVFX.transform.position = mousePos;
        }
    }


    private void UpdateLaser_Mine()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        lineRenderer.SetPosition(0, firePoint.position);

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            Vector2.Distance(firePoint.position, mousePos),
            ~ignoreLayers
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


    void EnableLaser()
    {
        lineRenderer.enabled = true;

        foreach (var ps in particles)
            ps.Play();
    }

    private void DisableLaser()
    {
        lineRenderer.enabled = false;

        foreach (var ps in particles)
            ps.Stop();
    }

    private void RotateToMouse()
    {
        Vector2 direction = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation.eulerAngles = new Vector3(0, 0, angle);
        transform.rotation = rotation;
    }

    private void FillLists()
    {
        foreach (Transform child in startVFX.transform)
        {
            ParticleSystem ps = child.GetComponent<ParticleSystem>();
            if (ps) particles.Add(ps);
        }

        foreach (Transform child in endVFX.transform)
        {
            ParticleSystem ps = child.GetComponent<ParticleSystem>();
            if (ps) particles.Add(ps);
        }
    }
    public void ApplyDamageUpgrade(int amount)
    {
        laserDamage += amount;
    }

    public void ApplyMiningSpeedUpgrade(float multiplier)
    {
        miningCooldown *= 1f / multiplier;
    }

}
