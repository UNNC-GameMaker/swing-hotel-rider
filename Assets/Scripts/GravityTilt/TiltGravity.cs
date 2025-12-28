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
    }
}