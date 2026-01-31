using UnityEngine;

[RequireComponent(typeof(InventoryManager))]
public class InventoryController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool pauseOnOpen = true;

    private InventoryManager manager;
    public bool open = false;

    private void Awake()
    {
        manager = GetComponent<InventoryManager>();
        if (inventoryUI) inventoryUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
        if (open && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Toggle()
    {
        open = !open;
        if (inventoryUI) inventoryUI.SetActive(open);

        if (pauseOnOpen)
            Time.timeScale = open ? 0f : 1f;
    }

    public void Close()
    {
        open = false;
        if (inventoryUI) inventoryUI.SetActive(false);

        if (pauseOnOpen)
            Time.timeScale = 1f;
    }
}