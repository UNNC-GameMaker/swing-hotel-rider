using UnityEngine;

namespace Camera
{
    /// <summary>
    /// Keeps a child object as close to parent's localPosition (0,0,0) as possible,
    /// while ensuring it always remains visible within the orthographic camera viewport.
    /// The camera can be rotated arbitrarily; the algorithm uses Viewport coordinates
    /// to perform bounding box clipping without manually calculating width/height.
    /// </summary>
    [DisallowMultipleComponent]
    public class KeepInCam : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField, Tooltip("Target orthographic camera. Defaults to Camera.main if not specified.")]
        private UnityEngine.Camera targetCamera;

        [Header("Viewport Padding")]
        [SerializeField, Tooltip("Margin in viewport coordinates (0~0.5) to leave space around edges.")]
        private Vector2 viewportPadding = new Vector2(0.02f, 0.02f);

        [Header("Behavior")]
        [Tooltip("Enable/disable keeping object in camera view")]
        public bool keepInView = true;

        // Private state
        private Transform _parentTransform;

        void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }
            
            _parentTransform = transform.parent;
        }

        /// <summary>
        /// LateUpdate executes after all Update calls, ensuring other scripts have moved first
        /// </summary>
        void LateUpdate()
        {
            // Early exit if dependencies are missing
            if (_parentTransform == null || targetCamera == null)
            {
                return;
            }

            // If keeping in view is disabled, just reset to local origin
            if (!keepInView)
            {
                transform.localPosition = Vector3.zero;
                return;
            }

            // 1) Get parent's origin position in world coordinates
            Vector3 desiredWorldPosition = _parentTransform.position;

            // 2) Convert to Viewport coordinates: bottom-left (0,0), top-right (1,1)
            Vector3 viewportPosition = targetCamera.WorldToViewportPoint(desiredWorldPosition);

            // 3) Check if already visible within bounds (considering padding)
            bool isWithinXBounds = viewportPosition.x >= viewportPadding.x && 
                                   viewportPosition.x <= 1f - viewportPadding.x;
            bool isWithinYBounds = viewportPosition.y >= viewportPadding.y && 
                                   viewportPosition.y <= 1f - viewportPadding.y;
            bool isWithinDepthBounds = viewportPosition.z >= targetCamera.nearClipPlane && 
                                       viewportPosition.z <= targetCamera.farClipPlane;

            if (isWithinXBounds && isWithinYBounds && isWithinDepthBounds)
            {
                // Already visible, stay at local origin
                transform.localPosition = Vector3.zero;
                return;
            }

            // 4) Clamp X and Y to [padding, 1-padding], keep Z unchanged
            viewportPosition.x = Mathf.Clamp(viewportPosition.x, viewportPadding.x, 1f - viewportPadding.x);
            viewportPosition.y = Mathf.Clamp(viewportPosition.y, viewportPadding.y, 1f - viewportPadding.y);

            // 5) Convert clamped viewport position back to world coordinates
            Vector3 clampedWorldPosition = targetCamera.ViewportToWorldPoint(viewportPosition);

            // 6) Set world position to keep object in view
            transform.position = clampedWorldPosition;
            
            // Alternative: maintain local coordinate relationship with parent
            // transform.localPosition = _parentTransform.InverseTransformPoint(clampedWorldPosition);
        }
    }
}