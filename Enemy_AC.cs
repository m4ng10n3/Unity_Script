using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimatorLink : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));
    }

    public void PlayAttack() => anim.SetTrigger("attack");
    public void PlayDie() => anim.SetTrigger("die");
}
//pippo