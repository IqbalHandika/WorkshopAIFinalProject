using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// All-in-one Player Controller: Movement + Shooting dengan Input System
/// FSM States: Idle → Moving → Boosting
/// Probability Critical Hit system
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    #region Player State (FSM)
    
    public enum PlayerState {
        Idle,
        Moving,
        Boosting
    }
    
    private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;
    
    #endregion

    #region Inspector Fields

    [Header("PLAYER INFO")]
    public string playerName = "Player 1";
    [Tooltip("Which action map to use")]
    public bool useController2Map = false;

    [Header("MOVEMENT SETTINGS")]
    public float maxSpeed = 6f;
    public float acceleration = 0.06f;
    [Range(0.90f, 0.999f)] public float friction = 0.995f;
    public float reverseFactor = 0.5f;
    public float rudderMaxDegPerFrame = 0.7f;

    [Header("BOOST SETTINGS")]
    public float boostDuration = 1f;
    public float boostMaxSpeedMult = 1.6f;
    public float boostAccelMult = 1.8f;

    [Header("SHOOTING SETTINGS")]
    public Transform cannonTransform;
    public Transform firePoint;
    public GameObject cannonballPrefab;
    public float cannonballSpeed = 15f;
    public float cannonballLifetime = 5f;
    public float fireRate = 1f;
    public float damage = 20f;
    public float minAimDistance = 0.3f;

    [Header("PROBABILITY: CRITICAL HIT")]
    [Range(0f, 1f)] public float criticalChance = 0.25f;
    [Range(1f, 5f)] public float criticalMultiplier = 2.0f;
    public Color criticalColor = Color.red;
    public Color normalColor = Color.white;

    [Header("VISUAL & AUDIO")]
    public bool showAimLine = false;
    public LineRenderer aimLine;
    public float aimLineLength = 3f;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip criticalSound;

    [Header("KEYBOARD FALLBACK")]
    public bool enableKeyboard = true;

    #endregion

    #region Private Variables

    private ControllerInput inputActions;
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // Movement
    private Vector2 moveInput;
    private bool throttlePressed;
    private bool reversePressed;
    private float speed;
    private float boostUntil = -1f;
    private float idleSpeedThreshold = 0.1f;
    
    // Shooting
    private Vector2 aimInput;
    private Vector2 aimDirection;
    private Vector2 lastValidAimDirection;
    private float nextFireTime = 0f;

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        // Input Actions
        inputActions = new ControllerInput();
        
        // CRITICAL: Assign specific device based on which controller map is used
        var gamepads = Gamepad.all;
        if (useController2Map) {
            // Controller 2 map = Player 2 = Second gamepad or specific device
            if (gamepads.Count > 1) {
                inputActions.devices = new[] { gamepads[1] };
                Debug.Log($"[{playerName}] Using Controller 2 map with Gamepad 1: {gamepads[1].name}");
            } else if (gamepads.Count > 0) {
                // Fallback to first gamepad if only one connected
                inputActions.devices = new[] { gamepads[0] };
                Debug.LogWarning($"[{playerName}] Only 1 gamepad found, using it for Controller 2 map");
            }
        } else {
            // Controller (default) map = Player 1 = First gamepad
            if (gamepads.Count > 0) {
                inputActions.devices = new[] { gamepads[0] };
                Debug.Log($"[{playerName}] Using Controller map with Gamepad 0: {gamepads[0].name}");
            }
        }
        
        // Rigidbody setup
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        mainCamera = Camera.main;
        lastValidAimDirection = Vector2.up;
    }

    void OnEnable()
    {
        inputActions.Enable();
        
        if (useController2Map)
        {
            // Subscribe to Controller 2 map
            inputActions.Controller2.Movement.performed += OnMovementLeftInput;
            inputActions.Controller2.Movement.canceled += OnMovementLeftInput;
            
            inputActions.Controller2.Movement1.performed += OnMovementRightInput;
            inputActions.Controller2.Movement1.canceled += OnMovementRightInput;
            
            inputActions.Controller2.Aim.performed += OnAimingInput;
            inputActions.Controller2.Aim.canceled += OnAimingInput;
            
            inputActions.Controller2.Shooting.performed += OnShootingInput;
            inputActions.Controller2.Boosting.performed += OnBoostingInput;
            
            inputActions.Controller2.Throttle.performed += OnThrottleInput;
            inputActions.Controller2.Throttle.canceled += OnThrottleInput;
            
            inputActions.Controller2.Reverse.performed += OnReverseInput;
            inputActions.Controller2.Reverse.canceled += OnReverseInput;
            
            Debug.Log($"[{playerName}] Using Controller 2 action map");
        }
        else
        {
            // Subscribe to Controller (default) map
            inputActions.Controller.Movement.performed += OnMovementLeftInput;
            inputActions.Controller.Movement.canceled += OnMovementLeftInput;
            
            inputActions.Controller.Movement1.performed += OnMovementRightInput;
            inputActions.Controller.Movement1.canceled += OnMovementRightInput;
            
            inputActions.Controller.Aiming.performed += OnAimingInput;
            inputActions.Controller.Aiming.canceled += OnAimingInput;
            
            inputActions.Controller.Shooting.performed += OnShootingInput;
            inputActions.Controller.Boosting.performed += OnBoostingInput;
            
            inputActions.Controller.Throttle.performed += OnThrottleInput;
            inputActions.Controller.Throttle.canceled += OnThrottleInput;
            
            inputActions.Controller.Reverse.performed += OnReverseInput;
            inputActions.Controller.Reverse.canceled += OnReverseInput;
            
            Debug.Log($"[{playerName}] Using Controller (default) action map");
        }
    }

    void OnDisable()
    {
        if (useController2Map)
        {
            inputActions.Controller2.Movement.performed -= OnMovementLeftInput;
            inputActions.Controller2.Movement.canceled -= OnMovementLeftInput;
            inputActions.Controller2.Movement1.performed -= OnMovementRightInput;
            inputActions.Controller2.Movement1.canceled -= OnMovementRightInput;
            inputActions.Controller2.Aim.performed -= OnAimingInput;
            inputActions.Controller2.Aim.canceled -= OnAimingInput;
            inputActions.Controller2.Shooting.performed -= OnShootingInput;
            inputActions.Controller2.Boosting.performed -= OnBoostingInput;
            inputActions.Controller2.Throttle.performed -= OnThrottleInput;
            inputActions.Controller2.Throttle.canceled -= OnThrottleInput;
            inputActions.Controller2.Reverse.performed -= OnReverseInput;
            inputActions.Controller2.Reverse.canceled -= OnReverseInput;
        }
        else
        {
            inputActions.Controller.Movement.performed -= OnMovementLeftInput;
            inputActions.Controller.Movement.canceled -= OnMovementLeftInput;
            inputActions.Controller.Movement1.performed -= OnMovementRightInput;
            inputActions.Controller.Movement1.canceled -= OnMovementRightInput;
            inputActions.Controller.Aiming.performed -= OnAimingInput;
            inputActions.Controller.Aiming.canceled -= OnAimingInput;
            inputActions.Controller.Shooting.performed -= OnShootingInput;
            inputActions.Controller.Boosting.performed -= OnBoostingInput;
            inputActions.Controller.Throttle.performed -= OnThrottleInput;
            inputActions.Controller.Throttle.canceled -= OnThrottleInput;
            inputActions.Controller.Reverse.performed -= OnReverseInput;
            inputActions.Controller.Reverse.canceled -= OnReverseInput;
        }
        
        inputActions.Disable();
    }

    void Start()
    {
        // Auto-disable old movement scripts to prevent conflict
        var oldMovement = GetComponent<KeyMovement2D>();
        if (oldMovement != null)
        {
            oldMovement.enabled = false;
            Debug.LogWarning($"[{playerName}] Auto-disabled KeyMovement2D to prevent input conflict with Controller");
        }

        // Setup fire point jika belum ada
        if (firePoint == null && cannonTransform != null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(cannonTransform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = fp.transform;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Keyboard input
        if (enableKeyboard)
        {
            HandleKeyboardInput();
        }

        // Update aim direction & cannon rotation
        UpdateAimDirection();
        RotateCannon();

        // Update aim line
        if (showAimLine && aimLine != null)
        {
            UpdateAimLine();
        }
    }

    void HandleKeyboardInput()
    {
        // WASD movement
        bool gamepadMoving = useController2Map 
            ? (inputActions.Controller2.Movement.IsPressed() || inputActions.Controller2.Movement1.IsPressed())
            : (inputActions.Controller.Movement.IsPressed() || inputActions.Controller.Movement1.IsPressed());
            
        if (Input.GetKey(KeyCode.A))
            moveInput.x = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveInput.x = 1f;
        else if (!gamepadMoving)
            moveInput.x = 0f;

        // W = forward, S = reverse
        bool gamepadThrottling = useController2Map
            ? (inputActions.Controller2.Throttle.IsPressed() || inputActions.Controller2.Reverse.IsPressed())
            : (inputActions.Controller.Throttle.IsPressed() || inputActions.Controller.Reverse.IsPressed());
            
        if (Input.GetKey(KeyCode.W))
        {
            throttlePressed = true;
            reversePressed = false;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            reversePressed = true;
            throttlePressed = false;
        }
        else if (!gamepadThrottling)
        {
            throttlePressed = false;
            reversePressed = false;
        }

        // Shift = boost
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            boostUntil = Time.time + boostDuration;
        }

        // Space = shoot
        if (Input.GetKeyDown(KeyCode.Space) && CanFire())
        {
            Fire();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    #endregion

    #region Input Callbacks

    private void OnMovementLeftInput(InputAction.CallbackContext context)
    {
        // Left stick left (A)
        if (context.performed || context.started)
        {
            moveInput.x = 1f;
        }
        else if (context.canceled)
        {
            // Reset only if currently pressing left
            if (moveInput.x > 0) moveInput.x = 0;
        }
    }

    private void OnMovementRightInput(InputAction.CallbackContext context)
    {
        // Left stick right (D)
        if (context.performed || context.started)
        {
            moveInput.x = -1f;
        }
        else if (context.canceled)
        {
            // Reset only if currently pressing right
            if (moveInput.x < 0) moveInput.x = 0;
        }
    }

    private void OnAimingInput(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
    }

    private void OnShootingInput(InputAction.CallbackContext context)
    {
        if (context.performed && CanFire())
        {
            Fire();
        }
    }

    private void OnBoostingInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            boostUntil = Time.time + boostDuration;
        }
    }

    private void OnThrottleInput(InputAction.CallbackContext context)
    {
        // RB = forward
        if (context.performed || context.started)
        {
            throttlePressed = true;
        }
        else if (context.canceled)
        {
            throttlePressed = false;
        }
    }

    private void OnReverseInput(InputAction.CallbackContext context)
    {
        // LB = reverse
        if (context.performed || context.started)
        {
            reversePressed = true;
        }
        else if (context.canceled)
        {
            reversePressed = false;
        }
    }

    #endregion

    #region Movement

    void HandleMovement()
    {
        bool isBoosting = Time.time < boostUntil;
        float curMax = isBoosting ? maxSpeed * boostMaxSpeedMult : maxSpeed;
        float curAccel = isBoosting ? acceleration * boostAccelMult : acceleration;

        float hInput = 0f;
        float vInput = 0f;
        
        // GAMEPAD: Movement button (left/right dari left stick), Throttle/Reverse (RB/LB)
        hInput = moveInput.x; // Left stick left/right
        
        if (throttlePressed)
        {
            vInput = 1f;  // RB = forward
        }
        else if (reversePressed)
        {
            vInput = -1f; // LB = reverse
        }

        // Update FSM state
        if (isBoosting)
        {
            currentState = PlayerState.Boosting;
        }
        else if (Mathf.Abs(speed) > idleSpeedThreshold || Mathf.Abs(vInput) > 0.01f || Mathf.Abs(hInput) > 0.01f)
        {
            currentState = PlayerState.Moving;
        }
        else
        {
            currentState = PlayerState.Idle;
        }

        // Throttle
        if (vInput > 0.01f)
        {
            speed += curAccel;
        }
        else if (vInput < -0.01f)
        {
            speed -= curAccel * reverseFactor;
        }

        speed = Mathf.Clamp(speed, -curMax * reverseFactor, curMax);
        speed *= friction;

        // Rudder steering
        if (Mathf.Abs(hInput) > 0.01f && Mathf.Abs(speed) > 0.1f)
        {
            float turnDir = Mathf.Sign(speed);
            float ang = rb.rotation;
            ang += hInput * rudderMaxDegPerFrame * turnDir;
            rb.rotation = ang;
        }

        // Apply velocity
        Vector2 forward = new Vector2(
            Mathf.Sin(rb.rotation * Mathf.Deg2Rad),
            -Mathf.Cos(rb.rotation * Mathf.Deg2Rad)
        );
        rb.linearVelocity = forward * speed;
    }

    #endregion

    #region Shooting

    void UpdateAimDirection()
    {
        // Keyboard mouse aim
        if (enableKeyboard)
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos != Vector3.zero)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                worldPos.z = 0;
                Vector2 mouseDir = (worldPos - transform.position);
                if (mouseDir.magnitude > 0.5f)
                {
                    aimDirection = mouseDir.normalized;
                    lastValidAimDirection = aimDirection;
                    return;
                }
            }
        }

        // Gamepad right stick
        float magnitude = aimInput.magnitude;
        
        if (magnitude > minAimDistance)
        {
            aimDirection = aimInput.normalized;
            lastValidAimDirection = aimDirection;
        }
        else
        {
            // Keep last valid direction if stick is in deadzone
            aimDirection = lastValidAimDirection;
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

    void Fire()
    {
        if (cannonballPrefab == null || firePoint == null)
        {
            Debug.LogError($"[{playerName}] Cannonball prefab or fire point not assigned!");
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
            Debug.Log($"<color=red>[{playerName}] CRITICAL HIT!</color> Damage: {finalDamage}");
        else
            Debug.Log($"[{playerName}] Shot. Damage: {finalDamage}");

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

    #endregion

    #region FSM State Methods (for UI)

    public string GetStateString()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                return "IDLE";
            case PlayerState.Moving:
                return "MOVING";
            case PlayerState.Boosting:
                return "BOOSTING";
            default:
                return "UNKNOWN";
        }
    }

    public Color GetStateColor()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                return Color.gray;
            case PlayerState.Moving:
                return Color.green;
            case PlayerState.Boosting:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }

    #endregion

    #region Gizmos

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

    #endregion
}
