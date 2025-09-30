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

    // Animator parameter hashes (veloci e sicuri contro refusi)
    private static readonly int HashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashVSpeed = Animator.StringToHash("vSpeed");
    private static readonly int HashAttack = Animator.StringToHash("attack");
    private static readonly int HashDie = Animator.StringToHash("die");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (groundCheck == null)
        {
            Debug.LogWarning("[PlayerAnimatorLink] groundCheck non assegnato: il check del suolo non funzioner�.");
        }
    }

    private void Update()
    {
        // Grounded
        bool isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        anim.SetBool(HashIsGrounded, isGrounded);

        // Velocit� orizzontale e verticale
        Vector2 v = rb.linearVelocity; // ?? in 2D � .velocity, non .linearVelocity
        anim.SetFloat(HashSpeed, Mathf.Abs(v.x));
        anim.SetFloat(HashVSpeed, v.y);
        // NB: in Animator useremo vSpeed>threshold e vSpeed<threshold per Jump/Fall
    }

    // Chiamala dal codice quando esegui l�attacco (Input)
    public void PlayAttack() => anim.SetTrigger(HashAttack);

    // Chiamala quando il player muore (Health -> OnDeath)
    public void PlayDie() => anim.SetTrigger(HashDie);

    // ---- Opzionale: Animation Events nella clip di attacco ----
    // Dentro la clip Attack puoi creare un Event chiamando questi metodi:

    // Per aprire/chiudere una finestra di hitbox legata all�animazione
    public void AE_StartAttackWindow()
    {
        var sword = GetComponentInChildren<SwordAttack>();
        var facingRight = transform.localScale.x >= 0f;
        // if (sword) sword.DoAttack(facingRight);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
