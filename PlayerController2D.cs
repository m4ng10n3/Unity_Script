using System;
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

    [Header("Attack Points")]
    [Tooltip("Punto d'attacco frontale (facoltativo se usi solo alto/basso).")]
    [SerializeField] private Transform attackPointFront;
    [Tooltip("Punto d'attacco verso l'alto.")]
    [SerializeField] private Transform attackPointUp;
    [Tooltip("Punto d'attacco verso il basso.")]
    [SerializeField] private Transform attackPointDown;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.25f;

    [Header("Down Attack Bounce")]
    [Tooltip("Velocità verticale impostata quando l'attacco in basso va a segno.")]
    [SerializeField] private float downAttackBounceVelocity = 12f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private float lastAttackTime = -999f;

    private PlayerAnimatorLink animLink;
    private SwordAttack sword;

    private enum AttackDir { Front, Up, Down }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animLink = GetComponent<PlayerAnimatorLink>();
        sword = GetComponentInChildren<SwordAttack>();

        if (sword != null)
        {
            // Rimbalzo quando un attacco Down colpisce
            sword.OnHit += OnSwordHit;
        }
        else
        {
            Debug.LogWarning("[PlayerController2D] Nessun SwordAttack trovato come child.");
        }
    }

    private void OnDestroy()
    {
        if (sword != null) sword.OnHit -= OnSwordHit;
    }

    private void Update()
    {
        // Movimento orizzontale
        moveInput = Input.GetAxisRaw("Horizontal");

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Attacchi: Fire1 + direzione verticale
        if (Input.GetButtonDown("Fire1"))
        {
            float v = Input.GetAxisRaw("Vertical");

            if (v > 0.5f && attackPointUp != null)
            {
                TryAttack(AttackDir.Up);
            }
            else if (v < -0.5f && attackPointDown != null)
            {
                // Down attack SOLO se non siamo grounded
                if (!isGrounded)
                    TryAttack(AttackDir.Down);
            }
            else
            {
                if (attackPointFront != null)
                    TryAttack(AttackDir.Front);
            }
        }

        FlipIfNeeded(moveInput);
    }

    private void FixedUpdate()
    {
        // Grounded
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Movimento orizzontale
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void TryAttack(AttackDir dir)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // Trigger animazioni (la hitbox parte tramite Animation Event)
        if (animLink != null)
        {
            switch (dir)
            {
                case AttackDir.Up: animLink.PlayAttackUp(); break;
                case AttackDir.Down: animLink.PlayAttackDown(); break; // chiamato solo se !isGrounded
                default: animLink.PlayAttack(); break;
            }
        }
    }

    private void OnSwordHit(SwordAttack.AttackType type)
    {
        if (type == SwordAttack.AttackType.Down)
        {
            // Rimbalzo solo se l'attacco in basso ha colpito
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, downAttackBounceVelocity);
        }
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
        Gizmos.color = Color.yellow;
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Solo punti di riferimento (nessuna hitbox qui)
        //Gizmos.color = Color.cyan;
        //if (attackPointFront != null) Gizmos.DrawWireSphere(attackPointFront.position, 0.07f);
        //if (attackPointUp != null) Gizmos.DrawWireSphere(attackPointUp.position, 0.07f);
        //if (attackPointDown != null) Gizmos.DrawWireSphere(attackPointDown.position, 0.07f);
    }
}
