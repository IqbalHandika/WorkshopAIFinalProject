using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor untuk JoystickInputMapper dengan button "Listen" untuk detect input
/// </summary>
[CustomPropertyDrawer(typeof(JoystickInputMapper))]
public class JoystickInputMapperDrawer : PropertyDrawer
{
    private static bool isListening = false;
    private static string listeningFor = "";
    private static SerializedProperty targetProperty = null;
    private static double listenStartTime = 0;
    private static readonly float listenTimeout = 5f; // 5 seconds timeout

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight + 2; // Foldout
        
        // Player Info section (no header for first section)
        height += GetFieldHeight(property, "playerName");
        height += GetFieldHeight(property, "joystickNumber");
        
        // Movement section (5 + 1 + 18 + 2 = 26)
        height += 26; // Header with separator
        height += GetFieldHeight(property, "moveHorizontalAxis");
        height += GetFieldHeight(property, "moveVerticalAxis");
        
        // Aim section
        height += 26; // Header with separator
        height += GetFieldHeight(property, "aimHorizontalAxis");
        height += GetFieldHeight(property, "aimVerticalAxis");
        
        // Boost & Throttle section
        height += 26; // Header with separator
        height += GetFieldHeight(property, "throttleTriggerAxis");
        height += GetFieldHeight(property, "boostTriggerAxis");
        height += GetFieldHeight(property, "triggerThreshold");
        
        // Shoot section
        height += 26; // Header with separator
        height += GetFieldHeight(property, "shootButton");
        height += GetFieldHeight(property, "shootKeyCode");
        height += GetFieldHeight(property, "boostKeyCode");
        
        // Deadzone section
        height += 26; // Header with separator
        height += GetFieldHeight(property, "analogDeadzone");
        
        return height;
    }
    
    private float GetFieldHeight(SerializedProperty parent, string propertyName)
    {
        SerializedProperty prop = parent.FindPropertyRelative(propertyName);
        if (prop == null) return 0;
        return EditorGUI.GetPropertyHeight(prop) + 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw default property
        EditorGUI.BeginProperty(position, label, property);
        
        // Foldout
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            float yOffset = EditorGUIUtility.singleLineHeight + 2;
            
            // Player Info
            DrawPropertyField(ref position, ref yOffset, property, "playerName");
            DrawPropertyField(ref position, ref yOffset, property, "joystickNumber");
            
            // Movement - Analog Kiri
            DrawHeaderWithSeparator(ref position, ref yOffset, "MOVEMENT - Analog Kiri");
            DrawAxisFieldWithListen(ref position, ref yOffset, property, "moveHorizontalAxis", "Move Horizontal (Analog L ←→)");
            DrawAxisFieldWithListen(ref position, ref yOffset, property, "moveVerticalAxis", "Move Vertical (Analog L ↑↓)");
            
            // Aim - Analog Kanan
            DrawHeaderWithSeparator(ref position, ref yOffset, "AIM - Analog Kanan");
            DrawAxisFieldWithListen(ref position, ref yOffset, property, "aimHorizontalAxis", "Aim Horizontal (Analog R ←→)");
            DrawAxisFieldWithListen(ref position, ref yOffset, property, "aimVerticalAxis", "Aim Vertical (Analog R ↑↓)");
            
            // Boost & Throttle - Triggers
            DrawHeaderWithSeparator(ref position, ref yOffset, "BOOST & THROTTLE - Triggers");
            DrawAxisFieldWithListen(ref position, ref yOffset, property, "throttleTriggerAxis", "Throttle (RT)");
            DrawAxisFieldWithListen(ref position, ref yOffset, property, "boostTriggerAxis", "Boost (LT)");
            DrawPropertyField(ref position, ref yOffset, property, "triggerThreshold");
            
            // Shoot - Button
            DrawHeaderWithSeparator(ref position, ref yOffset, "SHOOT - Button");
            DrawButtonFieldWithListen(ref position, ref yOffset, property, "shootButton", "Shoot Button");
            DrawPropertyField(ref position, ref yOffset, property, "shootKeyCode");
            DrawPropertyField(ref position, ref yOffset, property, "boostKeyCode");
            
            // Deadzone
            DrawHeaderWithSeparator(ref position, ref yOffset, "Deadzone Settings");
            DrawPropertyField(ref position, ref yOffset, property, "analogDeadzone");
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
        
        // Update listening state
        if (isListening)
        {
            // Check timeout
            if (EditorApplication.timeSinceStartup - listenStartTime > listenTimeout)
            {
                Debug.LogWarning($"[Input Mapper] Listen timeout for '{listeningFor}'");
                StopListening();
            }
            else
            {
                // Try to detect input
                DetectAndAssignInput(property);
            }
        }
    }

    private void DrawPropertyField(ref Rect position, ref float yOffset, SerializedProperty parent, string propertyName)
    {
        SerializedProperty prop = parent.FindPropertyRelative(propertyName);
        if (prop != null)
        {
            Rect rect = new Rect(position.x, position.y + yOffset, position.width, EditorGUI.GetPropertyHeight(prop));
            EditorGUI.PropertyField(rect, prop, true);
            yOffset += EditorGUI.GetPropertyHeight(prop) + 2;
        }
    }

    private void DrawHeaderWithSeparator(ref Rect position, ref float yOffset, string headerText)
    {
        yOffset += 5;
        
        // Draw separator line
        Rect lineRect = new Rect(position.x, position.y + yOffset, position.width, 1);
        EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        yOffset += 2;
        
        // Draw header text with background
        Rect headerRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.DrawRect(headerRect, new Color(0.3f, 0.3f, 0.3f, 0.2f));
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 11;
        headerStyle.normal.textColor = new Color(0.7f, 0.9f, 1f); // Light blue
        EditorGUI.LabelField(headerRect, "  " + headerText, headerStyle);
        
        yOffset += EditorGUIUtility.singleLineHeight + 2;
    }

    private void DrawAxisFieldWithListen(ref Rect position, ref float yOffset, SerializedProperty parent, string propertyName, string displayName)
    {
        SerializedProperty prop = parent.FindPropertyRelative(propertyName);
        if (prop == null) return;

        Rect rect = new Rect(position.x, position.y + yOffset, position.width - 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(rect, prop, new GUIContent(displayName));
        
        // Listen button
        Rect buttonRect = new Rect(position.x + position.width - 75, position.y + yOffset, 75, EditorGUIUtility.singleLineHeight);
        
        bool isThisListening = isListening && listeningFor == propertyName && targetProperty == prop;
        
        GUI.backgroundColor = isThisListening ? Color.yellow : Color.white;
        string buttonText = isThisListening ? "Listening..." : "🎮 Listen";
        
        if (GUI.Button(buttonRect, buttonText))
        {
            if (isThisListening)
            {
                StopListening();
            }
            else
            {
                StartListening(prop, propertyName, displayName);
            }
        }
        
        GUI.backgroundColor = Color.white;
        
        yOffset += EditorGUIUtility.singleLineHeight + 2;
    }

    private void DrawButtonFieldWithListen(ref Rect position, ref float yOffset, SerializedProperty parent, string propertyName, string displayName)
    {
        // Same as axis but for buttons
        DrawAxisFieldWithListen(ref position, ref yOffset, parent, propertyName, displayName);
    }

    private void StartListening(SerializedProperty prop, string propName, string displayName)
    {
        isListening = true;
        listeningFor = propName;
        targetProperty = prop;
        listenStartTime = EditorApplication.timeSinceStartup;
        
        Debug.Log($"<color=cyan>[Input Mapper]</color> Listening for '{displayName}'... Press button or move analog stick!");
        
        // Register update callback
        EditorApplication.update += UpdateListening;
    }

    private void StopListening()
    {
        isListening = false;
        listeningFor = "";
        targetProperty = null;
        
        EditorApplication.update -= UpdateListening;
    }

    private static void UpdateListening()
    {
        if (!isListening) return;

        // Repaint to show "Listening..." state
        SceneView.RepaintAll();
        
        // Force repaint Inspector
        EditorUtility.SetDirty(Selection.activeObject);
    }

    private void DetectAndAssignInput(SerializedProperty parent)
    {
        if (targetProperty == null) return;

        // Check all axes
        string[] axisNames = new string[]
        {
            "Horizontal", "Vertical",
            "Horizontal1", "Vertical1", "Horizontal2", "Vertical2",
            "RightStickHorizontal", "RightStickVertical",
            "RightStickHorizontal1", "RightStickVertical1",
            "RightStickHorizontal2", "RightStickVertical2",
            "Joy1Horizontal", "Joy1Vertical", "Joy2Horizontal", "Joy2Vertical",
            "3rd axis", "4th axis", "5th axis", "6th axis", "7th axis", "8th axis",
            "RT", "LT", "RT1", "LT1", "RT2", "LT2",
        };

        foreach (string axis in axisNames)
        {
            try
            {
                float value = Input.GetAxis(axis);
                if (Mathf.Abs(value) > 0.3f) // Significant movement
                {
                    targetProperty.stringValue = axis;
                    targetProperty.serializedObject.ApplyModifiedProperties();
                    
                    Debug.Log($"<color=green>[Input Mapper]</color> Assigned '{axis}' to '{listeningFor}' (value: {value:F2})");
                    
                    StopListening();
                    return;
                }
            }
            catch { }
        }

        // Check buttons
        for (int i = 0; i < 20; i++)
        {
            KeyCode key = (KeyCode)((int)KeyCode.JoystickButton0 + i);
            if (Input.GetKeyDown(key))
            {
                string buttonName = $"JoystickButton{i}";
                targetProperty.stringValue = buttonName;
                targetProperty.serializedObject.ApplyModifiedProperties();
                
                Debug.Log($"<color=green>[Input Mapper]</color> Assigned '{buttonName}' to '{listeningFor}'");
                
                StopListening();
                return;
            }
        }
    }
}
