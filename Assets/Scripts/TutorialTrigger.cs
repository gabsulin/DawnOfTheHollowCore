using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public TutorialScene tutorialManager;
    public int tutorialStepIndex = 0;
    public bool triggerOnce = true;
    public bool destroyAfterTrigger = false;

    [Header("Optional: Tag Filter")]
    public string requiredTag = "Player";

    private bool hasTriggered = false;

    void Start()
    {
        // Auto-find tutorial manager if not assigned
        if (tutorialManager == null)
        {
            tutorialManager = FindFirstObjectByType<TutorialScene>();
            if (tutorialManager == null)
            {
                Debug.LogError("TutorialTrigger: No TutorialScene found in scene!");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[TutorialTrigger] Something entered trigger: {other.gameObject.name} with tag: {other.tag}");

        // Check if already triggered
        if (triggerOnce && hasTriggered)
        {
            Debug.Log($"[TutorialTrigger] Already triggered, ignoring");
            return;
        }

        // Check tag if specified
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            Debug.Log($"[TutorialTrigger] Tag mismatch. Required: {requiredTag}, Got: {other.tag}");
            return;
        }

        // Trigger tutorial
        if (tutorialManager != null)
        {
            Debug.Log($"[TutorialTrigger] Triggering tutorial step {tutorialStepIndex}");
            tutorialManager.ShowTutorialStep(tutorialStepIndex);
            hasTriggered = true;

            if (destroyAfterTrigger)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("[TutorialTrigger] Tutorial manager is null!");
        }
    }

    // Optional: Visualize trigger area in editor
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.offset, boxCol.size);
        }
    }
}