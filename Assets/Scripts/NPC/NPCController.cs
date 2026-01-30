using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    public NPCData npcData;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;

    private Transform player;
    private bool playerInRange = false;
    private bool dialogueCompleted = false;

    private string npcId;

    private void Start()
    {
        player = PlayerController.Instance?.transform;
        
        if(player == null)
        {
            Debug.LogError("PlayerController instance not found in the scene.");
        }

        npcId = $"{npcData?.name}_{transform.position.x:F1}_{transform.position.y:F2}";

        LoadCompletionState();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if(playerInRange != wasInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(playerInRange);
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (npcData == null)
        {
            Debug.LogWarning("NPC Data is not assigned.");
            return;
        }

        if (DialogueUI.Instance == null)
        {
            Debug.LogError("DialogueUI instance not found in the scene.");
            return;
        }

        if (dialogueCompleted)
        {
            if(npcData.ambientDialogue != null && npcData.ambientDialogue.Count > 0)
            {
                string randomLine = npcData.ambientDialogue[Random.Range(0, npcData.ambientDialogue.Count)];
                DialogueUI.Instance.ShowAmbientDialogue(npcData.npcName, randomLine);
            }
            return;
        }
        DialogueUI.Instance.StartDialogue(npcData, OnDialogueComplete);
    }

    private void OnDialogueComplete()
    {
        dialogueCompleted = true;
        if (npcData.recipesToUnlock != null && npcData.recipesToUnlock.Count > 0)
        {
            RecipeManager.Instance?.UnlockRecipes(npcData.recipesToUnlock);
        }
        SaveCompletionState();

        Debug.Log("Dialogue with " + npcData.npcName + " completed.");
    }

    private void SaveCompletionState()
    {
        PlayerPrefs.SetInt($"NPC_Completed_{npcId}", dialogueCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadCompletionState()
    {
        dialogueCompleted = PlayerPrefs.GetInt($"NPC_Completed_{npcId}", 0) == 1;

        if (dialogueCompleted)
        {
            Debug.Log("Loaded completion state for " + npcData.npcName);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
