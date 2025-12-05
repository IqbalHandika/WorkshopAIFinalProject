using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Floating health bar untuk CPU yang muncul di atas kapal
/// Berbeda dari player yang healthbarnya di UI corner
/// </summary>
public class CPUHealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Floating Health Bar UI")]
    [Tooltip("Canvas dengan Render Mode = World Space")]
    public Canvas worldSpaceCanvas;
    
    [Tooltip("Slider untuk health bar")]
    public Slider healthBarSlider;
    
    [Tooltip("Fill image dari slider (untuk coloring)")]
    public Image sliderFillImage;
    
    [Tooltip("Text untuk show angka health (misal: 85/100)")]
    public TextMeshProUGUI healthText;
    
    [Tooltip("Text untuk show FSM state (misal: PATROL, ATTACK)")]
    public TextMeshProUGUI stateText;

    [Header("Health Bar Position")]
    [Tooltip("Offset dari posisi kapal (default: di atas kapal)")]
    public Vector3 healthBarOffset = new Vector3(0f, 1.5f, 0f);
    
    [Tooltip("Ukuran canvas world space (dalam world units, bukan pixels!)")]
    public Vector2 canvasSize = new Vector2(1f, 0.3f);
    
    [Tooltip("Scale canvas (adjust jika terlalu besar/kecil)")]
    [Range(0.001f, 0.1f)]
    public float canvasScale = 0.01f;

    [Header("Health Bar Colors")]
    public Color healthyColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    [Range(0f, 1f)] public float midHealthThreshold = 0.5f;
    [Range(0f, 1f)] public float lowHealthThreshold = 0.25f;

    [Header("CPU Info")]
    public string cpuName = "CPU 1";

    [Header("Camera Reference")]
    [Tooltip("Healthbar akan selalu menghadap camera ini")]
    public Camera mainCamera;

    // Events
    public System.Action<float> OnHealthChanged;
    public System.Action OnDeath;
    
    // AI Controller references (support both CPUController and AutoMovement)
    private CPUController cpuController;
    private AutoMovement autoMovement;

    void Awake()
    {
        // Auto setup camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Get AI controller references for FSM state display
        cpuController = GetComponent<CPUController>();
        autoMovement = GetComponent<AutoMovement>();

        // Auto create health bar jika belum ada
        if (worldSpaceCanvas == null)
        {
            CreateHealthBarUI();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void LateUpdate()
    {
        // Update posisi health bar di atas kapal
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.position = transform.position + healthBarOffset;

            // Buat health bar selalu menghadap camera
            if (mainCamera != null)
            {
                worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(
                    worldSpaceCanvas.transform.position - mainCamera.transform.position
                );
            }
        }
        
        // Update FSM state text
        UpdateStateDisplay();
    }

    /// <summary>
    /// Auto-create floating health bar UI
    /// </summary>
    void CreateHealthBarUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject($"{cpuName}_HealthBar");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = healthBarOffset;

        worldSpaceCanvas = canvasObj.AddComponent<Canvas>();
        worldSpaceCanvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100; // Higher = better quality

        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        // Set canvas size (pixels untuk world space canvas)
        RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200f, 50f); // Fixed pixel size
        
        // Set canvas scale untuk convert ke world units
        canvasRect.localScale = Vector3.one * canvasScale;

        // Create background panel
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.5f); // Semi-transparent black

        // Create Slider
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0.4f);
        sliderRect.anchorMax = new Vector2(0.95f, 0.7f);
        sliderRect.sizeDelta = Vector2.zero;

        healthBarSlider = sliderObj.AddComponent<Slider>();
        healthBarSlider.minValue = 0f;
        healthBarSlider.maxValue = 1f;
        healthBarSlider.value = 1f;

        // Create Background for slider
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray
        
        // Assign background ke slider (optional tapi bagus untuk consistency)
        healthBarSlider.targetGraphic = bgImage;

        // Create Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10f, -10f); // Padding 5px dari edge
        fillAreaRect.anchoredPosition = Vector2.zero;

        // Create Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.pivot = new Vector2(0f, 0.5f); // Pivot di kiri tengah (scale dari kiri)
        
        sliderFillImage = fillObj.AddComponent<Image>();
        sliderFillImage.color = healthyColor;
        sliderFillImage.type = Image.Type.Simple; // SIMPLE, bukan FILLED!

        // Assign fill rect ke slider
        healthBarSlider.fillRect = fillRect;
        healthBarSlider.transition = Selectable.Transition.None; // No transition effect

        // Create Health Text
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.7f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.sizeDelta = Vector2.zero;

        healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.text = $"{maxHealth}/{maxHealth}";
        healthText.fontSize = 18; // Smaller font
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;
        healthText.fontStyle = FontStyles.Bold;

        // Add outline untuk visibility
        healthText.outlineWidth = 0.15f;
        healthText.outlineColor = Color.black;
        
        // Enable auto-size jika text terlalu panjang
        healthText.enableAutoSizing = true;
        healthText.fontSizeMin = 10;
        healthText.fontSizeMax = 18;
        
        // Create FSM State Text (di sebelah health bar)
        GameObject stateTextObj = new GameObject("StateText");
        stateTextObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform stateTextRect = stateTextObj.AddComponent<RectTransform>();
        stateTextRect.anchorMin = new Vector2(0f, -0.5f); // Di bawah health bar
        stateTextRect.anchorMax = new Vector2(1f, 0f);
        stateTextRect.sizeDelta = Vector2.zero;
        
        stateText = stateTextObj.AddComponent<TextMeshProUGUI>();
        stateText.text = "PATROL";
        stateText.fontSize = 14;
        stateText.alignment = TextAlignmentOptions.Center;
        stateText.color = Color.black;
        stateText.fontStyle = FontStyles.Bold;
        stateText.outlineWidth = 0.15f;
        stateText.outlineColor = Color.black;
        stateText.enableAutoSizing = true;
        stateText.fontSizeMin = 8;
        stateText.fontSizeMax = 14;

        Debug.Log($"[{cpuName}] Auto-created floating health bar UI with FSM state display");
    }

    public void TakeDamage(float damage)
    {
        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth);

        float healthPercent = currentHealth / maxHealth;
        Debug.Log($"[{cpuName}] Took {damage} damage. Health: {oldHealth:F1} → {currentHealth:F1}/{maxHealth} ({healthPercent:P0}) | Slider value: {(healthBarSlider != null ? healthBarSlider.value.ToString("F2") : "NULL")}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth);
    }

    void UpdateHealthUI()
    {
        float healthPercent = currentHealth / maxHealth;

        // Update slider value
        if (healthBarSlider != null)
        {
            healthBarSlider.value = healthPercent;
        }

        // Update slider fill color
        if (sliderFillImage != null)
        {
            if (healthPercent <= lowHealthThreshold)
                sliderFillImage.color = lowHealthColor;
            else if (healthPercent <= midHealthThreshold)
                sliderFillImage.color = midHealthColor;
            else
                sliderFillImage.color = healthyColor;
        }

        // Update text dengan angka
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }

    void UpdateStateDisplay()
    {
        if (stateText == null) return;
        
        string state = "IDLE";
        Color stateColor = Color.gray;
        
        // Priority: AutoMovement (waypoint system) > CPUController
        if (autoMovement != null) {
            state = autoMovement.GetStateString();
            stateColor = autoMovement.GetStateColor();
        }
        else if (cpuController != null) {
            state = cpuController.GetStateString();
            stateColor = cpuController.GetStateColor();
        }
        
        stateText.text = state;
        stateText.color = stateColor;
    }

    void Die()
    {
        Debug.Log($"[{cpuName}] has been destroyed!");
        OnDeath?.Invoke();
        
        // Hide health bar saat mati
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(false);
        }

        // TODO: Implement death logic (explosion, disable AI, etc)
        // gameObject.SetActive(false);
    }

    // Public getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0f;

    // Editor helper
    void OnDrawGizmosSelected()
    {
        // Visualisasi posisi health bar di Scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + healthBarOffset, new Vector3(canvasSize.x, canvasSize.y, 0.1f));
    }
}
