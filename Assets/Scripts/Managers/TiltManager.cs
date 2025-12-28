using Cinemachine;
using GravityTilt;
using UnityEngine;

namespace Managers
{
    public class TiltManager : Manager
    {
        [SerializeField] private CinemachineVirtualCamera playerCamera;   
        void Update()
        {
            //Debug.Log("[TiltManager] _totalTilt: " + TiltTarget);

            TiltObject();
            
            TiltCamera();
        }

        private void TiltObject()
        {
            // Reset total tilt each frame
            _totalTilt = 0f;

            // Calculate total tilt from all tiltable objects
            foreach (var tiltObject in Tiltable.AllTiltables)
                _totalTilt += (tiltObject.transform.position.x - centerX) * tiltObject.Weight;

            // Smoothly interpolate to target tilt
            TiltTarget = Mathf.Lerp(TiltTarget, _totalTilt / tiltFactor, tiltSpeed * Time.deltaTime);
        }

        private void TiltCamera()
        {
            if (playerCamera)
            {
                playerCamera.m_Lens.Dutch = _totalTilt;
            }
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
        #endregion
        
        #region Private Properties

        private float _totalTilt;
        
        #endregion

    }
}