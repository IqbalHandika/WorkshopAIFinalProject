using UnityEngine;

/// <summary>
/// Sistema shooting dengan Probability-based Critical Hit
/// Player 1: Mouse aim (testing) + Left Click
/// Player 2+: Nanti ganti ke Analog Stick kanan
/// </summary>
public class ShootingSystem : MonoBehaviour
{
    [Header("Cannon References")]
    public Transform cannonTransform;        // Drag cannon child object
    public Transform firePoint;              // Titik spawn cannonball (ujung cannon)
    
    [Header("Cannonball")]
    public GameObject cannonballPrefab;      // Drag cannonball prefab
    public float cannonballSpeed = 15f;
    public float cannonballLifetime = 5f;    // Auto destroy setelah X detik

    [Header("Shooting Settings")]
    public float fireRate = 1f;              // Tembakan per detik
    private float nextFireTime = 0f;
    
    [Header("Damage Settings")]
    [Tooltip("Base damage per shot")]
    public float damage = 20f;               // Base damage

    [Header("Aim Control (Testing)")]
    public bool useMouseAim = true;          // TRUE untuk testing, FALSE nanti untuk joystick
    public KeyCode fireKey = KeyCode.Mouse0; // Left Click untuk testing
    
    [Header("Joystick Aim (Nanti)")]
    public string horizontalAxis = "Horizontal2"; // Analog kanan horizontal
    public string verticalAxis = "Vertical2";     // Analog kanan vertical
    public float minAimDistance = 0.3f;           // Deadzone analog stick

    [Header("━━━━━ PROBABILITY: CRITICAL HIT ━━━━━")]
    [Space(5)]
    [Tooltip("Chance untuk critical hit (0 = 0%, 1 = 100%)")]
    [Range(0f, 1f)]
    public float criticalChance = 0.25f;     // 25% chance critical
    
    [Tooltip("Damage multiplier untuk critical (2 = 2x damage)")]
    [Range(1f, 5f)]
    public float criticalMultiplier = 2.0f;  // 2x damage saat critical
    
    [Space(10)]
    [Header("Critical Visual Settings")]
    public Color criticalColor = Color.red;  // Warna cannonball critical
    public Color normalColor = Color.white;  // Warna cannonball normal

    [Header("Visual Feedback")]
    public bool showAimLine = true;
    public LineRenderer aimLine;             // Optional: Line untuk show aim direction
    public float aimLineLength = 3f;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip criticalSound;

    private Camera mainCamera;
    private Vector2 aimDirection;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Setup fire point jika belum ada
        if (firePoint == null && cannonTransform != null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(cannonTransform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0); // Adjust sesuai cannon
            firePoint = fp.transform;
        }

        // Setup audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Update aim direction
        UpdateAimDirection();

        // Rotate cannon ke arah aim
        RotateCannon();

        // Fire input
        if (CanFire() && GetFireInput())
        {
            Fire();
        }

        // Update aim line visual
        if (showAimLine && aimLine != null)
        {
            UpdateAimLine();
        }
    }

    void UpdateAimDirection()
    {
        if (useMouseAim)
        {
            // Mouse aim untuk testing
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            aimDirection = (mousePos - transform.position).normalized;
        }
        else
        {
            // Joystick analog kanan (nanti)
            float h = Input.GetAxis(horizontalAxis);
            float v = Input.GetAxis(verticalAxis);
            
            // Check deadzone
            if (h * h + v * v > minAimDistance * minAimDistance)
            {
                aimDirection = new Vector2(h, v).normalized;
            }
            // Kalau analog tidak digerakkan, pakai arah hadap cannon
            else if (cannonTransform != null)
            {
                aimDirection = cannonTransform.up;
            }
        }
    }

    void RotateCannon()
    {
        if (cannonTransform == null) return;

        // Hitung angle dari aim direction
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        // Rotate cannon (adjust offset sesuai sprite orientation)
        // Biasanya cannon sprite menghadap kanan = 0 deg, jadi offset -90
        cannonTransform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    bool CanFire()
    {
        return Time.time >= nextFireTime;
    }

    bool GetFireInput()
    {
        if (useMouseAim)
        {
            return Input.GetKeyDown(fireKey);
        }
        else
        {
            // Nanti ganti ke joystick button (R1/RB/etc)
            return Input.GetButtonDown("Fire1"); // Sesuaikan button name
        }
    }

    void Fire()
    {
        if (cannonballPrefab == null)
        {
            Debug.LogError("Cannonball prefab not assigned!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Fire point not assigned!");
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
        if (isCritical)
            Debug.Log($"<color=red>CRITICAL HIT!</color> Damage: {finalDamage} ({criticalChance * 100}% chance)");
        else
            Debug.Log($"Normal shot. Damage: {finalDamage}");

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
    }
}
