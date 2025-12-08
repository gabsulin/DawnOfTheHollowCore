using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] private GameObject worldItemPrefab;
    [SerializeField] private float popForce = 2f;

    public void Drop(ItemSO item, int amount, Vector3 position)
    {
        if (item == null || worldItemPrefab == null) return;
        var go = Instantiate(worldItemPrefab, position, Quaternion.identity);
        var wi = go.GetComponent<WorldItem>();
        if (wi != null) wi.Initialize(item, amount);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(0.6f, 1.2f)) * popForce, ForceMode2D.Impulse);
    }
}
