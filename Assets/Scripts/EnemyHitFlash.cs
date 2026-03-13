using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float resetDelay = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine resetCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Flash()
    {
        spriteRenderer.color = hitColor;

        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetColor());
    }

    private IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(resetDelay);
        spriteRenderer.color = originalColor;
        resetCoroutine = null;
    }
}