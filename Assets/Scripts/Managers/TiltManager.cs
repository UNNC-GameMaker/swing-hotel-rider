using UnityEngine;

namespace Managers
{
    public class TiltManager : Manager
    {
        private void Update()
        {
            // Reset total tilt each frame
            TotalTilt = 0f;

            // Calculate total tilt from all tiltable objects
            foreach (var tiltObject in Tiltable.AllTiltables)
                TotalTilt += (tiltObject.transform.position.x - centerX) * tiltObject.Weight;

            // Smoothly interpolate to target tilt
            TiltTarget = Mathf.Lerp(TiltTarget, TotalTilt / tiltFactor, tiltSpeed * Time.deltaTime);
        }

        public override void Init()
        {
            Debug.Log("[TiltManager] Init");
            GameManager.Instance.RegisterManager(this);
        }

        #region Inspector Fields

        [SerializeField] [Tooltip("Tilt Coefficient")]
        private float tiltFactor = 100f;

        [SerializeField] [Tooltip("Tilt Speed")]
        private float tiltSpeed = 1f;

        [SerializeField] private float centerX = 10f;

        #endregion

        #region Public Properties

        public float TiltTarget { get; private set; }

        public float TotalTilt { get; private set; }

        #endregion

        #region Private Fields

        #endregion
    }
}