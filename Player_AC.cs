using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimatorLink : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Air State Thresholds")]
    [Tooltip("Soglia minima per considerare 'salita' (JumpUp)")]
    [SerializeField] private float vSpeedUpThreshold = 0.1f;
    [Tooltip("Soglia massima (negativa) per considerare 'caduta' (Fall)")]
    [SerializeField] private float vSpeedDownThreshold = -0.1f;

    private Animator anim;
    private Rigidbody2D rb;

    // Parametri / trigger (adatta ai tuoi Animator Controller)
    private static readonly int HashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashVSpeed = Animator.StringToHash("vSpeed");
    private static readonly int HashAttack = Animator.StringToHash("attack");
    private static readonly int HashAttackUp = Animator.StringToHash("attackUp");
    private static readonly int HashAttackDown = Animator.StringToHash("attackDown");
    private static readonly int HashDie = Animator.StringToHash("die");

    // Memorizza l'ultimo tipo di attacco per l'Animation Event
    private SwordAttack.AttackType lastAttackType = SwordAttack.AttackType.Front;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (groundCheck == null)
            Debug.LogWarning("[PlayerAnimatorLink] groundCheck non assegnato.");
    }

    private void Update()
    {
        // Grounded
        bool grounded = false;
        if (groundCheck != null)
            grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool(HashIsGrounded, grounded);

        // Velocità
        Vector2 v = rb.linearVelocity;
        anim.SetFloat(HashSpeed, Mathf.Abs(v.x));
        anim.SetFloat(HashVSpeed, v.y);
    }

    // Chiamate dal PlayerController2D — memorizzano il tipo e settano il trigger
    public void PlayAttack()
    {
        lastAttackType = SwordAttack.AttackType.Front;
        anim.SetTrigger(HashAttack);
    }

    public void PlayAttackUp()
    {
        lastAttackType = SwordAttack.AttackType.Up;
        anim.SetTrigger(HashAttackUp);
    }

    public void PlayAttackDown()
    {
        lastAttackType = SwordAttack.AttackType.Down;
        anim.SetTrigger(HashAttackDown);
    }

    // Chiamala quando il player muore (Health -> OnDeath)
    public void PlayDie() => anim.SetTrigger(HashDie);

    // === ANIMATION EVENT ===
    // Aggiungi questo nome di evento nel frame d’impatto delle clip di attacco:
    // AE_StartAttackWindow
    public void AE_StartAttackWindow()
    {
        var sword = GetComponentInChildren<SwordAttack>();
        if (sword == null) return;

        bool facingRight = transform.localScale.x >= 0f;
        sword.StartAttackWindow(facingRight, lastAttackType);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
