using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Master UI Manager untuk Game Scene
/// Manages health bars dan state displays untuk kedua player
/// Setup UI hierarchy di Inspector
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("Player 1 UI (Top Left)")]
    public PlayerHealth player1Health;
    public KeyMovement2D player1Movement;
    public Slider player1HealthBarSlider;
    public Image player1SliderFill;
    public TextMeshProUGUI player1HealthText;
    public TextMeshProUGUI player1StateText;
    public Image player1StateIndicator;

    [Header("Player 2 UI (Top Right)")]
    public PlayerHealth player2Health;
    public KeyMovement2D player2Movement;
    public Slider player2HealthBarSlider;
    public Image player2SliderFill;
    public TextMeshProUGUI player2HealthText;
    public TextMeshProUGUI player2StateText;
    public Image player2StateIndicator;

    [Header("CPU/AI UI (Optional)")]
    public PlayerHealth cpu1Health;
    public AutoMovement cpu1Movement;
    public TextMeshProUGUI cpu1StateText;
    public Image cpu1StateIndicator;

    public PlayerHealth cpu2Health;
    public AutoMovement cpu2Movement;
    public TextMeshProUGUI cpu2StateText;
    public Image cpu2StateIndicator;

    [Header("Settings")]
    public bool autoAssignReferences = true;
    public bool showCPUStates = true;
    
    [Header("Text Styling")]
    public bool useWhiteTextWithOutline = true;
    public Color textOutlineColor = Color.black;
    [Range(0.1f, 0.5f)]
    public float textOutlineWidth = 0.2f;
    public bool useUnderlay = false; // Alternatif: pakai underlay untuk shadow effect

    void Start()
    {
        if (autoAssignReferences)
        {
            // Auto-assign player health references to UI
            if (player1Health != null)
            {
                player1Health.healthBarSlider = player1HealthBarSlider;
                player1Health.sliderFillImage = player1SliderFill;
                player1Health.healthText = player1HealthText;
            }

            if (player2Health != null)
            {
                player2Health.healthBarSlider = player2HealthBarSlider;
                player2Health.sliderFillImage = player2SliderFill;
                player2Health.healthText = player2HealthText;
            }
        }

        // Setup text styling untuk semua text
        if (useWhiteTextWithOutline)
        {
            SetupTextOutline(player1HealthText);
            SetupTextOutline(player1StateText);
            SetupTextOutline(player2HealthText);
            SetupTextOutline(player2StateText);
            SetupTextOutline(cpu1StateText);
            SetupTextOutline(cpu2StateText);
        }
    }

    void SetupTextOutline(TextMeshProUGUI text)
    {
        if (text == null) 
        {
            Debug.LogWarning("SetupTextOutline: Text reference is null!");
            return;
        }
        
        // Set text color ke putih
        text.color = Color.white;
        
        // Set font ke Bold
        text.fontStyle = FontStyles.Bold;
        
        // Add outline
        text.outlineColor = textOutlineColor;
        text.outlineWidth = textOutlineWidth;
        
        // Alternatif: pakai underlay jika outline kurang jelas
        if (useUnderlay)
        {
            text.fontSharedMaterial.SetColor("_UnderlayColor", textOutlineColor);
            text.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 0.5f);
            text.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -0.5f);
            text.fontSharedMaterial.SetFloat("_UnderlayDilate", 0.5f);
            text.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0.1f);
        }
        
        // Force update material
        text.ForceMeshUpdate();
        
        Debug.Log($"Outline setup untuk: {text.gameObject.name} - Width: {textOutlineWidth}");
    }
    
    // Method untuk re-apply outline (bisa dipanggil dari Inspector atau code)
    [ContextMenu("Force Reapply Text Outlines")]
    public void ForceReapplyOutlines()
    {
        if (useWhiteTextWithOutline)
        {
            SetupTextOutline(player1HealthText);
            SetupTextOutline(player1StateText);
            SetupTextOutline(player2HealthText);
            SetupTextOutline(player2StateText);
            SetupTextOutline(cpu1StateText);
            SetupTextOutline(cpu2StateText);
        }
        Debug.Log("All text outlines reapplied!");
    }

    void Update()
    {
        UpdatePlayerStates();
    }

    void UpdatePlayerStates()
    {
        // Update Player 1 State
        if (player1Movement != null && player1StateText != null)
        {
            player1StateText.text = "P1: " + player1Movement.GetStateString();
            
            // Jika tidak pakai white text, baru update color
            if (!useWhiteTextWithOutline)
                player1StateText.color = player1Movement.GetStateColor();
            
            if (player1StateIndicator != null)
                player1StateIndicator.color = player1Movement.GetStateColor();
        }

        // Update Player 2 State
        if (player2Movement != null && player2StateText != null)
        {
            player2StateText.text = "P2: " + player2Movement.GetStateString();
            
            if (!useWhiteTextWithOutline)
                player2StateText.color = player2Movement.GetStateColor();
            
            if (player2StateIndicator != null)
                player2StateIndicator.color = player2Movement.GetStateColor();
        }

        // Update CPU States (jika enabled)
        if (showCPUStates)
        {
            // CPU 1 State
            if (cpu1Movement != null && cpu1StateText != null)
            {
                cpu1StateText.text = cpu1Movement.aiName + ": " + cpu1Movement.GetStateString();
                
                if (!useWhiteTextWithOutline)
                    cpu1StateText.color = cpu1Movement.GetStateColor();
                
                if (cpu1StateIndicator != null)
                    cpu1StateIndicator.color = cpu1Movement.GetStateColor();
            }

            // CPU 2 State
            if (cpu2Movement != null && cpu2StateText != null)
            {
                cpu2StateText.text = cpu2Movement.aiName + ": " + cpu2Movement.GetStateString();
                
                if (!useWhiteTextWithOutline)
                    cpu2StateText.color = cpu2Movement.GetStateColor();
                
                if (cpu2StateIndicator != null)
                    cpu2StateIndicator.color = cpu2Movement.GetStateColor();
            }
        }
    }

    // Helper methods untuk test/debug
    public void TestDamagePlayer1(float damage)
    {
        if (player1Health != null)
            player1Health.TakeDamage(damage);
    }

    public void TestDamagePlayer2(float damage)
    {
        if (player2Health != null)
            player2Health.TakeDamage(damage);
    }
}
