using System.Collections.Generic;
using UnityEngine;

namespace GravityTilt
{
    public class Tiltable : MonoBehaviour
    {
        #region static

        public static readonly List<Tiltable> AllTiltables = new();

        #endregion

        public Rigidbody2D Rb => rb;
        public float Weight => weight;

        private bool _isPlayer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            _isPlayer = GetComponent<PlayerMovement>() != null;
        }

        private void OnEnable()
        {
            if (!AllTiltables.Contains(this) && autoActivate) AllTiltables.Add(this);
        }

        private void OnDisable()
        {
            AllTiltables.Remove(this);
        }

        public void Activate()
        {
            AllTiltables.Add(this);
        }

        public void Update()
        {
            if(transform.position.y < -50 && !_isPlayer)
            {
                AllTiltables.Remove(this);
            }
        }

        #region private variables

        [SerializeField] private float weight = 1f;

        [SerializeField] private bool autoActivate;

        [SerializeField] private Rigidbody2D rb;

        #endregion
    }
}