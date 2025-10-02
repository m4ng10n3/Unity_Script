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

    // Chiamate da PlayerController2D a seconda della direzione d'attacco
    public void PlayAttack() => anim.SetTrigger(HashAttack);
    public void PlayAttackUp() => anim.SetTrigger(HashAttackUp);
    public void PlayAttackDown() => anim.SetTrigger(HashAttackDown);

    // Chiamala quando il player muore (Health -> OnDeath)
    public void PlayDie() => anim.SetTrigger(HashDie);

    public void AE_AttackFront()
    {
        var ctrl = GetComponent<PlayerController2D>();
        if (ctrl != null)
        {
            // Chiama direttamente un attacco front (senza cooldown; gestiscilo in anim se serve)
            var m = ctrl.GetType().GetMethod("AE_AttackFront", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (m != null) m.Invoke(ctrl, null);
        }
    }

    public void AE_AttackUp()
    {
        var ctrl = GetComponent<PlayerController2D>();
        if (ctrl != null)
        {
            var m = ctrl.GetType().GetMethod("AE_AttackUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (m != null) m.Invoke(ctrl, null);
        }
    }

    public void AE_AttackDown()
    {
        var ctrl = GetComponent<PlayerController2D>();
        if (ctrl != null)
        {
            var m = ctrl.GetType().GetMethod("AE_AttackDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (m != null) m.Invoke(ctrl, null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
