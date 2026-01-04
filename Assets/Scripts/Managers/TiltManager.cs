using System;
using Cinemachine;
using GravityTilt;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class TiltManager : Manager
    {
        [SerializeField] private CinemachineVirtualCamera playerCamera;

        #region Private Properties

        private float _totalTilt;
        private Image _mask;
        private Animator _animator;

        #endregion

        #region Public Properties

        public float TiltTarget { get; private set; }

        #endregion

        private void Start()
        {
            _mask = GameObject.Find("ColorMask").GetComponent<Image>();
            _animator = _mask.GetComponent<Animator>();
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            //Debug.Log("[TiltManager] _totalTilt: " + TiltTarget);
            if (GameManager.Instance.gameState == GameState.Playing)
            {
                TiltObject();

                TiltCamera();
            }

            if (_mask)
            {
                if (Math.Abs(TiltTarget) < greenAngle)
                {
                    _mask.color = none;
                    _animator.SetBool("enable", false);
                } 
                else if (Math.Abs(TiltTarget) < yellowAngle)
                {
                    _mask.color = yellow;
                    _animator.SetBool("enable", true);
                }
                else
                {
                    _mask.color = red;
                    _animator.SetBool("enable", true);
                }
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

        [SerializeField] private Color none =  new (0f, 0f, 0f, 0f);
        [SerializeField] private Color green;
        [SerializeField] private Color yellow;
        [SerializeField] private Color red;

        [SerializeField] private float greenAngle;
        [SerializeField] private float yellowAngle;
        [SerializeField] private float redAngle;

        #endregion
    }
}