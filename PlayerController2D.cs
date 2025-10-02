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
    [Tooltip("Punto d'attacco frontale (facoltativo se usi solo alto/basso).")]
    [SerializeField] private Transform attackPointFront;
    [Tooltip("Punto d'attacco verso l'alto.")]
    [SerializeField] private Transform attackPointUp;
    [Tooltip("Punto d'attacco verso il basso.")]
    [SerializeField] private Transform attackPointDown;
    [SerializeField] private float attackRadius = 0.35f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private int attackDamage = 1;

    [Header("Down Attack Bounce")]
    [Tooltip("Velocit� verticale impostata quando l'attacco in basso va a segno.")]
    [SerializeField] private float downAttackBounceVelocity = 12f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private float lastAttackTime = -999f;

    private PlayerAnimatorLink animLink;

    private enum AttackDir { Front, Up, Down }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animLink = GetComponent<PlayerAnimatorLink>();
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
                TryAttack(AttackDir.Down);
            }
            else
            {
                // facoltativo: attacco frontale
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
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        // Trigger animazioni
        if (animLink != null)
        {
            switch (dir)
            {
                case AttackDir.Up: animLink.PlayAttackUp(); break;
                case AttackDir.Down: animLink.PlayAttackDown(); break;
                default: animLink.PlayAttack(); break;
            }
        }

        // Esegui effettivamente l'attacco
        bool hitSomething = DoAttack(dir);

        // Se attacco in basso ha colpito, rimbalzo
        if (dir == AttackDir.Down && hitSomething)
        {
            BounceOnDownAttack();
        }
    }

    private bool DoAttack(AttackDir dir)
    {
        Transform p = attackPointFront;

        switch (dir)
        {
            case AttackDir.Up: p = attackPointUp; break;
            case AttackDir.Down: p = attackPointDown; break;
            case AttackDir.Front:
                // Se non configurato il punto frontale, usa avanti rispetto alla scala
                if (attackPointFront == null)
                {
                    // fallback: piccolo offset davanti al player
                    Vector3 off = new Vector3(facingRight ? 0.35f : -0.35f, 0f, 0f);
                    return DamageAtPosition(transform.position + off);
                }
                break;
        }

        if (p == null) return false;
        return DamageAtPosition(p.position);
    }

    private bool DamageAtPosition(Vector2 pos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, attackRadius, enemyLayer);
        bool hitSomething = false;

        foreach (var h in hits)
        {
            hitSomething = true;
            // Invoca un eventuale metodo TakeDamage(int) sul nemico (non obbligatorio)
            h.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        return hitSomething;
    }

    private void BounceOnDownAttack()
    {
        // Imposta la velocit� verticale per simulare il rimbalzo
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, downAttackBounceVelocity);
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
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.cyan;
        if (attackPointFront != null)
            Gizmos.DrawWireSphere(attackPointFront.position, attackRadius);
        if (attackPointUp != null)
            Gizmos.DrawWireSphere(attackPointUp.position, attackRadius);
        if (attackPointDown != null)
            Gizmos.DrawWireSphere(attackPointDown.position, attackRadius);
    }
}
