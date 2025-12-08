using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeOutTime = 1f;
    public float lifeTime = 1f;

    private TextMeshProUGUI damageText;
    private Color startColor;
    private float elapsedTime = 0f;
    private RectTransform rectTransform;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        if (damageText != null)
            startColor = damageText.color;

        Destroy(gameObject, Mathf.Max(lifeTime, fadeOutTime));
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime * 100f;
        }

        if (damageText != null)
        {
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeOutTime);
            Color newColor = startColor;
            newColor.a = alpha;
            damageText.color = newColor;
        }
    }

    public void SetText(string text)
    {
        if (damageText != null)
            damageText.text = text;
    }
}
