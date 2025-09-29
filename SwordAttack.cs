using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackDuration = 0.12f;

    [Tooltip("Offset orizzontale della hitbox (destra). A sinistra viene specchiato.")]
    [SerializeField] private Vector2 hitboxOffsetRight = new Vector2(0.6f, 0f);

    [SerializeField] private Vector2 hitboxSize = new Vector2(0.8f, 0.5f);
    [SerializeField] private LayerMask targets; // setta su Enemy nel Inspector

    private bool attacking;
    private readonly Collider2D[] results = new Collider2D[8];

    public void DoAttack(bool facingRight)
    {
        if (!attacking) StartCoroutine(AttackWindow(facingRight));
    }

    private IEnumerator AttackWindow(bool facingRight)
    {
        attacking = true;
        float end = Time.time + attackDuration;

        Vector2 offset = facingRight
            ? hitboxOffsetRight
            : new Vector2(-hitboxOffsetRight.x, hitboxOffsetRight.y);

        while (Time.time < end)
        {
            Vector2 center = (Vector2)transform.position + offset;
             int count = Physics2D.OverlapBoxNonAlloc(center, hitboxSize, 0f, results, targets);
            //int count = Physics2D.OverlapBox(center, hitboxSize, 0f, targets);

            for (int i = 0; i < count; i++)
            {
                var d = results[i].GetComponentInParent<IDamageable>();
                if (d != null)
                    d.TakeDamage(damage, results[i].transform.position, Vector2.zero);
            }
            yield return null;
        }

        attacking = false;
    }

    public void StartAttackWindow(bool facingRight)
    {
        if (!attacking) StartCoroutine(AttackWindow(facingRight));
    }
    public void EndAttackWindow()
    {
        // se vuoi chiuderla prima, imposta attacking=false o usa una variabile endTime
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 r = (Vector2)transform.position + hitboxOffsetRight;
        Vector2 l = (Vector2)transform.position + new Vector2(-hitboxOffsetRight.x, hitboxOffsetRight.y);
        Gizmos.DrawWireCube(r, hitboxSize);
        Gizmos.DrawWireCube(l, hitboxSize);
    }
}
