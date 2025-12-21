using UnityEngine;

namespace Managers
{
    public class TiltManager : Manager
    {
        #region Inspector Fields
        [SerializeField, Tooltip("Tilt Coefficient")]
        private float tiltFactor = 100f;
        
        [SerializeField, Tooltip("Tilt Speed")]
        private float tiltSpeed = 1f;
        
        [SerializeField]
        private float centerX = 10f;
        #endregion
        
        #region Public Properties
        public float TiltTarget => _tiltTarget;
        public float TotalTilt => _totalTilt;
        #endregion
        
        #region Private Fields
        private float _tiltTarget;
        private float _totalTilt;
        #endregion
        
        public override void Init()
        {
            Debug.Log("[TiltManager] Init");
            GameManager.Instance.RegisterManager(this);
        }

        private void Update()
        {
            // Reset total tilt each frame
            _totalTilt = 0f;
            
            // Calculate total tilt from all tiltable objects
            foreach (var tiltObject in Tiltable.AllTiltables)
            {
                _totalTilt += (tiltObject.transform.position.x - centerX) * tiltObject.Weight;
            }
            
            // Smoothly interpolate to target tilt
            _tiltTarget = Mathf.Lerp(_tiltTarget, _totalTilt / tiltFactor, tiltSpeed * Time.deltaTime);
        }
    }
}
