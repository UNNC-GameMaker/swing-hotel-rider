using UnityEngine;

namespace Player
{
    /// <summary>
    ///     Attach this to your Player to debug ground check issues
    /// </summary>
    public class GroundCheckDebugger : MonoBehaviour
    {
        public Transform groundCheck;
        public Vector2 groundCheckSize = new(0.8f, 0.1f);
        public LayerMask groundLayer;

        [Header("Debug Settings")] public bool showDebugInfo = true;

        public bool showGizmosInPlayMode = true;

        private void Update()
        {
            if (!showDebugInfo || groundCheck == null) return;

            // Check without layer mask
            var allColliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckSize, 0f);

            // Check with layer mask
            var groundColliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckSize, 0f, groundLayer);

            UnityEngine.Debug.Log("=== GROUND CHECK DEBUG ===");
            UnityEngine.Debug.Log($"Position: {groundCheck.position}");
            UnityEngine.Debug.Log($"Size: {groundCheckSize}");
            UnityEngine.Debug.Log($"Layer Mask Value: {groundLayer.value}");
            UnityEngine.Debug.Log($"Total colliders found: {allColliders.Length}");
            UnityEngine.Debug.Log($"Ground layer colliders: {groundColliders.Length}");

            if (allColliders.Length > 0)
            {
                UnityEngine.Debug.Log("--- All Colliders Found ---");
                foreach (var col in allColliders)
                {
                    var layer = col.gameObject.layer;
                    var layerName = LayerMask.LayerToName(layer);
                    var isInMask = ((1 << layer) & groundLayer.value) != 0;
                    UnityEngine.Debug.Log(
                        $"  • {col.name} | Layer: {layer} ({layerName}) | In Mask: {isInMask} | Trigger: {col.isTrigger}");
                }
            }

            if (groundColliders.Length > 0)
            {
                UnityEngine.Debug.Log("--- Ground Layer Colliders ---");
                foreach (var col in groundColliders) UnityEngine.Debug.Log($"  ✓ {col.name} | Trigger: {col.isTrigger}");
            }

            // Check if ground check is too far from ground
            var hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 2f, groundLayer);
            if (hit.collider != null)
            {
                var distance = hit.distance;
                UnityEngine.Debug.Log($"Ground found {distance:F3}m below. Box height is {groundCheckSize.y:F3}m");
                if (distance > groundCheckSize.y / 2f)
                    UnityEngine.Debug.LogWarning("⚠️ Ground is too far! Increase box height or lower ground check position.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("⚠️ No ground found within 2m below ground check!");
            }

            UnityEngine.Debug.Log("=========================");
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
}