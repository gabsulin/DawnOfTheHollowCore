using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject nextArrow;
    [SerializeField] private Button nextButton;

    [Header("Settings")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Mouse0;
    [SerializeField] private float ambientDialogueDuration = 2f;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string fullLineText;

    private NPCData currentNPC;
    private List<string> currentDialogueLines;
    private int currentLineIndex = 0;
    private System.Action onDialogueComplete;
    private bool isShowingDialogue = false;
    private bool isAmbientDialogue = false;
    private float ambientTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (nextButton != null)
            nextButton.onClick.AddListener(AdvanceDialogue);
    }

    private void Update()
    {
        if (!isShowingDialogue) return;

        if (isAmbientDialogue)
        {
            ambientTimer -= Time.deltaTime;
            if (ambientTimer <= 0f)
            {
                HideDialogue();
            }
            return;
        }

        if (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }
    public bool IsShowingDialogue()
    {
        return isShowingDialogue && !isAmbientDialogue;
    }

    public void StartDialogue(NPCData npcData, System.Action onComplete)
    {
        if (npcData == null || npcData.dialogueLines == null || npcData.dialogueLines.Count == 0)
        {
            Debug.LogWarning("[DialogueUI] No dialogue lines available");
            return;
        }

        currentNPC = npcData;
        currentDialogueLines = new List<string>(npcData.dialogueLines);
        currentLineIndex = 0;
        onDialogueComplete = onComplete;
        isShowingDialogue = true;
        isAmbientDialogue = false;

        if (PlayerController.Instance != null)
            PlayerController.Instance.SetMovementEnabled(false);

        ShowDialoguePanel();
        DisplayCurrentLine();
    }

    public void ShowAmbientDialogue(string npcName, string text)
    {
        currentNPC = null;
        isShowingDialogue = true;
        isAmbientDialogue = true;
        ambientTimer = ambientDialogueDuration;

        ShowDialoguePanel();

        if (npcNameText != null)
            npcNameText.text = npcName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(text));

        if (nextArrow != null)
            nextArrow.SetActive(false);
    }

    private void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }

    private void DisplayCurrentLine()
    {
        if (currentLineIndex >= currentDialogueLines.Count)
        {
            CompleteDialogue();
            return;
        }

        string line = currentDialogueLines[currentLineIndex];

        if (npcNameText != null)
            npcNameText.text = currentNPC.npcName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line));


        bool hasMoreLines = currentLineIndex < currentDialogueLines.Count - 1;
        if (nextArrow != null)
            nextArrow.SetActive(hasMoreLines);
    }

    private void AdvanceDialogue()
    {
        if (isAmbientDialogue) return;
        if (!isShowingDialogue) return;

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = fullLineText;
            isTyping = false;
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentDialogueLines.Count)
        {
            CompleteDialogue();
        }
        else
        {
            DisplayCurrentLine();
        }
    }

    private void CompleteDialogue()
    {
        HideDialogue();

        if (PlayerController.Instance != null)
            PlayerController.Instance.SetMovementEnabled(true);

        onDialogueComplete?.Invoke();

        currentNPC = null;
        currentDialogueLines = null;
        onDialogueComplete = null;
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        isShowingDialogue = false;
        isAmbientDialogue = false;
        currentLineIndex = 0;
    }

    public void ForceClose()
    {
        HideDialogue();

        if (PlayerController.Instance != null)
            PlayerController.Instance.SetMovementEnabled(true);
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        fullLineText = line;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}