using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public enum AttackType { Front, Up, Down }

    [SerializeField] private int damage = 1;

    [Header("Attack Window")]
    [SerializeField] private float attackDuration = 1.1f;

    [Header("Hitbox - Front")]
    [Tooltip("Offset orizzontale della hitbox (destra). A sinistra viene specchiato.")]
    [SerializeField] private Vector2 hitboxOffsetRight = new Vector2(0.6f, 0f);
    [SerializeField] private Vector2 hitboxSize = new Vector2(0.8f, 0.5f);

    [Header("Hitbox - Up/Down")]
    [SerializeField] private Vector2 hitboxOffsetUp = new Vector2(0f, 0.9f);
    [SerializeField] private Vector2 hitboxSizeUp = new Vector2(0.7f, 0.7f);
    [SerializeField] private Vector2 hitboxOffsetDown = new Vector2(0f, -0.9f);
    [SerializeField] private Vector2 hitboxSizeDown = new Vector2(0.7f, 0.7f);

    [SerializeField] private LayerMask targets; // Imposta su layer dei nemici

    private bool attacking;
    private readonly Collider2D[] results = new Collider2D[8];
    private readonly HashSet<object> damagedTargets = new HashSet<object>();

    public bool IsAttacking => attacking;

    // Backward-compat: default Front
    public void DoAttack(bool facingRight)
    {
        if (!attacking) StartCoroutine(AttackWindow(facingRight, AttackType.Front));
    }

    public void DoAttack(bool facingRight, AttackType type)
    {
        if (!attacking) StartCoroutine(AttackWindow(facingRight, type));
    }

    private IEnumerator AttackWindow(bool facingRight, AttackType type)
    {
        attacking = true;
        float end = Time.time + attackDuration;
        damagedTargets.Clear();

        while (Time.time < end)
        {
            Vector2 center;
            Vector2 size;

            switch (type)
            {
                case AttackType.Up:
                    center = (Vector2)transform.position + hitboxOffsetUp;
                    size = hitboxSizeUp;
                    break;

                case AttackType.Down:
                    center = (Vector2)transform.position + hitboxOffsetDown;
                    size = hitboxSizeDown;
                    break;

                default: // Front
                    Vector2 offset = facingRight
                        ? hitboxOffsetRight
                        : new Vector2(-hitboxOffsetRight.x, hitboxOffsetRight.y);
                    center = (Vector2)transform.position + offset;
                    size = hitboxSize;
                    break;
            }

            int count = Physics2D.OverlapBoxNonAlloc(center, size, 0f, results, targets);
            for (int i = 0; i < count; i++)
            {
                var col = results[i];
                if (col == null || damagedTargets.Contains(col)) continue;

                // Usa IDamageable (Health lo implementa) e fai il cast a Vector2
                var receiver = col.GetComponentInParent<IDamageable>();
                if (receiver != null)
                {
                    receiver.TakeDamage(damage, (Vector2)col.transform.position, Vector2.zero);
                    damagedTargets.Add(col);
                }
            }

            yield return null;
        }

        attacking = false;
    }

    // AnimationEvents compat
    public void StartAttackWindow(bool facingRight, AttackType type)
    {
        if (!attacking) StartCoroutine(AttackWindow(facingRight, type));
    }

    public void EndAttackWindow() { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Front
        Vector2 r = (Vector2)transform.position + hitboxOffsetRight;
        Vector2 l = (Vector2)transform.position + new Vector2(-hitboxOffsetRight.x, hitboxOffsetRight.y);
        Gizmos.DrawWireCube(r, hitboxSize);
        Gizmos.DrawWireCube(l, hitboxSize);

        // Up / Down
        Gizmos.DrawWireCube((Vector2)transform.position + hitboxOffsetUp, hitboxSizeUp);
        Gizmos.DrawWireCube((Vector2)transform.position + hitboxOffsetDown, hitboxSizeDown);
    }
}
