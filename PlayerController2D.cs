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
    [Tooltip("Velocità verticale impostata quando l'attacco in basso va a segno.")]
    [SerializeField] private float downAttackBounceVelocity = 12f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private float lastAttackTime = -999f;
    private PlayerAnimatorLink animLink;

    private enum LocalAttackDir { Front, Up, Down }

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
                TryAttack(LocalAttackDir.Up);
            }
            else if (v < -0.5f && attackPointDown != null)
            {
                // ⬇️ Attacco in basso solo se NON siamo grounded
                if (!isGrounded)
                    TryAttack(LocalAttackDir.Down);
            }
            else
            {
                if (attackPointFront != null)
                    TryAttack(LocalAttackDir.Front);
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

    private void TryAttack(LocalAttackDir dir)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // Trigger animazioni coerenti
        if (animLink != null)
        {
            switch (dir)
            {
                case LocalAttackDir.Up: animLink.PlayAttackUp(); break;
                case LocalAttackDir.Down: animLink.PlayAttackDown(); break; // chiamato solo se !isGrounded
                default: animLink.PlayAttack(); break;
            }
        }

        // Hit immediato (se non usi AE sulla spada)
        bool hitSomething = DoAttack(dir);

        // Rimbalzo se l’attacco in basso ha colpito
        if (dir == LocalAttackDir.Down && hitSomething)
        {
            BounceOnDownAttack();
        }
    }

    private bool DoAttack(LocalAttackDir dir)
    {
        Transform p = attackPointFront;
        switch (dir)
        {
            case LocalAttackDir.Up: p = attackPointUp; break;
            case LocalAttackDir.Down: p = attackPointDown; break;
            case LocalAttackDir.Front:
                if (attackPointFront == null)
                {
                    // fallback: piccolo offset davanti al player
                    Vector3 off = new Vector3(facingRight ? 0.35f : -0.35f, 0f, 0f);
                    return DamageAtPosition((Vector2)(transform.position + off));
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
            if (h == null) continue;
            hitSomething = true;

            // Evita SendMessage con firma errata: usa IDamageable/Health quando disponibile
            var dmg = h.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(attackDamage, (Vector2)h.transform.position, Vector2.zero);
                continue;
            }

            // Fallback: prova ad invocare un TakeDamage(int) se esiste davvero
            var comp = h.GetComponentInParent<MonoBehaviour>();
            if (comp != null)
            {
                var m1 = comp.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                if (m1 != null)
                {
                    m1.Invoke(comp, new object[] { attackDamage });
                }
                // altrimenti ignora: evitiamo l'eccezione di SendMessage quando la firma non coincide
            }
        }

        return hitSomething;
    }

    private void BounceOnDownAttack()
    {
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
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.cyan;
        if (attackPointFront != null) Gizmos.DrawWireSphere(attackPointFront.position, attackRadius);
        if (attackPointUp != null) Gizmos.DrawWireSphere(attackPointUp.position, attackRadius);
        if (attackPointDown != null) Gizmos.DrawWireSphere(attackPointDown.position, attackRadius);
    }
}
