using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    #region Inspector Fields
    [Header("Movement")]
    [SerializeField, Tooltip("Maximum move speed (m/s)")]
    private float maxMoveSpeed = 12f;

    [SerializeField, Tooltip("Acceleration (m/s²). Higher values reach max speed faster")]
    private float acceleration = 60f;

    [SerializeField, Tooltip("Deceleration (m/s²). Horizontal speed decay when releasing input")]
    private float deceleration = 80f;

    [SerializeField, Tooltip("Reverse acceleration (m/s²). Acceleration when input direction is opposite to velocity")]
    private float reverseAcceleration = 100f;

    [Header("Jump")]
    [SerializeField, Tooltip("Initial jump impulse")]
    private float jumpForce = 16f;

    [SerializeField, Tooltip("Extra jump force applied per second while holding the jump button")]
    private float extraJumpForce = 12f;

    [SerializeField, Tooltip("Maximum additional height relative to the jump start point")]
    private float maxJumpHeight = 5f;

    [SerializeField, Tooltip("Gravity scale used while ascending")]
    private float ascendGravityScale = 1f;

    [SerializeField, Tooltip("Gravity scale used while descending")]
    private float descendGravityScale = 2.5f;

    [SerializeField, Tooltip("Number of extra air jumps (1 means double jump)")]
    private int maxAirJumps = 1;

    [Header("Ground Check")]
    [SerializeField, Tooltip("Transform used as the center of the ground check box")]
    private Transform groundCheck;

    [SerializeField, Tooltip("Ground check box size (width, height)")]
    private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);

    [SerializeField, Tooltip("Layer(s) considered ground")]
    private LayerMask groundLayer;
    
    [SerializeField] 
    private UnityEngine.InputSystem.PlayerInput playerInputComponent;
    #endregion
    
    #region Private Fields
    
    private Vector2 _movementInput;
    private PlayerAnimation _playerAnimation;

    // Physics
    private Rigidbody2D _rb;
    private bool _isGrounded;
    private int _airJumpsLeft;
    private float _jumpStartY; // the y position where the last jump started
    private bool _jumpButtonHeld; // whether jump button is still held
    private Collider2D[] _groundCheckResults = new Collider2D[4]; // Reusable array for ground check
    #endregion
    
    #region Public Properties
    public bool IsGrounded => _isGrounded;
    public Rigidbody2D Rb => _rb;

    public float MaxMoveSpeed => maxMoveSpeed;

    #endregion

    public enum MovementState
    {
        Idle,
        Left,
        Right,
        JumpAsc,
        JumpDesc,
    }

    public MovementState currentState;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _airJumpsLeft = maxAirJumps;
        _playerAnimation = GetComponent<PlayerAnimation>();
        
        // Set up PlayerInput component action callbacks
        if (playerInputComponent != null)
        {
            playerInputComponent.actions["Move"].performed += ctx => _movementInput = ctx.ReadValue<Vector2>();
            playerInputComponent.actions["Move"].canceled += _ => _movementInput = Vector2.zero;
            
            playerInputComponent.actions["Jump"].started += OnJumpStarted;
            playerInputComponent.actions["Jump"].canceled += OnJumpCanceled;
            
            Debug.Log("PlayerInput component callbacks registered");
        }
        else
        {
            Debug.LogError("PlayerInput component is not assigned! Please add a PlayerInput component to the GameObject.");
        }
    }
    
    private void Start()
    {
        // Auto-assign groundCheck from GroundCheck component (FootCollider)
        // Done in Start() to ensure GroundCheck.Instance is initialized
        if (groundCheck == null)
        {
            // Try to find GroundCheck component in children
            var groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (groundCheckComponent != null)
            {
                groundCheck = groundCheckComponent.transform;
                Debug.Log("GroundCheck auto-assigned from child component: " + groundCheck.name);
            }
        }
        
        // Validate ground layer mask
        if (groundLayer == 0)
        {
            Debug.LogWarning("Ground Layer Mask is not set! Ground check will not work. Please assign it in the Inspector.");
        }
        
        Debug.Log($"PlayerMovement initialized - GroundCheck: {(groundCheck != null ? groundCheck.name : "NULL")}, LayerMask: {groundLayer.value}");
    }


    private void Update()
    {
        // Handle input and non-physics logic only
        GroundCheck();
        HandleJumpInput();
        UpdateDirection();
    }

    private void FixedUpdate()
    {
        // All forces / velocity changes should happen in FixedUpdate
        HorizontalMove();
        AdjustGravity();
    }


    private void UpdateDirection()
    {
        // Determine vertical state first
        MovementState verticalState = MovementState.Idle;
        if (_rb.velocity.y > 0 && !_isGrounded)
        {
            verticalState = MovementState.JumpAsc;
        }
        else if (_rb.velocity.y < 0 && !_isGrounded) // Fixed: should check !_isGrounded for falling
        {
            verticalState = MovementState.JumpDesc;
        }

        // Determine horizontal state
        MovementState horizontalState = MovementState.Idle;
        if (_movementInput.x > 0)
        {
            horizontalState = MovementState.Right;
        }
        else if (_movementInput.x < 0)
        {
            horizontalState = MovementState.Left;
        }

        // Prioritize vertical state when jumping/falling, otherwise use horizontal
        if (verticalState == MovementState.JumpAsc || verticalState == MovementState.JumpDesc)
        {
            currentState = verticalState;
        }
        else
        {
            currentState = horizontalState;
        }
    }

    /// <summary>
    /// Horizontal movement: approach the target speed using acceleration rather than setting velocity directly
    /// </summary>
    private void HorizontalMove()
    {
        float input = _movementInput.x; // horizontal input from new Input System
        float targetSpeed = input * maxMoveSpeed;
        float speedDiff = targetSpeed - _rb.velocity.x;

        // Choose acceleration or deceleration depending on input
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            // Check if velocity direction is opposite to input
            bool isReversing = (input > 0 && _rb.velocity.x < 0) || (input < 0 && _rb.velocity.x > 0);
            accelRate = isReversing ? reverseAcceleration : acceleration;
        }
        else
        {
            accelRate = deceleration;
        }

        // Δv = a * Δt -> apply a force proportional to desired change
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        _rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // Clamp horizontal speed to avoid excessive velocity
        Vector2 v = _rb.velocity;
        v.x = Mathf.Clamp(v.x, -maxMoveSpeed, maxMoveSpeed);
        _rb.velocity = v;
    }

    /// <summary>
    /// Ground check using an overlap box at the groundCheck position
    /// </summary>
    private void GroundCheck()
    {
        if (groundCheck == null)
        {
            _isGrounded = false;
            return;
        }

        if (groundLayer == 0)
        {
            _isGrounded = false;
            return;
        }

        // Debug: Check what we're actually testing
        Vector3 checkPos = groundCheck.position;
        
        // Now check with layer mask
        int hitCount = Physics2D.OverlapBoxNonAlloc(checkPos, groundCheckSize, 0f, _groundCheckResults, groundLayer);
        
        bool wasGrounded = _isGrounded;
        _isGrounded = false;
        
        for (int i = 0; i < hitCount; i++)
        {
            if (_groundCheckResults[i] && !_groundCheckResults[i].isTrigger)
            {
                // Make sure we're not detecting ourselves or our children
                if (_groundCheckResults[i].transform != transform && 
                    !_groundCheckResults[i].transform.IsChildOf(transform))
                {
                    _isGrounded = true;
                    // Debug.Log($"Ground detected: {_groundCheckResults[i].name}");
                    break;
                }
            }
        }

        // Reset air jumps when landing
        if (_isGrounded && !wasGrounded)
        {
            _airJumpsLeft = maxAirJumps;
            Debug.Log($"Landed! Air jumps reset to {maxAirJumps}");
            
            // Trigger landing animation
            if (_playerAnimation != null)
            {
                _playerAnimation.OnLanding();
            }
        }
    }

    /// <summary>
    /// Handle jump-related input
    /// Tap jump = short jump with initial impulse only
    /// Hold jump = higher jump with continuous force applied
    /// </summary>
    private void HandleJumpInput()
    {
        // While holding jump: apply extra force until reaching max height
        // Only applies if we're still holding from the jump we initiated (_jumpButtonHeld)
        if (_jumpButtonHeld)
        {
            if (transform.position.y < _jumpStartY + maxJumpHeight && _rb.velocity.y > 0f)
            {
                // Use Impulse so the added force affects the velocity immediately
                _rb.AddForce(Vector2.up * (extraJumpForce * Time.deltaTime), ForceMode2D.Impulse);
            }
            else
            {
                // Stop applying extra force if we've reached max height or started falling
                _jumpButtonHeld = false;
            }
        }
    }
    
    /// <summary>
    /// Called when jump button is pressed (via InputAction callback)
    /// </summary>
    private void OnJumpStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            Jump();
        }
        else if (_airJumpsLeft > 0)
        {
            Jump();
            _airJumpsLeft--;
        }
    }
    
    /// <summary>
    /// Called when jump button is released (via InputAction callback)
    /// </summary>
    private void OnJumpCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _jumpButtonHeld = false;
    }

    /// <summary>
    /// Perform a jump: reset vertical velocity and apply initial impulse
    /// </summary>
    private void Jump()
    {
        // Reset vertical velocity to avoid stacking
        Vector2 v = _rb.velocity;
        v.y = 0f;
        _rb.velocity = v;

        // Apply initial jump impulse
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _jumpStartY = transform.position.y;
        _jumpButtonHeld = true;
        
        // Trigger jump start animation
        if (_playerAnimation != null)
        {
            _playerAnimation.OnJumpStart();
        }
    }

    /// <summary>
    /// Switch gravity scale depending on ascending/descending phase to get a nicer jump arc
    /// </summary>
    private void AdjustGravity()
    {
        if (_rb.velocity.y > 0f)            // ascending
        {
            _rb.gravityScale = ascendGravityScale;
        }
        else if (_rb.velocity.y < 0f)       // descending
        {
            _rb.gravityScale = descendGravityScale;
        }
        else                               // stationary vertically
        {
            _rb.gravityScale = 1f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
            
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(groundCheck.position, groundCheckSize);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (groundCheck != null && Application.isPlaying)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
