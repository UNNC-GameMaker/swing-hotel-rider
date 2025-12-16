using UnityEngine;

/// <summary>
/// Attach this to your Player to debug ground check issues
/// </summary>
public class GroundCheckDebugger : MonoBehaviour
{
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    public LayerMask groundLayer;
    
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public bool showGizmosInPlayMode = true;
    
    private void Update()
    {
        if (!showDebugInfo || groundCheck == null) return;
        
        // Check without layer mask
        Collider2D[] allColliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckSize, 0f);
        
        // Check with layer mask
        Collider2D[] groundColliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckSize, 0f, groundLayer);
        
        Debug.Log($"=== GROUND CHECK DEBUG ===");
        Debug.Log($"Position: {groundCheck.position}");
        Debug.Log($"Size: {groundCheckSize}");
        Debug.Log($"Layer Mask Value: {groundLayer.value}");
        Debug.Log($"Total colliders found: {allColliders.Length}");
        Debug.Log($"Ground layer colliders: {groundColliders.Length}");
        
        if (allColliders.Length > 0)
        {
            Debug.Log("--- All Colliders Found ---");
            foreach (var col in allColliders)
            {
                int layer = col.gameObject.layer;
                string layerName = LayerMask.LayerToName(layer);
                bool isInMask = ((1 << layer) & groundLayer.value) != 0;
                Debug.Log($"  • {col.name} | Layer: {layer} ({layerName}) | In Mask: {isInMask} | Trigger: {col.isTrigger}");
            }
        }
        
        if (groundColliders.Length > 0)
        {
            Debug.Log("--- Ground Layer Colliders ---");
            foreach (var col in groundColliders)
            {
                Debug.Log($"  ✓ {col.name} | Trigger: {col.isTrigger}");
            }
        }
        
        // Check if ground check is too far from ground
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 2f, groundLayer);
        if (hit.collider != null)
        {
            float distance = hit.distance;
            Debug.Log($"Ground found {distance:F3}m below. Box height is {groundCheckSize.y:F3}m");
            if (distance > groundCheckSize.y / 2f)
            {
                Debug.LogWarning($"⚠️ Ground is too far! Increase box height or lower ground check position.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No ground found within 2m below ground check!");
        }
        
        Debug.Log("=========================");
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmosInPlayMode || groundCheck == null) return;
        
        // Draw the check box
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        
        // Draw a raycast down
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(groundCheck.position, Vector2.down * 2f);
    }
}

