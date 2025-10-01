using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimatorLink : MonoBehaviour
{
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
}
