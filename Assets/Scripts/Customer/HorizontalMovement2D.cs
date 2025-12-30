using UnityEngine;

namespace Customer
{
    /// <summary>
    /// Generic 2D horizontal movement component with physics-based acceleration and deceleration.
    /// Features:
    ///   • Accelerates to max speed, then decelerates based on stopping distance to reach target X precisely
    ///   • No physics friction required - works with Drag and friction materials set to 0
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class HorizontalMovement2D : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField, Tooltip("Maximum horizontal speed (m/s)")]
        private float maxSpeed = 3f;
        
        [SerializeField, Tooltip("Acceleration rate (m/s²)")]
        private float acceleration = 10f;
        
        [SerializeField, Tooltip("Deceleration rate (m/s²), must be > 0")]
        private float deceleration = 15f;
        
        [SerializeField, Tooltip("Acceptable final position error (m)")]
        private float stopThreshold = 0.01f;

        [Header("Animation Control (Optional)")]
        [SerializeField, Tooltip("Character animator component")]
        private Animator animator;
        
        [SerializeField, Tooltip("Character sprite renderer for flipping")]
        private SpriteRenderer spriteRenderer;


        private Rigidbody2D _rb;
        private float _targetX;
        private bool _isMoving;

        public float TargetX => _targetX;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            // Auto-find components if not manually assigned
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Public API: Start moving towards target X position
        /// </summary>
        /// <param name="x">Target X coordinate in world space</param>
        public void MoveToX(float x)
        {
            _targetX = x;
            _isMoving = true;
            UpdateAnimation();
        }

        /// <summary>
        /// Check if currently moving towards target
        /// </summary>
        /// <returns>True if movement is in progress</returns>
        public bool IsMoving() => _isMoving;

        /// <summary>
        /// Stop movement immediately
        /// </summary>
        public void StopMove()
        {
            _isMoving = false;
            UpdateAnimation();
        }

        void FixedUpdate()
        {
            if (!_isMoving) return;

            float distanceToTarget = _targetX - transform.position.x;
            float direction = Mathf.Sign(distanceToTarget);
            float currentSpeed = _rb.velocity.x;

            // ──1) Check if arrived at target──
            if (Mathf.Abs(distanceToTarget) <= stopThreshold && Mathf.Abs(currentSpeed) < stopThreshold)
            {
                // Stop completely and snap to exact target position
                _rb.velocity = Vector2.zero;
                transform.position = new Vector3(_targetX, transform.position.y, transform.position.z);
                _isMoving = false;
                UpdateAnimation();
                return;
            }

            // ──2) Calculate stopping distance based on current speed──
            float stoppingDistance = (currentSpeed * currentSpeed) / (2f * deceleration);

            // ──3) Decide: accelerate or decelerate──
            if (Mathf.Abs(distanceToTarget) > stoppingDistance)
            {
                // Acceleration phase - we have room to speed up
                float newSpeed = Mathf.MoveTowards(currentSpeed, direction * maxSpeed, acceleration * Time.fixedDeltaTime);
                _rb.velocity = new Vector2(newSpeed, _rb.velocity.y);
            }
            else
            {
                // Deceleration phase - need to slow down to stop at target
                float newSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
                _rb.velocity = new Vector2(newSpeed, _rb.velocity.y);
            }

            // Update animation and sprite flip
            UpdateAnimation();
        }

        /// <summary>
        /// Update animation parameters and sprite flipping based on movement state
        /// </summary>
        private void UpdateAnimation()
        {
            // Set RUN animation parameter
            if (animator != null)
            {
                animator.SetBool("RUN", _isMoving);
            }

            // Flip sprite based on movement direction
            if (spriteRenderer != null && _isMoving)
            {
                float velocityX = _rb.velocity.x;
                if (velocityX > 0.01f)
                {
                    spriteRenderer.flipX = true;  // Moving right
                }
                else if (velocityX < -0.01f)
                {
                    spriteRenderer.flipX = false; // Moving left
                }
            }
        }
    }
}