using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "InventorySystem/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("NPC Identity")]
    public string npcName = "Unknown";

    [Header("Dialogue")]
    [Tooltip("Each string is one dialogue step. Player clicks to advance.")]
    [TextArea(2, 4)]
    public List<string> dialogueLines = new List<string>();

    [Header("Recipe Unlocks")]
    [Tooltip("Recipes that will be unlocked after dialogue completes")]
    public List<RecipeSO> recipesToUnlock = new List<RecipeSO>();

    [Header("Ambient Dialogue (After Completion)")]
    [Tooltip("Short lines shown on repeat interactions")]
    [TextArea(1, 2)]
    public List<string> ambientDialogue = new List<string>();
}