using UnityEngine;

/// <summary>
/// Helper script untuk visualisasi waypoint di Scene view
/// Attach ke GameObject waypoint untuk melihat posisi dan jalur
/// </summary>
public class WaypointVisualizer : MonoBehaviour {
    [Header("Gizmo Settings")]
    [Tooltip("Warna waypoint di Scene view")]
    public Color waypointColor = Color.cyan;
    
    [Tooltip("Ukuran lingkaran waypoint")]
    [Range(0.1f, 2f)]
    public float gizmoSize = 0.5f;
    
    [Tooltip("Tampilkan label nama waypoint?")]
    public bool showLabel = true;
    
    [Header("Path Visualization")]
    [Tooltip("Waypoint berikutnya dalam urutan (untuk visualisasi garis)")]
    public WaypointVisualizer nextWaypoint;
    
    [Tooltip("Warna garis ke waypoint berikutnya")]
    public Color pathLineColor = Color.yellow;
    
    [Tooltip("Tampilkan arah dengan arrow?")]
    public bool showDirection = true;
    
    void OnDrawGizmos() {
        // Draw waypoint sphere
        Gizmos.color = waypointColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.DrawSphere(transform.position, gizmoSize * 0.3f);
        
        // Draw label
        if(showLabel) {
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (gizmoSize + 0.3f), 
                gameObject.name,
                new GUIStyle() {
                    normal = new GUIStyleState() { textColor = waypointColor },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            );
            #endif
        }
        
        // Draw line to next waypoint
        if(nextWaypoint != null) {
            Gizmos.color = pathLineColor;
            Vector3 start = transform.position;
            Vector3 end = nextWaypoint.transform.position;
            
            Gizmos.DrawLine(start, end);
            
            // Draw direction arrow
            if(showDirection) {
                Vector3 direction = (end - start).normalized;
                Vector3 midPoint = (start + end) * 0.5f;
                
                // Arrow head
                Vector3 right = Vector3.Cross(Vector3.forward, direction).normalized * 0.3f;
                Vector3 arrowTip = midPoint + direction * 0.3f;
                
                Gizmos.DrawLine(arrowTip, midPoint - direction * 0.1f + right);
                Gizmos.DrawLine(arrowTip, midPoint - direction * 0.1f - right);
            }
        }
    }
    
    void OnDrawGizmosSelected() {
        // Draw larger sphere when selected
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.5f);
    }
}
