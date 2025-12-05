using UnityEngine;
using TMPro;

/// <summary>
/// Helper script untuk tambah outline ke TextMeshPro
/// Attach langsung ke GameObject yang punya TextMeshProUGUI component
/// Atau bisa dipakai di Editor untuk quick setup
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextOutlineHelper : MonoBehaviour
{
    [Header("Outline Settings")]
    public bool applyOnStart = true;
    public Color textColor = Color.white;
    public Color outlineColor = Color.black;
    [Range(0.1f, 0.5f)]
    public float outlineWidth = 0.2f;
    public bool makeBold = true;

    [Header("Extra Shadow (Optional)")]
    public bool addUnderlay = false;
    public Vector2 shadowOffset = new Vector2(1, -1);

    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        if (applyOnStart)
        {
            ApplyOutline();
        }
    }

    [ContextMenu("Apply Outline Now")]
    public void ApplyOutline()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI component not found!");
            return;
        }

        // Set text color
        textComponent.color = textColor;

        // Set bold
        if (makeBold)
            textComponent.fontStyle = FontStyles.Bold;

        // Set outline
        textComponent.outlineColor = outlineColor;
        textComponent.outlineWidth = outlineWidth;

        // Optional underlay/shadow
        if (addUnderlay && textComponent.fontSharedMaterial != null)
        {
            textComponent.fontSharedMaterial.SetColor("_UnderlayColor", outlineColor);
            textComponent.fontSharedMaterial.SetFloat("_UnderlayOffsetX", shadowOffset.x);
            textComponent.fontSharedMaterial.SetFloat("_UnderlayOffsetY", shadowOffset.y);
            textComponent.fontSharedMaterial.SetFloat("_UnderlayDilate", 0.5f);
            textComponent.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0.1f);
        }

        // Force update
        textComponent.ForceMeshUpdate();

        Debug.Log($"Outline applied to: {gameObject.name}");
    }

    [ContextMenu("Reset to Default")]
    public void ResetToDefault()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        if (textComponent == null) return;

        textComponent.color = Color.white;
        textComponent.outlineColor = Color.black;
        textComponent.outlineWidth = 0.2f;
        textComponent.fontStyle = FontStyles.Bold;

        Debug.Log($"Reset to default: {gameObject.name}");
    }
}
