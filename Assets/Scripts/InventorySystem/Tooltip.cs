using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [Header("Refs")]
    public RectTransform panel;
    public TMP_Text nameText;
    public TMP_Text typeText;
    public TMP_Text rarityText;
    public TMP_Text descText;

    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    public void Show(ItemSO item)
    {
        nameText.text = "Name: " + item.itemName;
        typeText.text = "Type: " + item.type.ToString();
        rarityText.text = "Rarity: " + item.rarity.ToString();
        rarityText.color = GetRarityColor(item.rarity);
        descText.text = item.description;

        panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        panel.gameObject.SetActive(false);
    }

    Color GetRarityColor(Rarity r)
    {
        switch (r)
        {
            case Rarity.Rare: return Color.cyan;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Legendary: return new Color(1f, 0.6f, 0f);
            case Rarity.Mythic: return Color.yellow;
            default: return Color.white;
        }
    }
}
