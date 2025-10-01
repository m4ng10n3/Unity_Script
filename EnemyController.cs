using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController2D : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }

    [System.Serializable]
    private class EnemyAttackInfo
    {
        public float range = 1.2f;
        public float hitboxRadius = 0.6f;
        public float cooldown = 0.8f;
        public int damage = 1;
        public string trigger = "attackClose";
        [HideInInspector] public float lastAttackTime = -999f;
    }

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

    [Header("Attack Types")]
    [SerializeField] private EnemyAttackInfo[] attacks = new EnemyAttackInfo[]
    {
        new EnemyAttackInfo { range = 1.2f, hitboxRadius = 0.6f, cooldown = 0.8f, damage = 1, trigger = "attackClose" },
        new EnemyAttackInfo { range = 2.5f, hitboxRadius = 0.8f, cooldown = 1.2f, damage = 1, trigger = "attackMid" },
        new EnemyAttackInfo { range = 4f, hitboxRadius = 1f, cooldown = 2f, damage = 2, trigger = "attackJump" }
    };

    [SerializeField] private float chaseMemoryTime = 3f;

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Links")]
    [SerializeField] private EnemyAnimatorLink animLink;

    private Rigidbody2D rb;
    private Transform player;
    private State state = State.Patrol;
    private bool facingRight = true;
    private bool waiting;

    // Waypoint positions "fisse" (world space), prese all'avvio
    private Vector2 patrolA, patrolB;
    private Vector2 currentTarget;

    private float lastSeenTimer = 0f;
    private EnemyAttackInfo currentAttack;

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

            // memoria del player: reset timer se dentro aggroRange, altrimenti incrementa
            if (distToPlayer <= aggroRange)
            {
                lastSeenTimer = 0f;
            }
            else
            {
                lastSeenTimer += Time.deltaTime;
            }

            // scegli attacco in base alla distanza
            currentAttack = ChooseAttackByDistance(distToPlayer);

            // Determina stato
            if (currentAttack != null && distToPlayer <= currentAttack.range)
            {
                state = State.Attack;
            }
            else if (distToPlayer <= aggroRange || lastSeenTimer < chaseMemoryTime)
            {
                state = State.Chase;
            }
            else
            {
                state = State.Patrol;
            }
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

    private EnemyAttackInfo ChooseAttackByDistance(float distance)
    {
        EnemyAttackInfo best = null;
        float smallestRange = float.MaxValue;
        foreach (var atk in attacks)
        {
            if (distance <= atk.range && atk.range < smallestRange)
            {
                best = atk;
                smallestRange = atk.range;
            }
        }
        return best;
    }

    private void Attack()
    {
        if (currentAttack == null)
        {
            state = State.Chase; // fallback
            return;
        }

        // fermo orizzontalmente mentre attacco
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Controlla il cooldown specifico dell'attacco
        if (Time.time - currentAttack.lastAttackTime < currentAttack.cooldown) return;

        currentAttack.lastAttackTime = Time.time;

        // riproduci l'animazione corretta tramite animLink
        if (animLink != null)
        {
            switch (currentAttack.trigger)
            {
                case "attackClose":
                    animLink.PlayAttackClose();
                    break;
                case "attackMid":
                    animLink.PlayAttackMid();
                    break;
                case "attackJump":
                    animLink.PlayAttackJump();
                    break;
                default:
                    animLink.PlayAttack();
                    break;
            }
        }
    }

    // Chiamato dagli Animation Events per infliggere il danno
    public void OpenDamageWindow()
    {
        if (currentAttack == null) return;

        float radius = currentAttack.hitboxRadius;
        Vector3 pos = (attackPoint != null) ? attackPoint.position : transform.position;
        Collider2D hit = Physics2D.OverlapCircle(pos, radius, playerLayer);
        if (hit != null)
        {
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(currentAttack.damage, pos, Vector2.zero);
        }
    }

    public void CloseDamageWindow()
    {
        // Placeholder for extended logic
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
        // Draw aggro range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Draw ranges for each attack using colors
        if (attacks != null)
        {
            foreach (var atk in attacks)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, atk.range);
            }
        }

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
            // draw max hitbox radius of current attacks
            float maxRadius = 0f;
            foreach (var atk in attacks)
            {
                if (atk.hitboxRadius > maxRadius) maxRadius = atk.hitboxRadius;
            }
            Gizmos.DrawWireSphere(attackPoint.position, maxRadius);
        }
    }
}
