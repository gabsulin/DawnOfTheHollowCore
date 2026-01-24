using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public static PlayerController Instance { get; private set; }
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;

    [Header("State")]
    public bool canMove = true;
    public bool canAttack = true;
    public bool isAttacking = false;

    public Vector2 input;
    private Vector2 lastMovementDirection = Vector2.right;
    private Vector2 currentVelocity;

    private Camera mainCamera;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        if (canMove)
        {
            HandleInput();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TriggerDeath();
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            ApplyMovement();
        }
    }

    private void HandleInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.magnitude > 1f)
        {
            input.Normalize();
        }

        if (input.magnitude > 0.01f)
        {
            lastMovementDirection = input;
            animator.SetBool(IsMovingHash, true);
        }
        else
        {
            animator.SetBool(IsMovingHash, false);
        }

        FlipCharacter();
    }

    private void ApplyMovement()
    {
        if (input.magnitude > 0.01f)
        {
            currentVelocity = input * moveSpeed;
            rb.linearVelocity = currentVelocity;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FlipCharacter()
    {
        if (mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        float direction = mouseWorldPos.x - transform.position.x;

        if (Mathf.Abs(direction) > 0.05f)
        {
            transform.localScale = new Vector3(
                direction > 0 ? 1 : -1,
                1,
                1
            );
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    public void TriggerDeath()
    {
        animator.SetBool(IsDeadHash, true);
        canMove = false;
        canAttack = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void ApplyMoveSpeedUpgrade(float amount)
    {
        moveSpeed = Mathf.Max(0f, moveSpeed + amount);
    }

    public Vector2 GetLastMovementDirection()
    {
        return lastMovementDirection;
    }

    public Vector2 GetCurrentInput()
    {
        return input;
    }
}