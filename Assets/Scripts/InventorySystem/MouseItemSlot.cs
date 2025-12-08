using UnityEngine;
using UnityEngine.UI;

public class MouseItemSlot : MonoBehaviour
{
    public static MouseItemSlot Instance { get; private set; }

    public Image icon;
    public UnityEngine.UI.Text amountText;

    public ItemSO item;
    public int amount;

    private void Awake()
    {
        Instance = this;
        Clear();
    }

    private void Update()
    {
        if (item != null) transform.position = Input.mousePosition;
    }

    public void Set(ItemSO i, int a)
    {
        item = i; amount = a;
        icon.sprite = i.icon; icon.enabled = true;
        amountText.text = a > 1 ? a.ToString() : "";
    }

    public void Clear()
    {
        item = null; amount = 0; icon.enabled = false; amountText.text = "";
    }
}
