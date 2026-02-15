using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages area progression - tracks unlocks, NPC dialogue completion, and key consumption
/// </summary>
public class AreaUnlockManager : MonoBehaviour
{
    public static AreaUnlockManager Instance { get; private set; }

    [System.Serializable]
    public class AreaRequirement
    {
        [Tooltip("Area ID (0-4)")]
        public int areaId;

        [Tooltip("Must complete this NPC's dialogue to unlock (null for Area 0)")]
        public string requiredNPCId;

        [Tooltip("Key item needed to enter (null for Area 0 and 1)")]
        public ItemSO requiredKey;

        [HideInInspector]
        public bool npcDialogueCompleted = false;

        [HideInInspector]
        public bool areaUnlocked = false;
    }

    [Header("Area Requirements")]
    [Tooltip("Define requirements for each area (Area 0 should have no requirements)")]
    public List<AreaRequirement> areaRequirements = new List<AreaRequirement>();

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Cache for quick lookups
    private Dictionary<int, AreaRequirement> requirementsByAreaId;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeCache();
    }

    private void Start()
    {
        inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (inventoryManager == null)
        {
            Debug.LogError("[AreaUnlockManager] InventoryManager not found in scene!");
        }

        // Area 0 is always unlocked
        if (requirementsByAreaId.ContainsKey(0))
        {
            requirementsByAreaId[0].areaUnlocked = true;
        }

        LoadProgressionState();
    }

    private void InitializeCache()
    {
        requirementsByAreaId = new Dictionary<int, AreaRequirement>();

        foreach (var req in areaRequirements)
        {
            if (!requirementsByAreaId.ContainsKey(req.areaId))
            {
                requirementsByAreaId.Add(req.areaId, req);
            }
            else
            {
                Debug.LogWarning($"[AreaUnlockManager] Duplicate area ID found: {req.areaId}");
            }
        }
    }

    /// <summary>
    /// Check if player can enter a specific area
    /// </summary>
    public bool CanEnterArea(int areaId)
    {
        // Area 0 is always accessible
        if (areaId == 0) return true;

        // Check if area is already permanently unlocked
        if (IsAreaPermanentlyUnlocked(areaId)) return true;

        // Check if area exists in our requirements
        if (!requirementsByAreaId.ContainsKey(areaId))
        {
            DebugLog($"Area {areaId} has no requirements defined - allowing entry");
            return true;
        }

        var req = requirementsByAreaId[areaId];

        // Check NPC dialogue requirement
        if (!string.IsNullOrEmpty(req.requiredNPCId) && !req.npcDialogueCompleted)
        {
            DebugLog($"Cannot enter Area {areaId}: NPC dialogue not completed ({req.requiredNPCId})");
            return false;
        }

        // Area 1 only needs NPC dialogue (no key required)
        if (areaId == 1)
        {
            return req.npcDialogueCompleted;
        }

        // For Area 2+, check if player has the required key
        if (req.requiredKey != null)
        {
            if (inventoryManager == null)
            {
                Debug.LogError("[AreaUnlockManager] InventoryManager is null!");
                return false;
            }

            bool hasKey = inventoryManager.HasItem(req.requiredKey, 1);

            if (!hasKey)
            {
                DebugLog($"Cannot enter Area {areaId}: Player doesn't have key ({req.requiredKey.itemName})");
            }

            return hasKey;
        }

        return true;
    }

    /// <summary>
    /// Attempt to unlock an area (consumes key if needed)
    /// </summary>
    public bool TryUnlockArea(int areaId)
    {
        if (areaId == 0) return true; // Area 0 always accessible

        // Already permanently unlocked
        if (IsAreaPermanentlyUnlocked(areaId))
        {
            DebugLog($"Area {areaId} is already permanently unlocked");
            return true;
        }

        if (!requirementsByAreaId.ContainsKey(areaId))
        {
            DebugLog($"Area {areaId} has no requirements");
            return true;
        }

        var req = requirementsByAreaId[areaId];

        // Check NPC dialogue
        if (!string.IsNullOrEmpty(req.requiredNPCId) && !req.npcDialogueCompleted)
        {
            DebugLog($"Cannot unlock Area {areaId}: NPC dialogue not completed");
            return false;
        }

        // Area 1 - just dialogue needed
        if (areaId == 1)
        {
            req.areaUnlocked = true;
            SaveProgressionState();
            DebugLog($"Area {areaId} unlocked permanently (no key required)");
            return true;
        }

        // Area 2+ - need key
        if (req.requiredKey != null)
        {
            if (inventoryManager == null) return false;

            if (inventoryManager.HasItem(req.requiredKey, 1))
            {
                // CONSUME THE KEY
                inventoryManager.RemoveItem(req.requiredKey, 1);

                // PERMANENTLY UNLOCK THE AREA
                req.areaUnlocked = true;
                SaveProgressionState();

                DebugLog($"Area {areaId} unlocked permanently! Key consumed: {req.requiredKey.itemName}");
                return true;
            }
            else
            {
                DebugLog($"Cannot unlock Area {areaId}: Missing key");
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Called when an NPC dialogue is completed
    /// </summary>
    public void OnNPCDialogueCompleted(string npcId, int associatedAreaId)
    {
        DebugLog($"NPC Dialogue Completed: {npcId} for Area {associatedAreaId}");

        if (requirementsByAreaId.ContainsKey(associatedAreaId))
        {
            requirementsByAreaId[associatedAreaId].npcDialogueCompleted = true;
            SaveProgressionState();

            DebugLog($"Area {associatedAreaId} NPC requirement met");

            // SPECIAL CASE: Area 1 doesn't need a key, just dialogue
            // So we should immediately deactivate its death zone
            if (associatedAreaId == 1)
            {
                // Find and deactivate the Area 1 death zone
                AreaDeathZone[] allDeathZones = FindObjectsByType<AreaDeathZone>(FindObjectsSortMode.None);
                foreach (var zone in allDeathZones)
                {
                    if (zone.ProtectedAreaId == 1)
                    {
                        zone.DeactivateZone();
                        DebugLog($"Area 1 death zone deactivated (no key required)");
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if an area is permanently unlocked (key was consumed)
    /// </summary>
    public bool IsAreaPermanentlyUnlocked(int areaId)
    {
        if (areaId == 0) return true;

        if (requirementsByAreaId.ContainsKey(areaId))
        {
            return requirementsByAreaId[areaId].areaUnlocked;
        }

        return false;
    }

    /// <summary>
    /// Get the required key for an area (for UI display)
    /// </summary>
    public ItemSO GetRequiredKey(int areaId)
    {
        if (requirementsByAreaId.ContainsKey(areaId))
        {
            return requirementsByAreaId[areaId].requiredKey;
        }
        return null;
    }

    /// <summary>
    /// Get requirement info for UI display
    /// </summary>
    public string GetAreaRequirementText(int areaId)
    {
        if (areaId == 0) return "Always accessible";

        if (!requirementsByAreaId.ContainsKey(areaId))
        {
            return "No requirements";
        }

        var req = requirementsByAreaId[areaId];

        if (req.areaUnlocked)
        {
            return "Unlocked";
        }

        List<string> missing = new List<string>();

        if (!string.IsNullOrEmpty(req.requiredNPCId) && !req.npcDialogueCompleted)
        {
            missing.Add("Talk to NPC");
        }

        if (req.requiredKey != null && !req.areaUnlocked)
        {
            missing.Add($"Need: {req.requiredKey.itemName}");
        }

        return missing.Count > 0 ? string.Join(", ", missing) : "Requirements met";
    }

    // ==================== SAVE/LOAD ====================

    private void SaveProgressionState()
    {
        foreach (var req in areaRequirements)
        {
            PlayerPrefs.SetInt($"Area_{req.areaId}_NPCCompleted", req.npcDialogueCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"Area_{req.areaId}_Unlocked", req.areaUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadProgressionState()
    {
        foreach (var req in areaRequirements)
        {
            req.npcDialogueCompleted = PlayerPrefs.GetInt($"Area_{req.areaId}_NPCCompleted", 0) == 1;
            req.areaUnlocked = PlayerPrefs.GetInt($"Area_{req.areaId}_Unlocked", 0) == 1;
        }

        DebugLog("Progression state loaded");
    }

    /// <summary>
    /// Reset all progression (for debugging/new game)
    /// </summary>
    public void ResetAllProgression()
    {
        foreach (var req in areaRequirements)
        {
            if (req.areaId != 0) // Don't lock Area 0
            {
                req.npcDialogueCompleted = false;
                req.areaUnlocked = false;
            }
        }

        SaveProgressionState();
        DebugLog("All progression reset");
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[AreaUnlockManager] {message}");
        }
    }
}