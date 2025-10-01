using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimatorLink : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private static readonly int HashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashVSpeed = Animator.StringToHash("vSpeed");

    private Animator anim;
    private Rigidbody2D rb;
    private EnemyController2D controller;



    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<EnemyController2D>();
    }

    private void Update()
    {
        anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));

        Vector2 v = rb.linearVelocity; 
        anim.SetFloat(HashSpeed, Mathf.Abs(v.x));
        anim.SetFloat(HashVSpeed, v.y);

        // Grounded
        bool isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        anim.SetBool(HashIsGrounded, isGrounded);
    }

    public void PlayAttack() => anim.SetTrigger("attack");
    public void PlayDie() => anim.SetTrigger("die");

    // New attack triggers for multiple attack types
    public void PlayAttackClose() => anim.SetTrigger("attackClose");
    public void PlayAttackMid() => anim.SetTrigger("attackMid");
    public void PlayAttackJump() => anim.SetTrigger("attackJump");

    // Animation events to control damage window
    public void AE_OpenDamageWindow()
    {
        if (controller != null)
        {
            controller.OpenDamageWindow();
        }
    }

    public void AE_CloseDamageWindow()
    {
        if (controller != null)
        {
            controller.CloseDamageWindow();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}
