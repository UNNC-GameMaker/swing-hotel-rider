using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platform
{
    /// <summary>
    ///     One-way platform (automatically retrieves the player's foot singleton).
    ///     - The player's foot must go from "low to high" to become solid again.
    ///     - Two height thresholds: higher than solidOffset -> potentially solid; lower than hollowOffset -> always hollow.
    ///     - When the down key is pressed, the platform is always hollow (only effective when the foot is close to the
    ///     platform).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [Header("Player Layer (for overlap detection)")]
        public LayerMask playerLayer = 1 << 0;

        [Header("Height above platform to become solid")]
        public float solidOffset = 0.05f;

        [Header("Height below platform to become hollow immediately")]
        public float hollowOffset = 0.05f;

        [Header("Distance threshold for down key to take effect")]
        public float downKeyDistance = 1.0f;
        
        [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;

        /// <summary>Flag: Only allow becoming solid again if the foot has been below the platform</summary>
        private bool _allowSolid;

        private BoxCollider2D _boxCollider;

        private bool _initialized;

        /// <summary>Current solid state</summary>
        private bool _isSolid;

        /// <summary>Cached movement input from PlayerInput callbacks</summary>
        private Vector2 _movementInput;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            _initialized = false;
            _isSolid = true;

            // Find the PlayerInput component and subscribe to its Move action
            if (playerInput != null)
            {
                // Subscribe to the Move action callbacks, just like PlayerMovement does
                playerInput.actions["Move"].performed += ctx => _movementInput = ctx.ReadValue<Vector2>();
                playerInput.actions["Move"].canceled += _ => _movementInput = Vector2.zero;
                
                UnityEngine.Debug.Log("OneWayPlatform: Move action callbacks registered");
            }
            else
            {
                UnityEngine.Debug.LogWarning("OneWayPlatform: Could not find PlayerInput component in scene");
            }
        }

        private void Update()
        {
            if (!GroundCheck.Instance) return;

            if (!_initialized)
            {
                InitializePlatformState();
                return;
            }

            UpdatePlatformState();
        }

        private void InitializePlatformState()
        {
            _initialized = true;
            if (GroundCheck.Instance.FootCollider.position.y < _boxCollider.bounds.max.y)
            {
                SetSolid(false);
                _allowSolid = true;
            }
            else
            {
                SetSolid(true);
            }
        }

        private void UpdatePlatformState()
        {
            var footTf = GroundCheck.Instance.FootCollider;
            var surfaceY = _boxCollider.bounds.max.y;
            
            var pressingDown = _movementInput.y < -0.1f;
            
            bool overlapping = Physics2D.OverlapArea(_boxCollider.bounds.min, _boxCollider.bounds.max, playerLayer);

            // Calculate distance from foot to platform surface
            var distanceToSurface = Mathf.Abs(footTf.position.y - surfaceY);
            var isNearPlatform = distanceToSurface <= downKeyDistance &&
                                 Mathf.Abs(footTf.position.x - transform.position.x) < 1.5f;
            
            if (pressingDown && isNearPlatform)
            {
                SetSolid(false);
                _allowSolid = false;
                return;
            }
            
            if (footTf.position.y < surfaceY - hollowOffset)
            {
                SetSolid(false);
                _allowSolid = true;
                return;
            }
            
            if (_allowSolid &&
                footTf.position.y > surfaceY + solidOffset &&
                !overlapping)
            {
                SetSolid(true);
                _allowSolid = false; // Need to go "low to high" again to become solid
            }
        }

        /// <summary>
        ///     Unify switching collision state and recording state
        /// </summary>
        private void SetSolid(bool solid)
        {
            if (_isSolid == solid) return;
            _isSolid = solid;

            // When hollow, make it a trigger so player passes through
            // When solid, make it a collider so player stands on it
            _boxCollider.isTrigger = !solid;
            
            // UnityEngine.Debug.Log($"OneWayPlatform: SetSolid({solid}) - isTrigger = {_boxCollider.isTrigger}");
        }
    }
}