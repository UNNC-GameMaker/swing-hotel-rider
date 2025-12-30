using Managers;
using UnityEngine;

namespace GravityTilt
{
    public class Gravity : MonoBehaviour
    {
        [SerializeField] private float gravity = 2f;
        private TiltManager _manager;

        private void Start()
        {
            _manager = GameManager.Instance.GetManager<TiltManager>();
        }

        private void FixedUpdate()
        {
            // UnityEngine.Debug.Log(_manager.TiltTarget);
            var forceX = gravity * Mathf.Sin(_manager.TiltTarget * Mathf.Deg2Rad);
            var forceY = -gravity * Mathf.Cos(_manager.TiltTarget * Mathf.Deg2Rad) * 0.5f;

            var force = new Vector2(forceX, forceY);

            foreach (var tiltable in Tiltable.AllTiltables)
            {
                var rb = tiltable.Rb;
                if (rb) rb.AddForce(force * rb.mass, ForceMode2D.Impulse);
            }
        }

        private void OnDrawGizmos()
        {
            if (_manager == null) return;

            // Calculate current gravity direction
            var forceX = gravity * Mathf.Sin(_manager.TiltTarget * Mathf.Deg2Rad);
            var forceY = -gravity * Mathf.Cos(_manager.TiltTarget * Mathf.Deg2Rad) * 0.5f;
            var force = new Vector2(forceX, forceY);

            // Draw gravity direction at origin
            Vector3 origin = Vector3.zero;
            Vector3 direction = force.normalized * 3f; // Scale for visibility
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + direction);
            DrawArrowHead(origin + direction, direction.normalized, 0.5f);

            #if UNITY_EDITOR
            // Display gravity info
            UnityEditor.Handles.Label(origin + direction + Vector3.up * 0.5f, 
                $"Gravity Direction\nAngle: {_manager.TiltTarget:F1}°\nForce: ({forceX:F2}, {forceY:F2})");
            
            // Draw force arrows on each tiltable object
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
            foreach (var tiltable in Tiltable.AllTiltables)
            {
                if (tiltable != null && tiltable.Rb != null)
                {
                    Vector3 pos = tiltable.transform.position;
                    Vector3 forceVector = force * tiltable.Rb.mass * 0.5f; // Scale down for visibility
                    Gizmos.DrawLine(pos, pos + forceVector);
                    DrawArrowHead(pos + forceVector, forceVector.normalized, 0.3f);
                }
            }
            #endif
        }

        private void DrawArrowHead(Vector3 position, Vector3 direction, float size)
        {
            Vector3 right = new Vector3(-direction.y, direction.x, 0) * size;
            Vector3 left = new Vector3(direction.y, -direction.x, 0) * size;
            
            Gizmos.DrawLine(position, position - direction * size + right);
            Gizmos.DrawLine(position, position - direction * size + left);
        }
    }
}