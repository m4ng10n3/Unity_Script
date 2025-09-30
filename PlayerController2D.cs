using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private SwordAttack sword;
    [SerializeField] private float attackCooldown = 0.3f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private float lastAttackTime = -999f;
    private PlayerAnimatorLink animLink;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animLink = GetComponent<PlayerAnimatorLink>();
    }

    private void Update()
    {
        // CHANGE: Lock movement and input while attacking
        if (sword != null && sword.IsAttacking)
        {
            moveInput = 0f;
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            TryAttack();
        }

        FlipIfNeeded(moveInput);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // CHANGE: Freeze horizontal velocity while attacking
        if (sword != null && sword.IsAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void TryAttack()
    {
        // CHANGE: Prevent new attack while current attack is active
        if (sword != null && sword.IsAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        if (animLink != null) animLink.PlayAttack();
        // Hitbox triggered via AnimationEvent
        // sword.DoAttack(facingRight);
    }

    private void FlipIfNeeded(float input)
    {
        if (input > 0 && !facingRight) Flip();
        else if (input < 0 && facingRight) Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        var scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
