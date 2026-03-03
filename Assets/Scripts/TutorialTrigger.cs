using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public TutorialScene tutorialManager;
    public int tutorialPanelIndex = 0;
    public bool triggerOnce = true;
    public bool destroyAfterTrigger = false;

    [Header("Optional: Tag Filter")]
    public string requiredTag = "Player";

    private bool hasTriggered = false;

    void Start()
    {
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

        if (triggerOnce && hasTriggered)
        {
            Debug.Log($"[TutorialTrigger] Already triggered, ignoring");
            return;
        }

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            Debug.Log($"[TutorialTrigger] Tag mismatch. Required: {requiredTag}, Got: {other.tag}");
            return;
        }

        if (tutorialManager != null)
        {
            Debug.Log($"[TutorialTrigger] Triggering tutorial panel {tutorialPanelIndex}");
            tutorialManager.ShowPanel(tutorialPanelIndex);
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