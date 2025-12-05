using UnityEngine;

/// <summary>
/// CPU AI Controller dengan FSM (Finite State Machine)
/// States: Patrol (jelajah) dan Attack (serang player)
/// Menggunakan Dijkstra/A* untuk pathfinding
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CPUController : MonoBehaviour
{
    #region FSM State

    public enum CPUState
    {
        Patrol,     // Jelajah area, cari musuh
        Attack      // Kejar dan serang musuh terdekat
    }

    private CPUState currentState = CPUState.Patrol;
    public CPUState CurrentState => currentState;

    #endregion

    #region Inspector Fields

    [Header("CPU INFO")]
    public string cpuName = "CPU 1";
    public PathfindingType pathfindingType = PathfindingType.Dijkstra;

    [Header("MOVEMENT SETTINGS")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 2f;
    [Range(0.90f, 0.999f)] public float friction = 0.98f;

    [Header("PATROL SETTINGS")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    public float patrolPointReachDistance = 1f;

    [Header("ATTACK SETTINGS")]
    public float detectionRange = 12f;
    public float attackRange = 5f;
    public float loseTargetRange = 15f;

    [Header("TARGET SETTINGS")]
    public LayerMask playerLayer;
    public Transform[] playerTargets; // Assign Player 1 & Player 2

    [Header("DEBUG")]
    public bool showDebugGizmos = true;

    #endregion

    #region Private Variables

    private Rigidbody2D rb;
    private Vector2 patrolTarget;
    private float patrolWaitTimer;
    private Transform currentTarget;
    private Vector2 moveDirection;
    private CPUHealthBar healthBar;

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        healthBar = GetComponent<CPUHealthBar>();
    }

    void Start()
    {
        // Generate first patrol point
        GenerateNewPatrolPoint();
    }

    void Update()
    {
        UpdateFSM();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    #endregion

    #region FSM Logic

    void UpdateFSM()
    {
        switch (currentState)
        {
            case CPUState.Patrol:
                PatrolBehavior();
                CheckForEnemies();
                break;

            case CPUState.Attack:
                AttackBehavior();
                CheckTargetValidity();
                break;
        }
    }

    void PatrolBehavior()
    {
        // Cek jarak ke patrol target
        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);

        if (distanceToTarget <= patrolPointReachDistance)
        {
            // Sampai di patrol point, tunggu sebentar
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                GenerateNewPatrolPoint();
                patrolWaitTimer = 0f;
            }

            moveDirection = Vector2.zero;
        }
        else
        {
            // Gerak menuju patrol point
            moveDirection = ((Vector2)patrolTarget - (Vector2)transform.position).normalized;
        }
    }

    void AttackBehavior()
    {
        if (currentTarget == null)
        {
            TransitionToPatrol();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        // Kalau target terlalu jauh, balik ke patrol
        if (distanceToTarget > loseTargetRange)
        {
            TransitionToPatrol();
            return;
        }

        // Kejar target
        moveDirection = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;

        // TODO: Integrate dengan CPUShootingSystem kalau dalam attack range
        if (distanceToTarget <= attackRange)
        {
            // Dalam range tembak, shooting system handle ini
        }
    }

    void CheckForEnemies()
    {
        // Cari player terdekat dalam detection range
        Transform nearestPlayer = FindNearestPlayer();

        if (nearestPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, nearestPlayer.position);

            if (distance <= detectionRange)
            {
                // Ketemu musuh, switch ke Attack
                currentTarget = nearestPlayer;
                TransitionToAttack();
            }
        }
    }

    void CheckTargetValidity()
    {
        if (currentTarget == null)
        {
            TransitionToPatrol();
            return;
        }

        // Cek apakah target masih hidup
        PlayerHealth targetHealth = currentTarget.GetComponent<PlayerHealth>();
        if (targetHealth != null && !targetHealth.IsAlive())
        {
            Debug.Log($"[{cpuName}] Target dead, returning to patrol");
            TransitionToPatrol();
        }
    }

    #endregion

    #region State Transitions

    void TransitionToPatrol()
    {
        currentState = CPUState.Patrol;
        currentTarget = null;
        GenerateNewPatrolPoint();
        Debug.Log($"[{cpuName}] → PATROL");
    }

    void TransitionToAttack()
    {
        currentState = CPUState.Attack;
        patrolWaitTimer = 0f;
        Debug.Log($"[{cpuName}] → ATTACK (Target: {currentTarget.name})");
    }

    #endregion

    #region Movement

    void ApplyMovement()
    {
        if (moveDirection.magnitude > 0.01f)
        {
            // Rotate towards movement direction
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.rotation = newAngle;

            // Move forward
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            // Apply friction when idle
            rb.linearVelocity *= friction;
        }
    }

    #endregion

    #region Helpers

    void GenerateNewPatrolPoint()
    {
        // Random point dalam patrol radius
        Vector2 randomDirection = Random.insideUnitCircle;
        patrolTarget = (Vector2)transform.position + (randomDirection * patrolRadius);

        Debug.Log($"[{cpuName}] New patrol point: {patrolTarget}");
    }

    Transform FindNearestPlayer()
    {
        if (playerTargets == null || playerTargets.Length == 0)
            return null;

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Transform player in playerTargets)
        {
            if (player == null) continue;

            // Cek apakah player masih hidup
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsAlive())
                continue;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = player;
            }
        }

        return nearest;
    }

    #endregion

    #region Public Getters

    public string GetStateString()
    {
        return currentState.ToString().ToUpper();
    }

    public Color GetStateColor()
    {
        switch (currentState)
        {
            case CPUState.Patrol: return Color.cyan;
            case CPUState.Attack: return Color.red;
            default: return Color.white;
        }
    }

    #endregion

    #region Debug Gizmos

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Patrol radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Current patrol target
        if (currentState == CPUState.Patrol)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, patrolTarget);
            Gizmos.DrawWireSphere(patrolTarget, patrolPointReachDistance);
        }

        // Current attack target
        if (currentState == CPUState.Attack && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    #endregion
}

/// <summary>
/// Enum untuk tipe pathfinding yang dipakai CPU
/// </summary>
public enum PathfindingType
{
    Dijkstra,   // Dijkstra's Algorithm
    AStar       // A* Algorithm
}
