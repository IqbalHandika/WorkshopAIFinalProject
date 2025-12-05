using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script untuk manage health player dan UI health bar
/// Nantinya bisa diintegrasikan dengan shooting system
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI References - Slider")]
    public Slider healthBarSlider;       // Slider untuk health bar
    public Image sliderFillImage;        // Fill area dari slider (untuk coloring)
    public TextMeshProUGUI healthText;   // Text untuk show "100/100"
    public Text legacyHealthText;        // Legacy UI Text fallback

    [Header("Health Bar Colors")]
    public Color healthyColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    [Range(0f, 1f)] public float midHealthThreshold = 0.5f;
    [Range(0f, 1f)] public float lowHealthThreshold = 0.25f;

    [Header("Player Info")]
    public string playerName = "Player 1";

    // Events (untuk nantinya)
    public System.Action<float> OnHealthChanged;
    public System.Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth);

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
            // Update color based on health percentage
            if (healthPercent <= lowHealthThreshold)
                sliderFillImage.color = lowHealthColor;
            else if (healthPercent <= midHealthThreshold)
                sliderFillImage.color = midHealthColor;
            else
                sliderFillImage.color = healthyColor;
        }

        // Update text
        string healthString = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        
        if (healthText != null)
            healthText.text = healthString;
        
        if (legacyHealthText != null)
            legacyHealthText.text = healthString;
    }

    void Die()
    {
        Debug.Log($"{playerName} has been destroyed!");
        OnDeath?.Invoke();
        
        // Implement death logic (disable controls, show explosion, etc)
        // gameObject.SetActive(false); // atau logic lain
    }

    // Public getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0f;
}
