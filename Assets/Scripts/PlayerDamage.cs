using System.Collections;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    PlayerHpSystem playerHp;
    Animator anim;
    CameraShake cameraShake;

    [SerializeField] int damage;
    [SerializeField] bool destroy = true;

    [Header("Lifetime Settings")]
    [SerializeField] bool useLifetime = false;
    [SerializeField] float lifeTime = 2f;

    [SerializeField] ParticleSystem particles;

    void Start()
    {
        playerHp = FindFirstObjectByType<PlayerHpSystem>();
        anim = GetComponent<Animator>();
        cameraShake = GetComponent<CameraShake>();

        if (useLifetime)
            StartCoroutine(LifetimeDespawn());
    }

    private void Update()
    {
        if (playerHp != null && playerHp.isDead && destroy)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        playerHp = collision.GetComponent<PlayerHpSystem>();
        if (playerHp != null)
        {
            playerHp.TakeHit(damage);
            cameraShake.StartShake(force: 0.1f);

            if (anim != null)
            {
                if (anim.GetBool("Hit") == true)
                {
                    anim.SetBool("Hit", true);
                    gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

                    if (destroy) Destroy(gameObject, 0.3f);
                }
                else if (destroy)
                {
                    Destroy(gameObject);
                }
            }
            else if (destroy)
            {
                Destroy(gameObject);
            }

            if (particles != null)
            {
                Instantiate(particles, playerHp.transform.position, Quaternion.identity);
            }
        }
    }

    IEnumerator LifetimeDespawn()
    {
        yield return new WaitForSeconds(lifeTime);

        if (destroy)
            Destroy(gameObject);
    }
}
