using UnityEngine;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        // TODO: make these private later I think
        // TODO: Fix triggers, bruh 
        public Animator animator;
        public SpriteRenderer spriteRenderer;

        public bool isHoldingThings;
        private readonly int _isDescending = Animator.StringToHash("IsDown");
        private readonly int _isHolding = Animator.StringToHash("RunHolding");
        private readonly int _isOnGround = Animator.StringToHash("OnGround");

        // walk/run is based on current speed
        // prefix with an is means that it is a bool, otherwise a trigger
        private readonly int _isRunning = Animator.StringToHash("Run");
        private readonly int _jumpStart = Animator.StringToHash("StartJump");


        private AnimatorStateInfo _animatorState;
        private float _currentHorizontalSpeed;

        private PlayerMovement.MovementState _movementState;

        private PlayerMovement _playerMovement;
        private Transform _spriteNode;

        private void Awake()
        {
            // Get PlayerMovement from parent if this script is on SpriteNode, otherwise from self
            _playerMovement = GetComponentInParent<PlayerMovement>();
            if (_playerMovement == null) _playerMovement = GetComponent<PlayerMovement>();

            // Check if we're on the SpriteNode or on the Player
            if (GetComponent<Animator>() != null)
            {
                // We're on the SpriteNode
                _spriteNode = transform;
                animator = GetComponent<Animator>();
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            else
            {
                // We're on the Player, find SpriteNode child
                _spriteNode = transform.Find("SpriteNode");
                if (_spriteNode != null)
                {
                    animator = _spriteNode.GetComponent<Animator>();
                    spriteRenderer = _spriteNode.GetComponent<SpriteRenderer>();
                }
            }

            UnityEngine.Debug.Log("get Animator = " + animator);
            UnityEngine.Debug.Log("get SpriteRenderer = " + spriteRenderer);
        }

        private void Update()
        {
            _animatorState = animator.GetCurrentAnimatorStateInfo(0);
            UpdateKinematicStatus();
            UpdateAnimation();
            UpdateSpriteDirection();
        }

        private void UpdateKinematicStatus()
        {
            _movementState = _playerMovement.currentState;
            _currentHorizontalSpeed = Mathf.Abs(_playerMovement.Rb.velocity.x);
            // Debug.Log(_currentHorizontalSpeed);
        }

        private void UpdateAnimation()
        {
            // Always set the ground state boolean
            animator.SetBool(_isOnGround, _playerMovement.IsGrounded);

            switch (_movementState)
            {
                case PlayerMovement.MovementState.Idle:
                    if (_playerMovement.IsGrounded && !IsInState("Idle1") && !IsInState("Idle2"))
                    {
                        // Reset conflicting triggers when transitioning to idle on ground
                        animator.ResetTrigger(_isDescending);
                        animator.SetBool(_isRunning, false);
                    }

                    break;

                case PlayerMovement.MovementState.Left:
                case PlayerMovement.MovementState.Right:
                    if (_playerMovement.IsGrounded)
                    {
                        // Reset conflicting triggers when moving on ground
                        animator.ResetTrigger(_isDescending);
                        HandleMovementAnimation();
                    }

                    break;

                case PlayerMovement.MovementState.JumpAsc:
                    if (!IsInState("JumpAsc"))
                    {
                        animator.ResetTrigger(_isDescending);
                        animator.SetBool(_isRunning, false);
                        animator.SetTrigger(_jumpStart);
                    }

                    break;

                case PlayerMovement.MovementState.JumpDesc:
                    if (!_playerMovement.IsGrounded && !IsInState("JumpDesc")) animator.SetTrigger(_isDescending);
                    break;
            }
        }

        private void HandleMovementAnimation()
        {
            // Debug.Log(_currentHorizontalSpeed);
            // Determine if player is running (above a speed threshold) or walking
            var runSpeedThreshold = _playerMovement.MaxMoveSpeed * 0.1f;
            if (_currentHorizontalSpeed > runSpeedThreshold)
            {
                // Running
                if (isHoldingThings)
                {
                    if (!IsInState("RunHolding")) animator.SetBool(_isHolding, true);
                }
                else
                {
                    if (!IsInState("Run")) animator.SetBool(_isRunning, true);
                }
            }
            else
            {
                // Not running - reset running state
                animator.SetBool(_isRunning, false);
            }
        }

        private void UpdateSpriteDirection()
        {
            // Flip sprite based on movement direction
            // Default sprite facing is right
            if (_movementState == PlayerMovement.MovementState.Left)
                spriteRenderer.flipX = true;
            else if (_movementState == PlayerMovement.MovementState.Right) spriteRenderer.flipX = false;
        }

        private bool IsInState(string stateName)
        {
            return _animatorState.IsName(stateName);
        }


        private void TriggerJumpStart()
        {
            animator.SetTrigger(_jumpStart);
        }

        private void TriggerJumpLanding()
        {
            animator.SetBool(_isOnGround, true);
        }

        // Public methods that can be called by PlayerMovement or other scripts
        public void OnJumpStart()
        {
            TriggerJumpStart();
        }

        public void OnLanding()
        {
            TriggerJumpLanding();
        }
    }
}