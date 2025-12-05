using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoMovement : MonoBehaviour {
    // FSM States untuk CPU/AI
    public enum AIState {
        Idle,
        Following,
        Recalculating,
        Patrol,         // Jalan ke waypoint
        Attack          // Ngejar player
    }

    [Header("AI Info")]
    public string aiName = "CPU 1"; // "CPU 1 (A*)" atau "CPU 2 (Dijkstra)"
    
    [Header("Pathfinding Algorithm")]
    [Tooltip("PATROL = Dijkstra (semua CPU), ATTACK = A* (semua CPU)")]
    private PathfindingAlgorithm currentAlgorithm = PathfindingAlgorithm.Dijkstra;
    
    [Header("Waypoint System")]
    [Tooltip("List waypoint yang akan dilewati CPU (W1>W2>W3>W2>W1>W2...)")]
    public Transform[] waypoints; // Drag waypoints dari scene
    [Tooltip("Jarak minimum untuk menganggap waypoint tercapai")]
    public float waypointReachDistance = 0.5f;
    
    private int currentWaypointIndex = 0;
    private bool movingForward = true; // true = maju (0→n), false = mundur (n→0)
    
    [Header("Target / Path Goal (Legacy - Optional)")]
    [Tooltip("Kosongkan jika menggunakan waypoint system")]
    public Transform target;
    public float repathInterval = 1.0f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float waypointReachDist = 0.1f;

    [Header("Facing")]
    public float facingOffsetDeg = 0f;

    [Header("Combat Settings - Radius Zones")]
    [Tooltip("[PATROL] Radius patrol area (waypoint exploration)")]
    public float patrolRadius = 10f;
    
    [Tooltip("[DETECT] Radius detect player (patrol → attack transition)")]
    public float detectionRange = 3f;
    
    [Tooltip("[FOLLOW] Radius follow/chase player (saat attack mode)")]
    public float followRange = 5f;
    
    [Tooltip("[ATTACK] Radius tembak player (shooting range)")]
    public float attackRange = 3f;
    
    [Tooltip("[STOP] Jarak berhenti dari player (stop distance)")]
    public float stopDistance = 2f;
    
    [Tooltip("[LOSE] Radius lose target (attack → patrol transition)")]
    public float loseTargetRange = 5f;
    
    [Header("Combat Targets")]
    [Tooltip("Layer untuk detect player")]
    public LayerMask playerLayer;
    [Tooltip("Drag Player 1 & Player 2 GameObject")]
    public Transform[] playerTargets;
    
    [Header("Cooperative FSM")]
    [Tooltip("Partner CPU (CPU 1 <-> CPU 2 untuk koordinasi)")]
    public AutoMovement partnerCPU;
    
    // Static cooperative target - shared antar CPU
    private static Transform cooperativeTarget = null;
    private static AutoMovement initiatorCPU = null;
    
    // Flag: apakah CPU ini sedang follow partner (bukan initiator)
    private bool isCooperativeFollower = false;

    [Header("Grid (untuk label progress)")]
    public Grid grid; // drag; atau auto find

    Rigidbody2D rb;
    Vector3[] path; int pathIndex; float nextRepathTime;
    Vector2 currentMoveDir;

    // FSM State
    private AIState currentState = AIState.Idle;
    public AIState CurrentState => currentState;
    
    // Combat variables
    private Transform currentTarget;
    private Vector3 lastKnownWaypointTarget; // Simpan waypoint terakhir sebelum attack
    private float nextTargetRepathTime = 0f; // Timer untuk repath ke target (A* untuk obstacle avoidance)

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale=0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        if(grid==null) grid = FindFirstObjectByType<Grid>();
    }

    void Start(){ 
        // Jika waypoint system aktif, langsung mulai PATROL
        if(waypoints != null && waypoints.Length > 0) {
            currentState = AIState.Patrol;
            currentWaypointIndex = 0;
            movingForward = true;
            RequestPathToCurrentWaypoint();
        }
        // Jika legacy target system
        else if(target != null) {
            currentState = AIState.Recalculating;
            RequestNewPath(); 
            nextRepathTime = Time.time + repathInterval;
        }
        else {
            Debug.LogWarning($"[{aiName}] Tidak ada waypoints atau target yang diset!", this);
            currentState = AIState.Idle;
        }
    }
    
    void Update(){
        // COMBAT FSM: Cek player detection (hanya saat PATROL)
        if(currentState == AIState.Patrol) {
            CheckForEnemies();
        }
        // Validasi target (saat FOLLOWING atau ATTACK)
        else if(currentState == AIState.Following || currentState == AIState.Attack) {
            CheckTargetValidity();
            
            // FOLLOWING mode: Request path berkala untuk hindari obstacle dengan A*
            if(currentState == AIState.Following && Time.time >= nextTargetRepathTime) {
                if(currentTarget != null) {
                    PathRequestManager.RequestPath(transform.position, currentTarget.position, OnPathFound, PathfindingAlgorithm.AStar);
                    nextTargetRepathTime = Time.time + 0.5f; // Repath setiap 0.5 detik
                    Debug.Log($"[{aiName}] [FOLLOWING] Repathfinding with A* to avoid obstacles");
                }
            }
        }
        
        // Waypoint system - cek apakah sudah sampai waypoint (HANYA saat PATROL, TIDAK saat ATTACK)
        if(currentState == AIState.Patrol && waypoints != null && waypoints.Length > 0) {
            Transform currentWaypoint = waypoints[currentWaypointIndex];
            if(currentWaypoint != null) {
                float distToWaypoint = Vector2.Distance(transform.position, currentWaypoint.position);
                
                // Jika sudah sampai waypoint
                if(distToWaypoint <= waypointReachDistance) {
                    MoveToNextWaypoint();
                }
            }
        }
        // Legacy target system dengan repath interval
        else if(target != null && Time.time >= nextRepathTime) { 
            currentState = AIState.Recalculating;
            RequestNewPath(); 
            nextRepathTime = Time.time + repathInterval; 
        }
    }
    
    /// <summary>
    /// Cek apakah ada player dalam detection range
    /// COOPERATIVE: Expand followRange jika partner sedang Following
    /// </summary>
    void CheckForEnemies() {
        // COOPERATIVE FSM: Cek apakah partner sedang Following/Attack
        float effectiveFollowRange = followRange;
        Transform partnerTarget = null;
        
        if(partnerCPU != null) {
            // Jika partner sedang Following atau Attack, perlebar radius follow kita
            if(partnerCPU.CurrentState == AIState.Following || partnerCPU.CurrentState == AIState.Attack) {
                effectiveFollowRange = Mathf.Infinity; // Radius unlimited untuk ikut ngejar
                partnerTarget = partnerCPU.currentTarget;
                
                // LANGSUNG TRANSITION ke Following, matikan patrol
                if(partnerTarget != null) {
                    currentTarget = partnerTarget;
                    lastKnownWaypointTarget = (waypoints != null && waypoints.Length > 0) ? waypoints[currentWaypointIndex].position : transform.position;
                    
                    float distanceToPartnerTarget = Vector2.Distance(transform.position, partnerTarget.position);
                    
                    Debug.Log($"[{aiName}] [COOPERATIVE] Partner is {partnerCPU.CurrentState}, joining to chase {partnerTarget.name}");
                    
                    // Set flag: kita adalah cooperative follower (ikut partner)
                    isCooperativeFollower = true;
                    
                    // Kalau dalam attack range, langsung attack
                    if(distanceToPartnerTarget <= attackRange) {
                        TransitionToAttack();
                    }
                    // Kalau belum dalam attack range, state Following
                    else {
                        TransitionToFollowing();
                    }
                    return; // STOP patrol, langsung following
                }
            }
        }
        
        // Normal detection: Cek radius follow normal
        Transform nearestPlayer = FindNearestPlayer();
        
        if(nearestPlayer != null) {
            float distance = Vector2.Distance(transform.position, nearestPlayer.position);
            
            if(distance <= followRange) {
                currentTarget = nearestPlayer;
                lastKnownWaypointTarget = (waypoints != null && waypoints.Length > 0) ? waypoints[currentWaypointIndex].position : transform.position;
                
                // Kalau dalam attack range, langsung attack
                if(distance <= attackRange) {
                    TransitionToAttack();
                }
                // Kalau cuma dalam follow range, state Following
                else {
                    TransitionToFollowing();
                }
            }
        }
    }
    
    /// <summary>
    /// Cari player terdekat yang masih hidup
    /// </summary>
    Transform FindNearestPlayer() {
        if(playerTargets == null || playerTargets.Length == 0)
            return null;
        
        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach(Transform player in playerTargets) {
            if(player == null) continue;
            
            // Cek apakah player masih hidup
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if(health != null && !health.IsAlive())
                continue;
            
            float distance = Vector2.Distance(transform.position, player.position);
            
            if(distance < nearestDistance) {
                nearestDistance = distance;
                nearest = player;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// Cek apakah target masih valid (hidup & dalam range)
    /// </summary>
    void CheckTargetValidity() {
        if(currentTarget == null) {
            TransitionToPatrol();
            return;
        }
        
        // Cek apakah target masih hidup
        PlayerHealth health = currentTarget.GetComponent<PlayerHealth>();
        if(health != null && !health.IsAlive()) {
            Debug.Log($"[{aiName}] Target dead, returning to patrol");
            TransitionToPatrol();
            return;
        }
        
        // Cek jarak
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        
        // STATE TRANSITION: Following <-> Attack berdasarkan jarak (CEK DULU sebelum cooperative logic)
        if(distance <= attackRange) {
            // Masuk attack range, transition ke ATTACK jika belum
            if(currentState != AIState.Attack) {
                TransitionToAttack();
            }
        }
        else if(distance <= followRange) {
            // Dalam follow range tapi tidak dalam attack range, state FOLLOWING
            if(currentState != AIState.Following) {
                TransitionToFollowing();
            }
        }
        
        // COOPERATIVE: Jika ini cooperative follower, cek partner state untuk patrol transition
        if(isCooperativeFollower && partnerCPU != null) {
            // Jangan balik patrol selama partner masih Following/Attack
            if(partnerCPU.CurrentState == AIState.Following || partnerCPU.CurrentState == AIState.Attack) {
                // Partner masih aktif, tetap following (meskipun jauh)
                // TAPI tetap bisa transition ke Attack jika masuk attackRange (sudah di-handle di atas)
                if(distance > loseTargetRange) {
                    Debug.Log($"[{aiName}] [COOPERATIVE] Distance {distance:F1}, but partner still active. Continuing...");
                    return; // Tetap following, jangan balik patrol
                }
            }
            else {
                // Partner sudah patrol, baru boleh balik patrol
                Debug.Log($"[{aiName}] [COOPERATIVE] Partner returned to patrol, safe to patrol");
                TransitionToPatrol();
                return;
            }
        }
        
        // Normal behavior: Kalau terlalu jauh (keluar dari loseTargetRange), balik patrol
        if(distance > loseTargetRange) {
            Debug.Log($"[{aiName}] Target too far ({distance:F1} > {loseTargetRange}), returning to patrol");
            TransitionToPatrol();
            return;
        }
        
        // MOVEMENT: Berbeda antara Following (pakai pathfinding A*) dan Attack (direct chase)
        if(currentState == AIState.Following) {
            // FOLLOWING: Pakai pathfinding A* dari Update() - FixedUpdate akan handle movement
            // Tidak perlu movement logic di sini, biar FixedUpdate yang handle path following
        }
        else if(currentState == AIState.Attack) {
            // ATTACK: Direct chase tanpa pathfinding (aggressive, fast)
            if(distance > stopDistance) {
                Vector2 directionToTarget = ((Vector2)currentTarget.position - rb.position).normalized;
                
                // Rotate towards target
                float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                rb.rotation = Mathf.LerpAngle(rb.rotation, targetAngle + facingOffsetDeg, 5f * Time.deltaTime);
                
                // Move towards target
                Vector2 velocity = directionToTarget * moveSpeed;
                rb.linearVelocity = velocity;
                
                Debug.Log($"[{aiName}] [ATTACK] Chasing target, distance: {distance:F1}");
            }
            else {
                // Berhenti di stop distance
                rb.linearVelocity = Vector2.zero;
                Debug.Log($"[{aiName}] [ATTACK] Stopped at target, distance: {distance:F1}");
            }
        }
    }
    
    /// <summary>
    /// Transition ke FOLLOWING state (dalam followRange tapi belum attackRange)
    /// </summary>
    void TransitionToFollowing() {
        currentState = AIState.Following;
        currentAlgorithm = PathfindingAlgorithm.AStar; // FOLLOWING juga pakai A*
        
        // Clear old patrol path immediately
        path = null;
        pathIndex = 0;
        rb.linearVelocity = Vector2.zero;
        
        // COOPERATIVE FSM: Set cooperative target (hanya jika bukan follower)
        if(!isCooperativeFollower) {
            cooperativeTarget = currentTarget;
            initiatorCPU = this;
        }
        
        // Request path immediately dengan A*
        if(currentTarget != null) {
            PathRequestManager.RequestPath(transform.position, currentTarget.position, OnPathFound, PathfindingAlgorithm.AStar);
            nextTargetRepathTime = Time.time + 0.5f;
        }
        
        string coopTag = isCooperativeFollower ? "[COOPERATIVE FOLLOWER]" : "";
        Debug.Log($"[{aiName}] → FOLLOWING (Target: {currentTarget.name}) [A*] [PATROL STOPPED] {coopTag}");
    }
    
    /// <summary>
    /// Transition ke ATTACK state - Switch ke A*
    /// </summary>
    void TransitionToAttack() {
        currentState = AIState.Attack;
        currentAlgorithm = PathfindingAlgorithm.AStar; // ATTACK pakai A*
        
        // Clear old patrol path immediately
        path = null;
        pathIndex = 0;
        
        // Update cooperative target jika belum ada atau kita initiator
        if(cooperativeTarget == null || initiatorCPU == this) {
            cooperativeTarget = currentTarget;
            initiatorCPU = this;
        }
        
        Debug.Log($"[{aiName}] → ATTACK (Target: {currentTarget.name}) [A*] [PATROL STOPPED]");
    }
    
    /// <summary>
    /// Transition kembali ke PATROL state - Switch ke Dijkstra
    /// </summary>
    void TransitionToPatrol() {
        // FORCE PATROL STATE
        currentState = AIState.Patrol;
        currentTarget = null;
        currentAlgorithm = PathfindingAlgorithm.Dijkstra; // PATROL pakai Dijkstra
        
        // Clear old path
        path = null;
        pathIndex = 0;
        rb.linearVelocity = Vector2.zero;
        
        // Clear cooperative target jika kita initiator
        if(initiatorCPU == this) {
            cooperativeTarget = null;
            initiatorCPU = null;
            Debug.Log($"[{aiName}] Clearing cooperative target (initiator)");
        }
        
        // Clear cooperative follower flag
        if(isCooperativeFollower) {
            Debug.Log($"[{aiName}] Clearing cooperative follower status");
            isCooperativeFollower = false;
        }
        
        // Cari waypoint terdekat untuk kembali patrol
        if(waypoints != null && waypoints.Length > 0) {
            FindNearestWaypoint();
            RequestPathToCurrentWaypoint();
        }
        else {
            Debug.LogWarning($"[{aiName}] No waypoints available for patrol!", this);
        }
        
        Debug.Log($"[{aiName}] → PATROL [Dijkstra] (State forced)");
    }
    
    /// <summary>
    /// COOPERATIVE FSM: Alert partner CPU untuk ikut ngejar target yang sama
    /// </summary>
    void AlertPartner() {
        if(partnerCPU == null) return;
        if(currentTarget == null) return;
        
        // Alert partner DIMANAPUN posisinya (bahkan jika jauh)
        partnerCPU.ReceiveCooperativeAlert(currentTarget);
    }
    
    /// <summary>
    /// COOPERATIVE FSM: Terima alert dari partner CPU
    /// </summary>
    public void ReceiveCooperativeAlert(Transform target) {
        // Terima alert dari state apapun (Patrol, Idle, bahkan sedang Following target lain)
        if(currentState == AIState.Following || currentState == AIState.Attack) {
            // Jika sudah following/attack target yang sama, skip
            if(currentTarget == target) return;
        }
        
        Debug.Log($"[{aiName}] Received cooperative alert! Following partner's target: {target.name}");
        
        currentTarget = target;
        lastKnownWaypointTarget = (waypoints != null && waypoints.Length > 0) ? waypoints[currentWaypointIndex].position : transform.position;
        TransitionToFollowing();
    }
    
    /// <summary>
    /// Cari waypoint terdekat dari posisi saat ini
    /// </summary>
    void FindNearestWaypoint() {
        if(waypoints == null || waypoints.Length == 0) return;
        
        float nearestDistance = float.MaxValue;
        int nearestIndex = 0;
        
        for(int i = 0; i < waypoints.Length; i++) {
            if(waypoints[i] == null) continue;
            
            float distance = Vector2.Distance(transform.position, waypoints[i].position);
            if(distance < nearestDistance) {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        currentWaypointIndex = nearestIndex;
        Debug.Log($"[{aiName}] Nearest waypoint found: W{nearestIndex} (distance: {nearestDistance:F1})");
    }
    
    /// <summary>
    /// Pindah ke waypoint berikutnya dengan pattern ping-pong (maju-mundur)
    /// W1>W2>W3>W2>W1>W2>W3...
    /// </summary>
    void MoveToNextWaypoint() {
        // Jika cuma 1 waypoint, stay di tempat
        if(waypoints.Length == 1) {
            currentState = AIState.Patrol;
            return;
        }
        
        // Hitung waypoint berikutnya
        if(movingForward) {
            currentWaypointIndex++;
            // Jika sudah mentok di akhir, balik arah
            if(currentWaypointIndex >= waypoints.Length) {
                currentWaypointIndex = waypoints.Length - 2; // Mundur 1 step
                movingForward = false;
            }
        }
        else {
            currentWaypointIndex--;
            // Jika sudah mentok di awal, balik arah
            if(currentWaypointIndex < 0) {
                currentWaypointIndex = 1; // Maju 1 step
                movingForward = true;
            }
        }
        
        // Request path ke waypoint baru
        currentState = AIState.Patrol;
        RequestPathToCurrentWaypoint();
        
        Debug.Log($"[{aiName}] [PATROL - Dijkstra] Moving to waypoint {currentWaypointIndex} ({(movingForward ? "FORWARD" : "BACKWARD")})");
    }
    
    /// <summary>
    /// Request path ke waypoint yang sedang dituju
    /// </summary>
    void RequestPathToCurrentWaypoint() {
        if(waypoints == null || waypoints.Length == 0) return;
        if(currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Length) return;
        
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if(targetWaypoint == null) {
            Debug.LogWarning($"[{aiName}] Waypoint {currentWaypointIndex} is null!", this);
            return;
        }
        
        // PATROL selalu pakai Dijkstra
        PathRequestManager.RequestPath(transform.position, targetWaypoint.position, OnPathFound, PathfindingAlgorithm.Dijkstra);
    }

    void FixedUpdate(){
        // Progress label berbasis posisi + proyeksi segmen (di Grid)
        if(grid!=null && grid.routeList!=null && grid.routeList.Count>0)
            grid.UpdateRouteProgressByPosition(transform.position);

        // CRITICAL: Jangan gerak kalau lagi ATTACK mode (CheckTargetValidity handle direct movement)
        // FOLLOWING mode boleh pakai pathfinding dari FixedUpdate (A* untuk obstacle avoidance)
        if(currentState == AIState.Attack) {
            return;
        }

        if(path==null || path.Length==0 || pathIndex>=path.Length){
            // Force PATROL state jika tidak sedang Following/Attack
            if(currentState != AIState.Attack && currentState != AIState.Following) {
                if(currentState != AIState.Patrol) {
                    Debug.Log($"[{aiName}] No path, forcing PATROL state (was {currentState})");
                    currentState = AIState.Patrol;
                    // Request path ke waypoint jika ada
                    if(waypoints != null && waypoints.Length > 0) {
                        RequestPathToCurrentWaypoint();
                    }
                }
            }
            rb.linearVelocity = Vector2.zero; return;
        }
        
        // State maintenance: PATROL adalah primary state untuk waypoint movement
        // Following hanya untuk transisi sementara

        Vector3 wp = path[pathIndex];
        if(Vector2.Distance(transform.position, wp) < waypointReachDist){
            pathIndex++;
            if(pathIndex>=path.Length){ rb.linearVelocity=Vector2.zero; return; }

            Vector3 next = path[pathIndex];
            Vector2 nd = ((Vector2)(next - transform.position)).normalized;
            if(nd==Vector2.zero) nd=currentMoveDir;
            currentMoveDir=nd;

            float ang = Mathf.Atan2(nd.y, nd.x)*Mathf.Rad2Deg;
            rb.rotation = ang + facingOffsetDeg;
        }

        if(currentMoveDir==Vector2.zero){
            Vector2 init = ((Vector2)(wp - transform.position)).normalized;
            if(init!=Vector2.zero){
                currentMoveDir=init;
                float ang = Mathf.Atan2(init.y, init.x)*Mathf.Rad2Deg;
                    rb.rotation = ang + facingOffsetDeg;
            }
        }

        Vector2 v = currentMoveDir * moveSpeed;
        rb.MovePosition(rb.position + v * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }

    void RequestNewPath(){
        if(target==null) return;
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound, currentAlgorithm);
    }

    void OnPathFound(Vector3[] newPath, bool success){
        if(!success || newPath==null || newPath.Length==0) {
            // Jangan ubah state - biarkan tetap di state saat ini
            // Jika path gagal saat Following, TransitionToPatrol akan handle
            Debug.LogWarning($"[{aiName}] Pathfinding failed, maintaining current state: {currentState}");
            return;
        }
        path = newPath;

        // mulai dari waypoint terdekat
        int nearest=0; float best=float.PositiveInfinity;
        for(int i=0;i<path.Length;i++){
            float d=(path[i]-transform.position).sqrMagnitude;
            if(d<best){ best=d; nearest=i; }
        }
        pathIndex=nearest;
        currentMoveDir=Vector2.zero;
        // Jangan override state PATROL atau ATTACK
    }

    // ===== FSM STATE MANAGEMENT untuk AI =====
    public string GetStateString() {
        switch (currentState) {
            case AIState.Idle:
                return "IDLE";
            case AIState.Following:
                return "FOLLOWING";
            case AIState.Recalculating:
                return "RECALC";
            case AIState.Patrol:
                return "PATROL";
            case AIState.Attack:
                return "ATTACK";
            default:
                return "UNKNOWN";
        }
    }

    public Color GetStateColor() {
        switch (currentState) {
            case AIState.Idle:
                return Color.gray;
            case AIState.Following:
                return Color.yellow; // Following = kuning
            case AIState.Recalculating:
                return Color.cyan;
            case AIState.Patrol:
                return new Color(0f, 0f, 0f); // Patrol = hitam
            case AIState.Attack:
                return Color.red; // Attack = merah
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Visualisasi radius zones di Scene view
    /// </summary>
    void OnDrawGizmos() {
        if(!Application.isPlaying) return;
        
        Vector3 pos = transform.position;
        
        // Patrol Radius (hitam/abu)
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(pos, patrolRadius);
        
        // Lose Target Range (pink/magenta)
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(pos, loseTargetRange);
        
        // Follow Range (kuning)
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(pos, followRange);
        
        // Attack Range (merah)
        Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
        Gizmos.DrawWireSphere(pos, attackRange);
        
        // Stop Distance (hijau)
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(pos, stopDistance);
        
        // Draw line to current target
        if(currentTarget != null) {
            Gizmos.color = currentState == AIState.Attack ? Color.red : Color.yellow;
            Gizmos.DrawLine(pos, currentTarget.position);
        }
    }
}
