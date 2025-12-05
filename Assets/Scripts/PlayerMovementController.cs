using UnityEngine;

/// <summary>
/// Player movement dengan support Keyboard DAN Joystick
/// FSM: Idle → Moving → Boosting
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    // FSM States
    public enum PlayerState {
        Idle,
        Moving,
        Boosting
    }

    [Header("Player Info")]
    public string playerName = "Player 1";
    
    [Header("━━━━━ INPUT MODE ━━━━━")]
    [Tooltip("TRUE = Keyboard, FALSE = Joystick")]
    public bool useKeyboard = true;
    
    [Header("Keyboard Input")]
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode boostKey = KeyCode.LeftShift;

    [Header("Joystick Input")]
    public JoystickInputMapper joystickInput;

    [Header("Movement Settings")]
    public float maxSpeed = 6f;
    public float acceleration = 0.06f;
    [Range(0.90f, 0.999f)] public float friction = 0.995f;
    public float reverseFactor = 0.5f;
    public float rudderMaxDegPerFrame = 0.7f;

    [Header("Boost Settings")]
    public float boostDuration = 1f;
    public float boostMaxSpeedMult = 1.6f;
    public float boostAccelMult = 1.8f;

    [Header("State Detection")]
    public float idleSpeedThreshold = 0.1f;

    // Private
    private Rigidbody2D rb;
    private float speed;
    private float boostUntil = -1f;
    private PlayerState currentState = PlayerState.Idle;
    
    public PlayerState CurrentState => currentState;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Boost input
        if (GetBoostInput())
        {
            boostUntil = Time.time + boostDuration;
        }
    }

    void FixedUpdate()
    {
        bool isBoosting = Time.time < boostUntil;
        float curMax = isBoosting ? maxSpeed * boostMaxSpeedMult : maxSpeed;
        float curAccel = isBoosting ? acceleration * boostAccelMult : acceleration;

        // Get input
        float vInput = GetVerticalInput();
        float hInput = GetHorizontalInput();

        // Determine state based on boost and movement
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
        Vector2 forward = new Vector2(Mathf.Sin(rb.rotation * Mathf.Deg2Rad),
                                      -Mathf.Cos(rb.rotation * Mathf.Deg2Rad));
        rb.linearVelocity = forward * speed;
    }

    #region Input Methods

    private float GetVerticalInput()
    {
        if (useKeyboard)
        {
            float v = 0f;
            if (Input.GetKey(up)) v += 1f;
            if (Input.GetKey(down)) v -= 1f;
            return v;
        }
        else
        {
            // Joystick: bisa pakai analog atau trigger
            float analog = joystickInput.GetMoveVertical();
            float trigger = joystickInput.GetThrottle(); // RT for forward throttle
            
            // Prioritize trigger if moved, otherwise use analog
            if (Mathf.Abs(trigger) > 0.01f)
                return trigger;
            else
                return analog;
        }
    }

    private float GetHorizontalInput()
    {
        if (useKeyboard)
        {
            float h = 0f;
            if (Input.GetKey(left)) h -= 1f;
            if (Input.GetKey(right)) h += 1f;
            return h;
        }
        else
        {
            return joystickInput.GetMoveHorizontal();
        }
    }

    private bool GetBoostInput()
    {
        if (useKeyboard)
        {
            return Input.GetKeyDown(boostKey);
        }
        else
        {
            // LT for boost
            float boost = joystickInput.GetBoost();
            return boost > 0.5f; // Trigger pulled
        }
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
}
