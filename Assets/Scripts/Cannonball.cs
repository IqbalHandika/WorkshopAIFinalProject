using UnityEngine;

/// <summary>
/// Script untuk cannonball projectile
/// Handle movement dan damage TANPA collider (manual detection)
/// Tidak pakai collider = tidak push kapal!
/// </summary>
public class Cannonball : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 20f;
    public bool isCritical = false;
    
    [Header("Movement")]
    public Vector2 direction;
    public float speed = 15f;

    [Header("Owner")]
    public GameObject owner; // Kapal yang nembak (tidak kena collision sendiri)

    [Header("Hit Detection (Manual)")]
    public float hitRadius = 0.3f;           // Radius untuk detect hit
    public LayerMask hitLayers;              // Layer yang bisa di-hit (Player, Enemy, etc)

    [Header("Visual Effects")]
    public GameObject hitEffectPrefab;       // Particle effect saat hit
    public GameObject criticalEffectPrefab;  // Special effect untuk critical
    
    [Header("Floating Damage Text")]
    public GameObject damageTextPrefab;      // Prefab floating damage text
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.red;

    [Header("Trail (Optional)")]
    public TrailRenderer trail;

    private bool hasHit = false;

    public void Initialize(Vector2 dir, float spd, float dmg, bool critical, GameObject shooter = null)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        isCritical = critical;
        owner = shooter;

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Setup trail if exists
        if (trail != null)
        {
            trail.startColor = isCritical ? Color.red : Color.white;
            trail.endColor = isCritical ? new Color(1, 0, 0, 0) : new Color(1, 1, 1, 0);
        }
    }

    void Update()
    {
        if (hasHit) return;

        // Manual movement (tanpa rigidbody)
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Manual hit detection (tanpa collider!)
        CheckHit();
    }

    void CheckHit()
    {
        // Manual hit detection pakai OverlapCircle (TANPA COLLIDER!)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, hitLayers);

        foreach (Collider2D hit in hits)
        {
            // Skip owner (kapal yang nembak)
            if (owner != null && hit.gameObject == owner)
                continue;

            // Check if hit player
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Apply damage
                playerHealth.TakeDamage(damage);

                Debug.Log($"Hit {playerHealth.playerName}! Damage: {damage} | Critical: {isCritical}");

                // Spawn hit effect
                SpawnHitEffect(hit.transform.position);
                
                // Spawn floating damage text
                SpawnFloatingDamageText(hit.transform.position);

                hasHit = true;
                DestroyCannonballImmediately();
                return; // Stop checking after first hit
            }

            // Check if hit CPU
            CPUHealthBar cpuHealth = hit.GetComponent<CPUHealthBar>();
            if (cpuHealth != null)
            {
                // Apply damage
                cpuHealth.TakeDamage(damage);

                Debug.Log($"Hit {cpuHealth.cpuName}! Damage: {damage} | Critical: {isCritical}");

                // Spawn hit effect
                SpawnHitEffect(hit.transform.position);
                
                // Spawn floating damage text
                SpawnFloatingDamageText(hit.transform.position);

                hasHit = true;
                DestroyCannonballImmediately();
                return; // Stop checking after first hit
            }
            // Jika hit obstacle/wall
            else if (hit.CompareTag("Obstacle") || hit.CompareTag("Wall"))
            {
                SpawnHitEffect(transform.position);
                hasHit = true;
                DestroyCannonballImmediately();
                return;
            }
        }
    }

    void SpawnHitEffect(Vector3 position)
    {
        GameObject effectPrefab = isCritical ? criticalEffectPrefab : hitEffectPrefab;
        
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Destroy effect after 2 seconds
        }
    }

    void SpawnFloatingDamageText(Vector3 position)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("Damage Text Prefab not assigned!");
            return;
        }

        // Spawn di posisi hit dengan offset sedikit ke atas
        Vector3 spawnPos = position + Vector3.up * 0.5f;
        GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        // Initialize floating text
        FloatingDamageText floatingText = textObj.GetComponent<FloatingDamageText>();
        if (floatingText != null)
        {
            floatingText.Initialize(damage, isCritical, normalDamageColor, criticalDamageColor);
            Debug.Log($"Spawned floating text: Damage={damage}, Critical={isCritical}");
        }
        else
        {
            Debug.LogError("FloatingDamageText component not found on prefab!");
        }
    }

    void DestroyCannonball()
    {
        // Disable collider untuk prevent multiple hits
        GetComponent<Collider2D>().enabled = false;
        
        // Disable trail
        if (trail != null)
            trail.emitting = false;

        // Destroy setelah delay kecil (biar effect keliatan)
        Destroy(gameObject, 0.1f);
    }

    void DestroyCannonballImmediately()
    {
        // Disable trail
        if (trail != null)
            trail.emitting = false;

        // Destroy IMMEDIATELY - tanpa collider, tanpa rigidbody, simple!
        Destroy(gameObject);
    }

    // Visual debug
    void OnDrawGizmos()
    {
        // Draw hit detection radius
        Gizmos.color = isCritical ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
        
        // Draw direction
        if (Application.isPlaying)
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction.normalized * 0.5f);
    }
}
