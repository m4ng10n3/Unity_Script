using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public enum AttackType { Front, Up, Down }

    [SerializeField] private int damage = 1;

    [Header("Attack Window")]
    [SerializeField] private float attackDuration = 0.15f;

    [Header("Hitbox - Front")]
    [SerializeField] private Vector2 hitboxOffsetRight = new Vector2(0.6f, 0f);
    [SerializeField] private Vector2 hitboxSize = new Vector2(0.8f, 0.5f);

    [Header("Hitbox - Up")]
    [SerializeField] private Vector2 hitboxOffsetUp = new Vector2(0f, 0.9f);
    [SerializeField] private Vector2 hitboxSizeUp = new Vector2(0.7f, 0.7f);

    [Header("Hitbox - Down")]
    [SerializeField] private Vector2 hitboxOffsetDown = new Vector2(0f, -0.9f);
    [SerializeField] private Vector2 hitboxSizeDown = new Vector2(0.7f, 0.7f);

    [SerializeField] private LayerMask targets; // layer dei bersagli (Enemy)

    // Notifica quando un colpo va a segno (usato per il rimbalzo del down-attack)
    public event Action<AttackType> OnHit;

    private bool attacking;
    private readonly Collider2D[] results = new Collider2D[8];
    private readonly HashSet<Collider2D> damagedColliders = new HashSet<Collider2D>();

    public bool IsAttacking => attacking;

    // Avvio tramite Animation Event
    public void StartAttackWindow(bool facingRight, AttackType type)
    {
        if (!attacking) StartCoroutine(AttackWindow(facingRight, type));
    }

    public void EndAttackWindow() { } // disponibile se vuoi chiuderla da AE

    private IEnumerator AttackWindow(bool facingRight, AttackType type)
    {
        attacking = true;
        float end = Time.time + attackDuration;
        damagedColliders.Clear();

        bool notifiedHit = false;

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
                if (col == null || damagedColliders.Contains(col)) continue;

                // Colpisci Health con firma a 3 parametri
                var health = col.GetComponentInParent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, (Vector2)col.transform.position, Vector2.zero);
                    damagedColliders.Add(col);

                    if (!notifiedHit)
                    {
                        notifiedHit = true;
                        OnHit?.Invoke(type);
                    }
                }
            }

            yield return null;
        }

        attacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Front
        Vector2 r = (Vector2)transform.position + hitboxOffsetRight;
        Vector2 l = (Vector2)transform.position + new Vector2(-hitboxOffsetRight.x, hitboxOffsetRight.y);
        Gizmos.DrawWireCube(r, hitboxSize);
        Gizmos.DrawWireCube(l, hitboxSize);

        // Up
        Gizmos.DrawWireCube((Vector2)transform.position + hitboxOffsetUp, hitboxSizeUp);

        // Down
        Gizmos.DrawWireCube((Vector2)transform.position + hitboxOffsetDown, hitboxSizeDown);
    }
}
