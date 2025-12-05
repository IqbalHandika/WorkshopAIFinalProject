using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Floating damage text yang muncul saat hit (seperti game RPG)
/// Auto destroy setelah animasi selesai
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class FloatingDamageText : MonoBehaviour
{
    [Header("Text Settings")]
    public TextMeshPro textMesh;
    
    [Header("Animation")]
    public float floatSpeed = 2f;           // Kecepatan naik
    public float fadeSpeed = 1f;            // Kecepatan fade out
    public float lifetime = 1.5f;           // Total durasi
    public Vector2 randomOffset = new Vector2(0.5f, 0.5f); // Random spread

    [Header("Scale Animation")]
    public bool animateScale = true;
    public float startScale = 0.5f;
    public float endScale = 1.2f;
    public float scaleTime = 0.2f;

    private float timer = 0f;
    private Vector3 moveDirection;
    private Color startColor;

    void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();
        
        // Fallback jika masih null
        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component not found on FloatingDamageText!");
        }
    }

    void Start()
    {
        // Kalau Initialize tidak dipanggil (prefab issue), set default
        if (textMesh != null && textMesh.text == "99")
        {
            Debug.LogWarning("FloatingDamageText was not initialized! Using default values.");
            textMesh.text = "0";
            startColor = textMesh.color;
        }
    }

    public void Initialize(float damage, bool isCritical, Color normalColor, Color criticalColor)
    {
        // Safety check
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null)
            {
                Debug.LogError("Cannot initialize FloatingDamageText: TextMeshPro is null!");
                return;
            }
        }
        
        // Set text
        textMesh.text = Mathf.RoundToInt(damage).ToString();
        
        // Set color
        Color targetColor = isCritical ? criticalColor : normalColor;
        textMesh.color = targetColor;
        startColor = targetColor;
        
        Debug.Log($"FloatingText initialized: {textMesh.text} | Critical: {isCritical} | Color: {targetColor}");
        
        // Set font size based on critical
        textMesh.fontSize = isCritical ? 6f : 4f;
        
        // Bold untuk critical
        textMesh.fontStyle = isCritical ? FontStyles.Bold : FontStyles.Normal;
        
        // Random offset untuk spread
        float randomX = Random.Range(-randomOffset.x, randomOffset.x);
        float randomY = Random.Range(0, randomOffset.y);
        moveDirection = new Vector3(randomX, 1 + randomY, 0).normalized;
        
        // Scale animation
        if (animateScale)
        {
            transform.localScale = Vector3.one * startScale;
            StartCoroutine(ScaleAnimation());
        }
        
        // Auto destroy
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Float up
        transform.position += moveDirection * floatSpeed * Time.deltaTime;
        
        // Fade out
        float fadeProgress = timer / lifetime;
        Color color = startColor;
        color.a = Mathf.Lerp(1f, 0f, fadeProgress);
        textMesh.color = color;
    }

    IEnumerator ScaleAnimation()
    {
        float elapsed = 0f;
        
        while (elapsed < scaleTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleTime;
            float scale = Mathf.Lerp(startScale, endScale, progress);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        
        // Bounce back sedikit
        float bounceTime = 0.1f;
        elapsed = 0f;
        
        while (elapsed < bounceTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / bounceTime;
            float scale = Mathf.Lerp(endScale, 1f, progress);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }
}
