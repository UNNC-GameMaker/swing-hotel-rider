using Cinemachine;
using GravityTilt;
using UnityEditor;
using UnityEngine;

namespace Managers
{
    public class TiltManager : Manager
    {
        [SerializeField] private CinemachineVirtualCamera playerCamera;

        #region Private Properties

        private float _totalTilt;

        #endregion

        #region Public Properties

        public float TiltTarget { get; private set; }

        #endregion

        private void Update()
        {
            //Debug.Log("[TiltManager] _totalTilt: " + TiltTarget);
            if (GameManager.Instance.gameState == GameState.Playing)
            {
                TiltObject();

                TiltCamera();
            }
        }

        #region Debug

        private void OnDrawGizmos()
        {
            // Draw vertical line at centerX position
            Gizmos.color = Color.yellow;
            var topPoint = new Vector3(centerX, 20f, 0f);
            var bottomPoint = new Vector3(centerX, -20f, 0f);
            Gizmos.DrawLine(bottomPoint, topPoint);

            // Draw a label
#if UNITY_EDITOR
            Handles.Label(new Vector3(centerX, 10f, 0f), $"Tilt Center X: {centerX}");
#endif
        }

        #endregion

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
            if (playerCamera) playerCamera.m_Lens.Dutch = TiltTarget;
        }

        public override void Init()
        {
            UnityEngine.Debug.Log("[TiltManager] Init");
            GameManager.Instance.RegisterManager(this);
        }

        #region Inspector Fields

        [SerializeField] [Tooltip("Tilt Coefficient")]
        private float tiltFactor = 100f;

        [SerializeField] [Tooltip("Tilt Speed")]
        private float tiltSpeed = 1f;

        [SerializeField] private float centerX = 10f;

        #endregion
    }
}