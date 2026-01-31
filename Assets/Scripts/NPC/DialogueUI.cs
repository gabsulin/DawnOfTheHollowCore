using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

        // Handle ambient dialogue timer
        if (isAmbientDialogue)
        {
            ambientTimer -= Time.deltaTime;
            if (ambientTimer <= 0f)
            {
                HideDialogue();
            }
            return;
        }

        // Advance dialogue with key press or mouse click
        if (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    /// <summary>
    /// PUBLIC METHOD: Check if dialogue is currently showing (for Laser script)
    /// Returns true for main dialogue, false for ambient dialogue
    /// </summary>
    public bool IsShowingDialogue()
    {
        // Only return true for main dialogue (not ambient)
        // Ambient dialogue is just flavor text and shouldn't block actions
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

        // Disable player movement during dialogue
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

        if (dialogueText != null)
            dialogueText.text = text;

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

        if (dialogueText != null)
            dialogueText.text = line;

        bool hasMoreLines = currentLineIndex < currentDialogueLines.Count - 1;
        if (nextArrow != null)
            nextArrow.SetActive(hasMoreLines);
    }

    private void AdvanceDialogue()
    {
        if (isAmbientDialogue) return;
        if (!isShowingDialogue) return;

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

        // Re-enable player movement
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetMovementEnabled(true);

        // Trigger completion callback (unlocks recipes)
        onDialogueComplete?.Invoke();

        // Clear state
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
}