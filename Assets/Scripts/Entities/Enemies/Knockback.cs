using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public bool gettingKnockedBack { get; private set; }
    [SerializeField] private float knockbackTime = 0.2f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockback(Transform damageSource, float knockbackThrust)
    {
        gettingKnockedBack = true;
        Vector2 difference = (transform.position - damageSource.position).normalized * knockbackThrust * rb.mass;
        rb.AddForce(difference, ForceMode2D.Impulse);
        StartCoroutine(KnockRoutine());
    }

    private IEnumerator KnockRoutine()
    {
        yield return new WaitForSeconds(knockbackTime);
        rb.linearVelocity = Vector2.zero;
        gettingKnockedBack = false;
    }
}
