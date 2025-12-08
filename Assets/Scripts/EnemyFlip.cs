using UnityEngine;

public class EnemyFlip : MonoBehaviour
{
    Transform player;

    private bool isFlipped = false;

    [SerializeField]
    private bool startsFlipped = false;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>().transform;
        isFlipped = startsFlipped;
        LookAtPlayer();
    }

    public void LookAtPlayer()
    {
        if (player != null)
        {
            if (transform.position.x > player.position.x && !isFacingLeft())
            {
                Flip();
            }
            else if (transform.position.x < player.position.x && isFacingLeft())
            {
                Flip();
            }
        }

    }

    private bool isFacingLeft()
    {
        return isFlipped;
    }

    private void Flip()
    {
        Vector3 flippedScale = transform.localScale;
        flippedScale.x *= -1;
        transform.localScale = flippedScale;

        isFlipped = !isFlipped;
    }
}