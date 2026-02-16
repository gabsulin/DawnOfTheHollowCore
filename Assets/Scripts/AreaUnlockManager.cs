using UnityEngine;
using System.Collections.Generic;

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
    public bool CanEnterArea(int areaId)
    {
        if (areaId == 0) return true;

        if (IsAreaPermanentlyUnlocked(areaId)) return true;

        if (!requirementsByAreaId.ContainsKey(areaId))
        {
            DebugLog($"Area {areaId} has no requirements defined - allowing entry");
            return true;
        }

        var req = requirementsByAreaId[areaId];

        if (!string.IsNullOrEmpty(req.requiredNPCId) && !req.npcDialogueCompleted)
        {
            DebugLog($"Cannot enter Area {areaId}: NPC dialogue not completed ({req.requiredNPCId})");
            return false;
        }

        if (areaId == 1)
        {
            return req.npcDialogueCompleted;
        }

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
    public bool TryUnlockArea(int areaId)
    {
        if (areaId == 0) return true;

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

        if (!string.IsNullOrEmpty(req.requiredNPCId) && !req.npcDialogueCompleted)
        {
            DebugLog($"Cannot unlock Area {areaId}: NPC dialogue not completed");
            return false;
        }

        if (areaId == 1)
        {
            req.areaUnlocked = true;
            SaveProgressionState();
            DebugLog($"Area {areaId} unlocked permanently (no key required)");
            return true;
        }

        if (req.requiredKey != null)
        {
            if (inventoryManager == null) return false;

            if (inventoryManager.HasItem(req.requiredKey, 1))
            {
                inventoryManager.RemoveItem(req.requiredKey, 1);

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
    public void OnNPCDialogueCompleted(string npcId, int associatedAreaId)
    {
        DebugLog($"NPC Dialogue Completed: {npcId} for Area {associatedAreaId}");

        if (requirementsByAreaId.ContainsKey(associatedAreaId))
        {
            requirementsByAreaId[associatedAreaId].npcDialogueCompleted = true;
            SaveProgressionState();

            DebugLog($"Area {associatedAreaId} NPC requirement met");

        }
    }

    public bool IsAreaPermanentlyUnlocked(int areaId)
    {
        if (areaId == 0) return true;

        if (requirementsByAreaId.ContainsKey(areaId))
        {
            return requirementsByAreaId[areaId].areaUnlocked;
        }

        return false;
    }
    public ItemSO GetRequiredKey(int areaId)
    {
        if (requirementsByAreaId.ContainsKey(areaId))
        {
            return requirementsByAreaId[areaId].requiredKey;
        }
        return null;
    }
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
    public AreaRequirement GetRequirement(int areaId)
    {
        if (requirementsByAreaId.ContainsKey(areaId))
        {
            return requirementsByAreaId[areaId];
        }
        return null;
    }
    public bool IsNPCDialogueCompleted(int areaId)
    {
        if (requirementsByAreaId.ContainsKey(areaId))
        {
            return requirementsByAreaId[areaId].npcDialogueCompleted;
        }
        return false;
    }

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
    public void ResetAllProgression()
    {
        foreach (var req in areaRequirements)
        {
            if (req.areaId != 0)
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