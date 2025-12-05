using UnityEngine;

/// <summary>
/// Input remapping system untuk joystick dengan button detection
/// Klik button "Listen" di Inspector untuk detect input dari joystick
/// Support 2 joystick berbeda untuk P1 dan P2
/// </summary>
[System.Serializable]
public class JoystickInputMapper
{
    [Header("Player Info")]
    public string playerName = "Player 1";
    public int joystickNumber = 1; // 1 = Joystick 1, 2 = Joystick 2

    [Header("━━━ MOVEMENT - Analog Kiri ━━━")]
    [Tooltip("Horizontal axis untuk steering (analog kiri)")]
    public string moveHorizontalAxis = "Horizontal";
    
    [Tooltip("Vertical axis untuk throttle (analog kiri)")]
    public string moveVerticalAxis = "Vertical";

    [Header("━━━ AIM - Analog Kanan ━━━")]
    [Tooltip("Horizontal axis untuk aim (analog kanan)")]
    public string aimHorizontalAxis = "RightStickHorizontal";
    
    [Tooltip("Vertical axis untuk aim (analog kanan)")]
    public string aimVerticalAxis = "RightStickVertical";

    [Header("━━━ BOOST & THROTTLE - Triggers ━━━")]
    [Tooltip("RT (Right Trigger) untuk throttle")]
    public string throttleTriggerAxis = "RT";
    
    [Tooltip("LT (Left Trigger) untuk boost")]
    public string boostTriggerAxis = "LT";
    
    [Tooltip("Threshold untuk trigger (0-1)")]
    [Range(0f, 1f)]
    public float triggerThreshold = 0.1f;

    [Header("━━━ SHOOT - Button ━━━")]
    [Tooltip("Button untuk shoot (biasanya RB/R1)")]
    public string shootButton = "Fire1";
    
    [Header("━━━ ALTERNATE: Keyboard Input Names ━━━")]
    [Tooltip("Untuk joystick yang pakai KeyCode naming")]
    public KeyCode shootKeyCode = KeyCode.JoystickButton5; // RB on Xbox
    public KeyCode boostKeyCode = KeyCode.JoystickButton4;  // LB on Xbox

    [Header("Deadzone Settings")]
    [Range(0f, 0.9f)]
    public float analogDeadzone = 0.15f;

    // Runtime listening state
    [System.NonSerialized]
    public bool isListening = false;
    [System.NonSerialized]
    public string listeningFor = "";

    /// <summary>
    /// Get horizontal movement input (analog kiri X)
    /// </summary>
    public float GetMoveHorizontal()
    {
        float value = GetAxisWithJoystickNumber(moveHorizontalAxis);
        return Mathf.Abs(value) > analogDeadzone ? value : 0f;
    }

    /// <summary>
    /// Get vertical movement input (analog kiri Y)
    /// </summary>
    public float GetMoveVertical()
    {
        float value = GetAxisWithJoystickNumber(moveVerticalAxis);
        return Mathf.Abs(value) > analogDeadzone ? value : 0f;
    }

    /// <summary>
    /// Get horizontal aim input (analog kanan X)
    /// </summary>
    public float GetAimHorizontal()
    {
        float value = GetAxisWithJoystickNumber(aimHorizontalAxis);
        return Mathf.Abs(value) > analogDeadzone ? value : 0f;
    }

    /// <summary>
    /// Get vertical aim input (analog kanan Y)
    /// </summary>
    public float GetAimVertical()
    {
        float value = GetAxisWithJoystickNumber(aimVerticalAxis);
        return Mathf.Abs(value) > analogDeadzone ? value : 0f;
    }

    /// <summary>
    /// Get throttle trigger value (RT)
    /// </summary>
    public float GetThrottle()
    {
        float value = GetAxisWithJoystickNumber(throttleTriggerAxis);
        return value > triggerThreshold ? value : 0f;
    }

    /// <summary>
    /// Get boost trigger value (LT)
    /// </summary>
    public float GetBoost()
    {
        float value = GetAxisWithJoystickNumber(boostTriggerAxis);
        return value > triggerThreshold ? value : 0f;
    }

    /// <summary>
    /// Get shoot button press
    /// </summary>
    public bool GetShootButton()
    {
        // Try button name first
        try
        {
            if (Input.GetButton(shootButton))
                return true;
        }
        catch { }

        // Try KeyCode fallback
        if (Input.GetKey(shootKeyCode))
            return true;

        return false;
    }

    /// <summary>
    /// Get shoot button down (press)
    /// </summary>
    public bool GetShootButtonDown()
    {
        // Try button name first
        try
        {
            if (Input.GetButtonDown(shootButton))
                return true;
        }
        catch { }

        // Try KeyCode fallback
        if (Input.GetKeyDown(shootKeyCode))
            return true;

        return false;
    }

    /// <summary>
    /// Helper: Get axis value dengan joystick number suffix
    /// Contoh: "Horizontal" + joystick 1 = coba "Horizontal", "Horizontal1", "Joy1Horizontal"
    /// </summary>
    private float GetAxisWithJoystickNumber(string axisName)
    {
        if (string.IsNullOrEmpty(axisName))
            return 0f;

        // Try variations
        string[] variations = new string[]
        {
            axisName,                                    // "Horizontal"
            axisName + joystickNumber.ToString(),       // "Horizontal1"
            "Joy" + joystickNumber + axisName,          // "Joy1Horizontal"
            $"Joystick{joystickNumber}{axisName}",     // "Joystick1Horizontal"
        };

        foreach (string variation in variations)
        {
            try
            {
                float value = Input.GetAxis(variation);
                if (Mathf.Abs(value) > 0.001f) // If we get non-zero, this is the right axis
                    return value;
            }
            catch
            {
                // Axis not found, try next
                continue;
            }
        }

        return 0f;
    }

    /// <summary>
    /// Detect any input dari joystick ini
    /// Returns axis name atau button name yang di-press
    /// </summary>
    public string DetectInput()
    {
        // Check all possible axes
        string[] axisNames = new string[]
        {
            "Horizontal", "Vertical",
            "Horizontal" + joystickNumber, "Vertical" + joystickNumber,
            "RightStickHorizontal", "RightStickVertical",
            "RightStickHorizontal" + joystickNumber, "RightStickVertical" + joystickNumber,
            "Joy" + joystickNumber + "Horizontal", "Joy" + joystickNumber + "Vertical",
            "3rd axis", "4th axis", "5th axis", "6th axis",
            "RT", "LT", "RT" + joystickNumber, "LT" + joystickNumber,
        };

        foreach (string axis in axisNames)
        {
            try
            {
                float value = Input.GetAxis(axis);
                if (Mathf.Abs(value) > 0.3f) // Significant movement
                {
                    return axis;
                }
            }
            catch { }
        }

        // Check buttons (0-19 typical)
        for (int i = 0; i < 20; i++)
        {
            KeyCode key = (KeyCode)((int)KeyCode.JoystickButton0 + i);
            if (Input.GetKeyDown(key))
            {
                return $"JoystickButton{i}";
            }
        }

        return null;
    }
}
