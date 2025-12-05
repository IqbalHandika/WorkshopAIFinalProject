using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script untuk menampilkan state FSM player di UI
/// Attach ke GameObject UI yang ada Text/TextMeshPro component
/// </summary>
public class PlayerStateUI : MonoBehaviour
{
    [Header("References")]
    public KeyMovement2D playerMovement; // Drag player object yang ingin ditrack
    
    [Header("UI Components")]
    public TextMeshProUGUI stateText;    // Untuk TextMeshPro
    public Text legacyText;              // Untuk Legacy Text (jika tidak pakai TMP)
    public Image stateIndicator;         // Optional: Image untuk color indicator

    [Header("Display Settings")]
    public string labelPrefix = "P1: "; // "P1: " atau "P2: "
    public bool showColorIndicator = true;
    public bool updateEveryFrame = true;
    
    [Header("Text Styling")]
    public bool useWhiteText = true;     // Text putih dengan outline hitam
    public bool addOutline = true;       // Tambah outline otomatis
    public Color outlineColor = Color.black;
    public float outlineWidth = 0.2f;

    void Start()
    {
        SetupTextStyling();
    }

    void Update()
    {
        if (updateEveryFrame && playerMovement != null)
        {
            UpdateStateDisplay();
        }
    }

    void SetupTextStyling()
    {
        // Setup TextMeshPro styling
        if (stateText != null)
        {
            if (useWhiteText)
            {
                stateText.color = Color.white;
            }
            
            if (addOutline)
            {
                stateText.outlineColor = outlineColor;
                stateText.outlineWidth = outlineWidth;
            }
            
            // Set font weight ke Bold untuk lebih tebal
            stateText.fontStyle = FontStyles.Bold;
        }

        // Setup Legacy Text styling (jika dipakai)
        if (legacyText != null)
        {
            if (useWhiteText)
            {
                legacyText.color = Color.white;
            }
            
            // Legacy Text pakai Outline component
            if (addOutline)
            {
                var outline = legacyText.GetComponent<Outline>();
                if (outline == null)
                    outline = legacyText.gameObject.AddComponent<Outline>();
                
                outline.effectColor = outlineColor;
                outline.effectDistance = new Vector2(1, -1);
            }
        }
    }

    void UpdateStateDisplay()
    {
        if (playerMovement == null) return;

        string stateString = labelPrefix + playerMovement.GetStateString();
        Color stateColor = playerMovement.GetStateColor();

        // Update TextMeshPro
        if (stateText != null)
        {
            stateText.text = stateString;
            
            // Jika useWhiteText = false, baru pakai state color
            if (showColorIndicator && !useWhiteText)
                stateText.color = stateColor;
        }

        // Update Legacy Text (fallback)
        if (legacyText != null)
        {
            legacyText.text = stateString;
            
            if (showColorIndicator && !useWhiteText)
                legacyText.color = stateColor;
        }

        // Update Image indicator (optional) - ini tetap pakai color
        if (stateIndicator != null && showColorIndicator)
        {
            stateIndicator.color = stateColor;
        }
    }

    // Method untuk dipanggil dari external script jika perlu
    public void ForceUpdate()
    {
        UpdateStateDisplay();
    }
}
