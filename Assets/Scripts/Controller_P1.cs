using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player 1 Controller - Uses first gamepad (Controller map)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Controller_P1 : MonoBehaviour
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
        inputActions = new ControllerInput();
        
        // CRITICAL: Disable Controller2 map to prevent conflicts
        inputActions.Controller2.Disable();
        
        // PLAYER 1: Find Xbox/XInput controller ONLY (not ShanWan)
        var gamepads = Gamepad.all;
        Gamepad xboxGamepad = null;
        
        foreach (var pad in gamepads) {
            string deviceName = pad.name.ToLower();
            Debug.Log($"[P1] Checking device: {pad.name}");
            
            // Skip ShanWan device
            if (deviceName.Contains("shanwan")) {
                Debug.Log($"[P1] Skipping ShanWan device: {pad.name}");
                continue;
            }
            
            // Accept Xbox or XInput controller
            if (deviceName.Contains("xbox") || deviceName.Contains("xinput") || pad is UnityEngine.InputSystem.XInput.XInputController) {
                xboxGamepad = pad;
                Debug.Log($"[P1] Found Xbox controller: {pad.name}");
                break;
            }
        }
        
        if (xboxGamepad != null) {
            inputActions.devices = new[] { xboxGamepad };
            Debug.Log($"[P1] ✅ Assigned to Xbox controller: {xboxGamepad.name}");
        } else {
            Debug.LogWarning("[P1] ⚠️ No Xbox controller found, using keyboard fallback");
        }
        
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
    }

    void OnDisable()
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
        inputActions.Disable();
    }

    void Start()
    {
        var oldMovement = GetComponent<KeyMovement2D>();
        if (oldMovement != null) {
            oldMovement.enabled = false;
            Debug.LogWarning($"[{playerName}] Auto-disabled KeyMovement2D");
        }

        if (firePoint == null && cannonTransform != null) {
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
        if (enableKeyboard) {
            HandleKeyboardInput();
        }

        UpdateAimDirection();
        RotateCannon();

        if (showAimLine && aimLine != null) {
            UpdateAimLine();
        }
    }

    void HandleKeyboardInput()
    {
        bool gamepadMoving = inputActions.Controller.Movement.IsPressed() || inputActions.Controller.Movement1.IsPressed();
            
        if (Input.GetKey(KeyCode.A))
            moveInput.x = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveInput.x = 1f;
        else if (!gamepadMoving)
            moveInput.x = 0f;

        bool gamepadThrottling = inputActions.Controller.Throttle.IsPressed() || inputActions.Controller.Reverse.IsPressed();
            
        if (Input.GetKey(KeyCode.W)) {
            throttlePressed = true;
            reversePressed = false;
        }
        else if (Input.GetKey(KeyCode.S)) {
            reversePressed = true;
            throttlePressed = false;
        }
        else if (!gamepadThrottling) {
            throttlePressed = false;
            reversePressed = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            boostUntil = Time.time + boostDuration;
        }

        if (Input.GetKeyDown(KeyCode.Space) && CanFire()) {
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
        if (context.performed || context.started)
            moveInput.x = 1f;
        else if (context.canceled)
            if (moveInput.x > 0) moveInput.x = 0;
    }

    private void OnMovementRightInput(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
            moveInput.x = -1f;
        else if (context.canceled)
            if (moveInput.x < 0) moveInput.x = 0;
    }

    private void OnAimingInput(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
    }

    private void OnShootingInput(InputAction.CallbackContext context)
    {
        if (context.performed && CanFire())
            Fire();
    }

    private void OnBoostingInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            boostUntil = Time.time + boostDuration;
    }

    private void OnThrottleInput(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
            throttlePressed = true;
        else if (context.canceled)
            throttlePressed = false;
    }

    private void OnReverseInput(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
            reversePressed = true;
        else if (context.canceled)
            reversePressed = false;
    }

    #endregion

    #region Movement

    void HandleMovement()
    {
        bool isBoosting = Time.time < boostUntil;
        float curMax = isBoosting ? maxSpeed * boostMaxSpeedMult : maxSpeed;
        float curAccel = isBoosting ? acceleration * boostAccelMult : acceleration;

        float hInput = moveInput.x;
        float vInput = 0f;
        
        if (throttlePressed)
            vInput = 1f;
        else if (reversePressed)
            vInput = -1f;

        if (isBoosting)
            currentState = PlayerState.Boosting;
        else if (Mathf.Abs(speed) > idleSpeedThreshold || Mathf.Abs(vInput) > 0.01f || Mathf.Abs(hInput) > 0.01f)
            currentState = PlayerState.Moving;
        else
            currentState = PlayerState.Idle;

        if (vInput > 0.01f)
            speed += curAccel;
        else if (vInput < -0.01f)
            speed -= curAccel * reverseFactor;

        speed = Mathf.Clamp(speed, -curMax * reverseFactor, curMax);
        speed *= friction;

        if (Mathf.Abs(hInput) > 0.01f && Mathf.Abs(speed) > 0.1f) {
            float turnDir = Mathf.Sign(speed);
            float ang = rb.rotation;
            ang += hInput * rudderMaxDegPerFrame * turnDir;
            rb.rotation = ang;
        }

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
        float magnitude = aimInput.magnitude;
        
        if (magnitude > minAimDistance) {
            aimDirection = aimInput.normalized;
            lastValidAimDirection = aimDirection;
        }
        else {
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
        if (cannonballPrefab == null || firePoint == null) {
            Debug.LogError($"[{playerName}] Cannonball prefab or fire point not assigned!");
            return;
        }

        bool isCritical = Random.value <= criticalChance;
        float finalDamage = isCritical ? damage * criticalMultiplier : damage;
        
        GameObject cannonball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
        
        Cannonball cbScript = cannonball.GetComponent<Cannonball>();
        if (cbScript == null)
            cbScript = cannonball.AddComponent<Cannonball>();
        
        cbScript.Initialize(aimDirection, cannonballSpeed, finalDamage, isCritical, this.gameObject);
        
        SpriteRenderer sr = cannonball.GetComponent<SpriteRenderer>();
        if (sr != null) {
            sr.color = isCritical ? criticalColor : normalColor;
        }

        Destroy(cannonball, cannonballLifetime);

        if (audioSource != null) {
            AudioClip clip = isCritical ? criticalSound : fireSound;
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

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

    #region FSM State Methods

    public string GetStateString()
    {
        switch (currentState) {
            case PlayerState.Idle: return "IDLE";
            case PlayerState.Moving: return "MOVING";
            case PlayerState.Boosting: return "BOOSTING";
            default: return "UNKNOWN";
        }
    }

    public Color GetStateColor()
    {
        switch (currentState) {
            case PlayerState.Idle: return Color.gray;
            case PlayerState.Moving: return Color.green;
            case PlayerState.Boosting: return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)aimDirection * 2f);
        
        if (firePoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }
    }

    #endregion
}
