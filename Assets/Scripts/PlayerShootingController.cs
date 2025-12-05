using UnityEngine;

/// <summary>
/// Player shooting system dengan support Keyboard DAN Joystick
/// Probability-based Critical Hit
/// </summary>
public class PlayerShootingController : MonoBehaviour
{
    [Header("Cannon References")]
    public Transform cannonTransform;
    public Transform firePoint;
    
    [Header("Cannonball")]
    public GameObject cannonballPrefab;
    public float cannonballSpeed = 15f;
    public float cannonballLifetime = 5f;

    [Header("Shooting Settings")]
    public float fireRate = 1f;
    private float nextFireTime = 0f;
    
    [Header("Damage Settings")]
    public float damage = 20f;

    [Header("━━━━━ INPUT MODE ━━━━━")]
    [Tooltip("TRUE = Mouse aim, FALSE = Joystick")]
    public bool useMouseAim = true;
    
    [Header("Keyboard/Mouse Input")]
    public KeyCode fireKey = KeyCode.Mouse0;
    
    [Header("Joystick Input")]
    public JoystickInputMapper joystickInput;
    public float minAimDistance = 0.3f; // Deadzone analog kanan

    [Header("━━━━━ PROBABILITY: CRITICAL HIT ━━━━━")]
    [Space(5)]
    [Range(0f, 1f)]
    public float criticalChance = 0.25f;
    
    [Range(1f, 5f)]
    public float criticalMultiplier = 2.0f;
    
    [Space(10)]
    [Header("Critical Visual Settings")]
    public Color criticalColor = Color.red;
    public Color normalColor = Color.white;

    [Header("Visual Feedback")]
    public bool showAimLine = true;
    public LineRenderer aimLine;
    public float aimLineLength = 3f;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip criticalSound;

    private Camera mainCamera;
    private Vector2 aimDirection;
    private Vector2 lastValidAimDirection;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Setup fire point
        if (firePoint == null && cannonTransform != null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(cannonTransform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = fp.transform;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // Default aim direction
        lastValidAimDirection = Vector2.up;
    }

    void Update()
    {
        UpdateAimDirection();
        RotateCannon();

        if (CanFire() && GetFireInput())
        {
            Fire();
        }

        if (showAimLine && aimLine != null)
        {
            UpdateAimLine();
        }
    }

    void UpdateAimDirection()
    {
        if (useMouseAim)
        {
            // Mouse aim
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            aimDirection = (mousePos - transform.position).normalized;
            lastValidAimDirection = aimDirection;
        }
        else
        {
            // Joystick analog kanan
            float h = joystickInput.GetAimHorizontal();
            float v = joystickInput.GetAimVertical();
            
            // Check deadzone
            float magnitude = Mathf.Sqrt(h * h + v * v);
            if (magnitude > minAimDistance)
            {
                aimDirection = new Vector2(h, v).normalized;
                lastValidAimDirection = aimDirection;
            }
            else
            {
                // Keep last valid direction jika analog tidak digerakkan
                aimDirection = lastValidAimDirection;
            }
        }
    }

    void RotateCannon()
    {
        if (cannonTransform == null) return;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
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
            // Joystick shoot button (RB/R1)
            return joystickInput.GetShootButtonDown();
        }
    }

    void Fire()
    {
        if (cannonballPrefab == null || firePoint == null)
        {
            Debug.LogError("Cannonball prefab or fire point not assigned!");
            return;
        }

        // Probability: Critical Hit
        bool isCritical = Random.value <= criticalChance;
        float finalDamage = isCritical ? damage * criticalMultiplier : damage;
        
        // Spawn cannonball
        GameObject cannonball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
        
        Cannonball cbScript = cannonball.GetComponent<Cannonball>();
        if (cbScript == null)
            cbScript = cannonball.AddComponent<Cannonball>();
        
        cbScript.Initialize(aimDirection, cannonballSpeed, finalDamage, isCritical, this.gameObject);
        
        // Visual
        SpriteRenderer sr = cannonball.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isCritical ? criticalColor : normalColor;
        }

        Destroy(cannonball, cannonballLifetime);

        // Audio
        if (audioSource != null)
        {
            AudioClip clip = isCritical ? criticalSound : fireSound;
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        // Debug
        if (isCritical)
            Debug.Log($"<color=red>CRITICAL HIT!</color> Damage: {finalDamage}");
        else
            Debug.Log($"Normal shot. Damage: {finalDamage}");

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

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)aimDirection * 2f);
        
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }
    }
}
