using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KeyMovement2D : MonoBehaviour
{
    // FSM States
    public enum PlayerState {
        Idle,
        Moving,
        Boosting
    }

    [Header("Player Info")]
    public string playerName = "Player 1"; // "Player 1" atau "Player 2"
    
    [Header("Keys")]
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode boostKey = KeyCode.LeftShift;

    [Header("Movement")]
    public float maxSpeed = 6f;
    public float acceleration = 0.06f;
    [Range(0.90f, 0.999f)] public float friction = 0.995f;
    public float reverseFactor = 0.5f;
    public float rudderMaxDegPerFrame = 0.7f;

    [Header("Boost (1s)")]
    public float boostDuration = 1f;
    public float boostMaxSpeedMult = 1.6f;
    public float boostAccelMult = 1.8f;

    Rigidbody2D rb;
    float speed;
    float boostUntil = -1f;

    // FSM State
    private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;

    // Threshold untuk deteksi moving
    [Header("State Detection")]
    public float idleSpeedThreshold = 0.1f;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    void Update() {
        if (Input.GetKeyDown(boostKey))
            boostUntil = Time.time + boostDuration;
    }

    void FixedUpdate() {
        float ix = (Input.GetKey(left) ? -1 : 0) + (Input.GetKey(right) ? 1 : 0);
        float iy = (Input.GetKey(up) ? 1 : 0) + (Input.GetKey(down) ? -1 : 0);

        bool boosting = Time.time < boostUntil;
        float curAccel  = boosting ? acceleration * boostAccelMult : acceleration;
        float curMaxSpd = boosting ? maxSpeed    * boostMaxSpeedMult : maxSpeed;

        // Update FSM State
        UpdateState(boosting);

        // throttle / brake
        if (iy > 0) {
            speed += curAccel * (Time.fixedDeltaTime * 60f);
            if (speed > curMaxSpd) speed = curMaxSpd;
        } else if (iy < 0) {
            speed -= (curAccel * 0.7f) * (Time.fixedDeltaTime * 60f);
            float minBack = -curMaxSpd * reverseFactor;
            if (speed < minBack) speed = minBack;
        } else {
            speed *= Mathf.Pow(friction, Time.fixedDeltaTime * 60f);
            if (Mathf.Abs(speed) < 0.05f) speed = 0f;
        }

        // steer (udah dibenerin: -1 fix)
            if (Mathf.Abs(speed) > 0.05f && Mathf.Abs(ix) > 0f) {
            float steer = -ix; // dibalik arah steer
            if (speed < 0) steer = -steer;
            float rudderEffect = rudderMaxDegPerFrame * Mathf.Sqrt(Mathf.Abs(speed) / Mathf.Max(0.001f, curMaxSpd));
            float turnDeg = steer * rudderEffect * (Time.fixedDeltaTime * 60f);
            rb.MoveRotation(rb.rotation + turnDeg);
        }

    // arah maju dibalik: gunakan -transform.up sehingga input 'maju' mengarah sebaliknya
        Vector2 fwd = -transform.up;
        Vector2 v = fwd * speed;
        rb.MovePosition(rb.position + v * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }

    // ===== FSM STATE MANAGEMENT =====
    void UpdateState(bool isBoosting) {
        PlayerState previousState = currentState;

        // State transition logic
        if (isBoosting) {
            currentState = PlayerState.Boosting;
        }
        else if (Mathf.Abs(speed) > idleSpeedThreshold) {
            currentState = PlayerState.Moving;
        }
        else {
            currentState = PlayerState.Idle;
        }

        // Optional: Log state changes untuk debugging
        if (previousState != currentState) {
            OnStateChanged(previousState, currentState);
        }
    }

    void OnStateChanged(PlayerState from, PlayerState to) {
        // Debug.Log($"{playerName} state changed: {from} -> {to}");
        // Bisa ditambahkan event/callback untuk UI di sini
    }

    // Public method untuk get state sebagai string (untuk UI)
    public string GetStateString() {
        switch (currentState) {
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

    // Public method untuk get state color (untuk UI)
    public Color GetStateColor() {
        switch (currentState) {
            case PlayerState.Idle:
                return Color.gray;
            case PlayerState.Moving:
                return Color.green;
            case PlayerState.Boosting:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}
