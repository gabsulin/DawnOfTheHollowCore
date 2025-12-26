using UnityEngine;

public class PlayerCraftingSensor : MonoBehaviour
{
    public CraftingPool currentPool;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pool = other.GetComponent<CraftingPool>();
        if (pool != null)
            Debug.Log("pool detected");
        currentPool = pool;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pool = other.GetComponent<CraftingPool>();
        if (pool != null && pool == currentPool)
        {
            Debug.Log("pool exited");
            currentPool.AttemptCraft();
            currentPool.ReturnAllToInventory();
            currentPool = null;
        }
    }
}
