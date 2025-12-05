using UnityEngine;

/// <summary>
/// Shooting system untuk CPU dengan auto-aim ke player terdekat
/// CPU akan otomatis aim dan shoot ke P1 atau P2 yang paling dekat
/// </summary>
public class CPUShootingSystem : MonoBehaviour
{
    [Header("CPU Info")]
    public string cpuName = "CPU 1";

    [Header("Cannon References")]
    public Transform cannonTransform;        // Drag cannon child object
    public Transform firePoint;              // Titik spawn cannonball (ujung cannon)
    
    [Header("Cannonball")]
    public GameObject cannonballPrefab;      // Drag cannonball prefab
    public float cannonballSpeed = 15f;
    public float cannonballLifetime = 5f;    // Auto destroy setelah X detik

    [Header("Shooting Settings")]
    public float fireRate = 0.5f;            // Tembakan per detik (lebih lambat dari player)
    private float nextFireTime = 0f;
    
    [Header("Damage Settings")]
    [Tooltip("Base damage per shot")]
    public float damage = 15f;               // Base damage (lebih kecil dari player)

    [Header("Auto-Aim Settings")]
    [Tooltip("Tag untuk detect player (misal: 'Player')")]
    public string playerTag = "Player";
    
    [Tooltip("Jarak maksimum untuk shoot (tidak shoot jika terlalu jauh)")]
    public float maxShootRange = 15f;
    
    [Tooltip("Jarak minimum untuk shoot (tidak shoot jika terlalu dekat)")]
    public float minShootRange = 2f;
    
    [Tooltip("Update target interval (seconds)")]
    public float targetUpdateInterval = 0.5f;
    private float nextTargetUpdateTime = 0f;
    
    private Transform currentTarget;         // Player terdekat saat ini
    private Vector2 aimDirection;

    [Header("━━━━━ PROBABILITY: CRITICAL HIT ━━━━━")]
    [Space(5)]
    [Tooltip("Chance untuk critical hit (0 = 0%, 1 = 100%)")]
    [Range(0f, 1f)]
    public float criticalChance = 0.15f;     // 15% chance critical (lebih rendah dari player)
    
    [Tooltip("Damage multiplier untuk critical (2 = 2x damage)")]
    [Range(1f, 5f)]
    public float criticalMultiplier = 2.0f;  // 2x damage saat critical
    
    [Space(10)]
    [Header("Critical Visual Settings")]
    public Color criticalColor = Color.red;  // Warna cannonball critical
    public Color normalColor = Color.white;  // Warna cannonball normal

    [Header("Visual Feedback")]
    public bool showAimLine = false;         // Disable untuk CPU (lebih fair)
    public LineRenderer aimLine;             // Optional: Line untuk show aim direction
    public float aimLineLength = 3f;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip criticalSound;

    [Header("AI Behavior")]
    [Tooltip("Random delay sebelum shoot (min-max seconds)")]
    public Vector2 shootDelayRange = new Vector2(0.2f, 0.5f);
    private float shootDelay = 0f;

    void Start()
    {
        // Setup fire point jika belum ada
        if (firePoint == null && cannonTransform != null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(cannonTransform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0); // Adjust sesuai cannon
            firePoint = fp.transform;
            Debug.Log($"[{cpuName}] Auto-created FirePoint");
        }

        // Setup audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // Initial target update
        FindNearestPlayer();
        
        // Debug initial state
        Debug.Log($"[{cpuName}] CPUShootingSystem initialized. Player tag: '{playerTag}', Cannon: {(cannonTransform != null ? "OK" : "NULL")}");
    }

    void Update()
    {
        // Update target secara berkala (tidak tiap frame untuk performance)
        if (Time.time >= nextTargetUpdateTime)
        {
            FindNearestPlayer();
            nextTargetUpdateTime = Time.time + targetUpdateInterval;
        }

        // Update aim direction ke target
        UpdateAimDirection();

        // Rotate cannon ke arah target
        RotateCannon();

        // Auto shoot jika ada target valid
        if (CanShoot())
        {
            if (shootDelay <= 0f)
            {
                Fire();
                // Set random delay untuk shoot berikutnya
                shootDelay = Random.Range(shootDelayRange.x, shootDelayRange.y);
            }
            else
            {
                shootDelay -= Time.deltaTime;
            }
        }

        // Update aim line visual (jika enabled)
        if (showAimLine && aimLine != null && currentTarget != null)
        {
            UpdateAimLine();
        }
    }

    /// <summary>
    /// Cari player terdekat (P1 atau P2)
    /// </summary>
    void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        
        Debug.Log($"[{cpuName}] Finding nearest player... Found {players.Length} objects with tag '{playerTag}'");
        
        if (players.Length == 0)
        {
            Debug.LogWarning($"[{cpuName}] No players found with tag '{playerTag}'! Make sure Player GameObjects have correct tag.");
            currentTarget = null;
            return;
        }

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            // Skip jika player sudah mati
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsAlive())
            {
                Debug.Log($"[{cpuName}] Skipping {player.name} (dead)");
                continue;
            }

            float distance = Vector2.Distance(transform.position, player.transform.position);
            Debug.Log($"[{cpuName}] Distance to {player.name}: {distance:F1}");
            
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = player.transform;
            }
        }

        // Update target hanya jika berubah
        if (currentTarget != nearest)
        {
            currentTarget = nearest;
            if (currentTarget != null)
            {
                Debug.Log($"<color=cyan>[{cpuName}] NEW TARGET: {currentTarget.name} (distance: {nearestDistance:F1})</color>");
            }
            else
            {
                Debug.LogWarning($"[{cpuName}] No valid target found (all players dead or out of range)");
            }
        }
    }

    void UpdateAimDirection()
    {
        if (currentTarget == null)
        {
            // Default aim forward jika tidak ada target
            if (cannonTransform != null)
                aimDirection = cannonTransform.up;
            else
                aimDirection = Vector2.up;
            return;
        }

        // Aim ke target
        Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
        aimDirection = directionToTarget;
        
        // Debug every few frames
        if (Time.frameCount % 60 == 0) // Every ~1 second at 60fps
        {
            Debug.Log($"[{cpuName}] Aiming at {currentTarget.name}: direction = {aimDirection}, angle = {Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg:F1}°");
        }
    }

    void RotateCannon()
    {
        if (cannonTransform == null)
        {
            if (Time.frameCount % 120 == 0) // Warning every ~2 seconds
                Debug.LogWarning($"[{cpuName}] Cannon Transform is NULL! Assign Cannon child in Inspector.");
            return;
        }

        // Hitung angle dari aim direction
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        // Rotate cannon (adjust offset sesuai sprite orientation)
        // Biasanya cannon sprite menghadap kanan = 0 deg, jadi offset -90
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);
        cannonTransform.rotation = targetRotation;
        
        // Debug rotation
        if (Time.frameCount % 60 == 0 && currentTarget != null)
        {
            Debug.Log($"[{cpuName}] Cannon rotation: {cannonTransform.rotation.eulerAngles.z:F1}° (target angle: {angle - 90f:F1}°)");
        }
    }

    bool CanShoot()
    {
        // Check fire rate cooldown
        if (Time.time < nextFireTime)
            return false;

        // Check ada target
        if (currentTarget == null)
            return false;

        // Check target masih hidup
        PlayerHealth targetHealth = currentTarget.GetComponent<PlayerHealth>();
        if (targetHealth != null && !targetHealth.IsAlive())
            return false;

        // Check jarak ke target
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance > maxShootRange || distance < minShootRange)
            return false;

        return true;
    }

    void Fire()
    {
        if (cannonballPrefab == null)
        {
            Debug.LogError($"[{cpuName}] Cannonball prefab not assigned!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError($"[{cpuName}] Fire point not assigned!");
            return;
        }

        // === PROBABILITY LOGIC: Critical Hit ===
        bool isCritical = Random.value <= criticalChance;
        float finalDamage = isCritical ? damage * criticalMultiplier : damage;
        
        // Spawn cannonball
        GameObject cannonball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
        
        // Setup cannonball component
        Cannonball cbScript = cannonball.GetComponent<Cannonball>();
        if (cbScript == null)
            cbScript = cannonball.AddComponent<Cannonball>();
        
        // Initialize dengan owner (this gameObject) agar tidak collision sendiri
        cbScript.Initialize(aimDirection, cannonballSpeed, finalDamage, isCritical, this.gameObject);
        
        // Visual: Change color jika critical
        SpriteRenderer sr = cannonball.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isCritical ? criticalColor : normalColor;
        }

        // Destroy cannonball setelah lifetime
        Destroy(cannonball, cannonballLifetime);

        // Audio feedback
        if (audioSource != null)
        {
            AudioClip clip = isCritical ? criticalSound : fireSound;
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        // Debug log
        string targetName = currentTarget != null ? currentTarget.name : "Unknown";
        if (isCritical)
            Debug.Log($"<color=orange>[{cpuName}]</color> <color=red>CRITICAL HIT!</color> → {targetName} | Damage: {finalDamage}");
        else
            Debug.Log($"<color=orange>[{cpuName}]</color> Shot → {targetName} | Damage: {finalDamage}");

        // Update fire cooldown
        nextFireTime = Time.time + (1f / fireRate);
    }

    void UpdateAimLine()
    {
        if (aimLine == null) return;

        Vector3 endPos = (Vector3)(aimDirection * aimLineLength) + transform.position;
        
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, endPos);
        aimLine.enabled = true;
    }

    // Gizmos untuk debug
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Draw aim direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)aimDirection * 2f);
        
        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }

        // Draw target connection
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }

        // Draw shoot range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxShootRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minShootRange);
    }

    // Public method untuk disable/enable shooting (misal saat CPU mati)
    public void SetShootingEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}
