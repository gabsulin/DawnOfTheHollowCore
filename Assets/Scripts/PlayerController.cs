using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Rigidbody2D rb;
    private Animator animator;

    public float moveSpeed;
    public bool canMove = true;
    public bool canAttack = true;

    public bool isAttacking = false;

    public Vector2 input;
    private Vector2 lastMovementDirection = Vector2.right;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (canMove) HandleMovement();

        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetBool("IsDead", true);
            canMove = false;
        }
    }

    private void HandleMovement()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (input.magnitude > 0)
        {
            transform.position += (Vector3)(input * moveSpeed * Time.deltaTime);
            lastMovementDirection = input;
            FlipCharacter(input.x);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);

            if (!isAttacking)
            {
                FlipCharacter(lastMovementDirection.x);
            }
        }

    }


    private void FlipCharacter(float directionX)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mouseWorldPos.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (mouseWorldPos.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void ApplyMoveSpeedUpgrade(float amount)
    {
        moveSpeed += amount;
    }

}