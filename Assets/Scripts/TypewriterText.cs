using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI dialogueText;

    [Header("Typewriter Settings")]
    [SerializeField] float typeSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private string fullText;
    private bool isTyping;
    
    public void ShowText(string text)
    {
        if(typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        fullText = text;
        dialogueText.text = "";
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    public void SkipTyping()
    {
        if (!isTyping) return;

        StopCoroutine(typingCoroutine);
        dialogueText.text = fullText;
        isTyping = false;
    }

    public bool IsTyping()
    {
        return isTyping;
    }
}
