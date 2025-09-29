using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController2D : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }

    [Header("Waypoints (posizionane due in scena)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Refs")]
    [SerializeField] private Transform attackPoint; // un Empty davanti al nemico

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitAtPoint = 0.8f;

    [Header("Behavior")]
    [SerializeField] private float aggroRange = 6f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private int damage = 1;

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Links")]
    [SerializeField] private EnemyAnimatorLink animLink;



    private Rigidbody2D rb;
    private Transform player;
    private State state = State.Patrol;
    private bool facingRight = true;
    private bool waiting;
    private float lastAttackTime = -999f;

    // Waypoint positions "fisse" (world space), prese all'avvio
    private Vector2 patrolA, patrolB;
    private Vector2 currentTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Trova il player via Tag (assicurati che il Player abbia Tag = "Player")
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;

        // Salva le POSIZIONI all'avvio, così anche se A/B sono figli non "si muovono" con noi
        if (pointA != null) patrolA = pointA.position; else patrolA = transform.position + Vector3.left * 2f;
        if (pointB != null) patrolB = pointB.position; else patrolB = transform.position + Vector3.right * 2f;

        currentTarget = patrolA; // parte verso A
    }

    private void Update()
    {
        if (player != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);

            if (distToPlayer <= attackRange)
                state = State.Attack;
            else if (distToPlayer <= aggroRange)
                state = State.Chase;
            else
                state = State.Patrol;
        }
        else
        {
            state = State.Patrol;
        }

        HandleFlip();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    private void Patrol()
    {
        if (waiting) return;

        // Muovi verso il target corrente
        float dir = Mathf.Sign(currentTarget.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        // Quando sei vicino al target, fermati un attimo e poi alterna
        if (Mathf.Abs(currentTarget.x - transform.position.x) < 0.05f)
        {
            StartCoroutine(WaitAndSwapPoint());
        }
    }

    private IEnumerator WaitAndSwapPoint()
    {
        waiting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(waitAtPoint);
        currentTarget = (currentTarget == patrolA) ? patrolB : patrolA;
        waiting = false;
    }

    private void Chase()
    {
        if (player == null) return;
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    private void Attack()
    {
        // fermo orizzontalmente mentre attacco
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        if (animLink != null) animLink.PlayAttack();

        if (attackPoint == null) attackPoint = transform;

        // Colpisci il player se nel raggio
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, 0.6f, playerLayer);
        if (hit != null)
        {
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage, attackPoint.position, Vector2.zero);
        }
    }

    private void HandleFlip()
    {
        float targetX;

        if (state == State.Chase && player != null)
        {
            targetX = player.position.x;
        }
        else if (state == State.Patrol)
        {
            targetX = currentTarget.x;
        }
        else
        {
            return;
        }

        bool shouldFaceRight = targetX >= transform.position.x;
        if (shouldFaceRight != facingRight) Flip();
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Disegna i waypoint salvati (se in Play) oppure i Transform (in Edit)
        Vector3 a = (Application.isPlaying || pointA == null) ? (Vector3)patrolA : pointA.position;
        Vector3 b = (Application.isPlaying || pointB == null) ? (Vector3)patrolB : pointB.position;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(a, 0.08f);
        Gizmos.DrawSphere(b, 0.08f);
        Gizmos.DrawLine(a, b);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, 0.6f);
        }
    }
}
