using UnityEngine;

public class OreNode : MonoBehaviour
{
    public enum Rarity { Common, Rare, Epic, Legendary, Mythic }
    public Rarity rarity;

    public int maxHP = 20;
    public int currentHP;

    public GameObject dropPrefab;
    public int dropAmount = 1;

    private Material material;

    void Start()
    {
        currentHP = maxHP;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        material = sr.material;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        float progress = 1f - (currentHP / (float)maxHP);

        material.SetFloat("_MiningProgress", progress);

        if (currentHP <= 0)
        {
            Mine();
        }
    }

    void Mine()
    {
        for (int i = 0; i < dropAmount; i++)
            Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
