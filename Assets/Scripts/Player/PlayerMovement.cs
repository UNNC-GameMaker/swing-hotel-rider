using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    #region Inspector
    [Header("Movement")]
    [Tooltip("Maximum move speed (m/s)")]
    public float maxMoveSpeed = 12f;

    [Tooltip("Acceleration (m/s²). Higher values reach max speed faster")]
    public float acceleration = 60f;

    [Tooltip("Deceleration (m/s²). Horizontal speed decay when releasing input")]
    public float deceleration = 80f;

    [Tooltip("Reverse acceleration (m/s²). Acceleration when input direction is opposite to velocity")]
    public float reverseAcceleration = 100f;

    [Header("Jump")]
    [Tooltip("Initial jump impulse")]
    public float jumpForce = 16f;

    [Tooltip("Extra jump force applied per second while holding the jump button")]
    public float extraJumpForce = 12f;

    [Tooltip("Maximum additional height relative to the jump start point")]
    public float maxJumpHeight = 5f;

    [Tooltip("Gravity scale used while ascending")]
    public float ascendGravityScale = 1f;

    [Tooltip("Gravity scale used while descending")]
    public float descendGravityScale = 2.5f;

    [Tooltip("Number of extra air jumps (1 means double jump)")]
    public int maxAirJumps = 1;

    [Header("Ground Check")]
    [Tooltip("Transform used as the center of the ground check box")]
    public Transform groundCheck;

    [Tooltip("Ground check box size (width, height)")]
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);

    [Tooltip("Layer(s) considered ground")]
    public LayerMask groundLayer;
    #endregion
    
    private PlayerInput _playerInput;
    private Vector2 _movementInput;
    private PlayerAnimation _playerAnimation;

    // Physics
    public Rigidbody2D rb;
    private bool _isGrounded;
    private int _airJumpsLeft;
    private float _jumpStartY; // the y position where the last jump started
    private bool _jumpButtonHeld; // whether jump button is still held
    private Collider2D[] _groundCheckResults = new Collider2D[4]; // Reusable array for ground check

    public bool IsGrounded { get { return _isGrounded; } }

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
        rb = GetComponent<Rigidbody2D>();
        _airJumpsLeft = maxAirJumps;
        _playerAnimation = GetComponent<PlayerAnimation>();
        
        // Initialize new Input System
        _playerInput = new PlayerInput();
        Debug.Log("Player input initialized:" +  _playerInput);
        Debug.Log("player input init, position:" + transform.position);
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

    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Disable();
    }

    private void Update()
    {
        // Handle input and non-physics logic only
        ReadInput();
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

    /// <summary>
    /// Read input from the new Input System
    /// </summary>
    private void ReadInput()
    {
        _movementInput = _playerInput.Player.Move.ReadValue<Vector2>();
    }

    private void UpdateDirection()
    {
        if (_movementInput.x > 0)
        {
            currentState =  MovementState.Right;
        } else if (_movementInput.x < 0)
        {
            currentState = MovementState.Left;
        }

        if (rb.velocity.y > 0 && !_isGrounded)
        {
            currentState = MovementState.JumpAsc;
        }
        else if (rb.velocity.y < 0 && _isGrounded)
        {
            currentState = MovementState.JumpDesc;
        }
        else
        {
            currentState = MovementState.Idle;
        }
    }

    /// <summary>
    /// Horizontal movement: approach the target speed using acceleration rather than setting velocity directly
    /// </summary>
    private void HorizontalMove()
    {
        float input = _movementInput.x; // horizontal input from new Input System
        float targetSpeed = input * maxMoveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;

        // Choose acceleration or deceleration depending on input
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            // Check if velocity direction is opposite to input
            bool isReversing = (input > 0 && rb.velocity.x < 0) || (input < 0 && rb.velocity.x > 0);
            accelRate = isReversing ? reverseAcceleration : acceleration;
        }
        else
        {
            accelRate = deceleration;
        }

        // Δv = a * Δt -> apply a force proportional to desired change
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // Clamp horizontal speed to avoid excessive velocity
        Vector2 v = rb.velocity;
        v.x = Mathf.Clamp(v.x, -maxMoveSpeed, maxMoveSpeed);
        rb.velocity = v;
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
            if (_groundCheckResults[i] != null && !_groundCheckResults[i].isTrigger)
            {
                // Make sure we're not detecting ourselves or our children
                if (_groundCheckResults[i].transform != transform && 
                    !_groundCheckResults[i].transform.IsChildOf(transform))
                {
                    _isGrounded = true;
                    Debug.Log($"Ground detected: {_groundCheckResults[i].name}");
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
        // On jump pressed (triggered on first frame of press)
        if (_playerInput.Player.Jump.triggered)
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

        // While holding jump: apply extra force until reaching max height
        // Only applies if we're still holding from the jump we initiated (_jumpButtonHeld)
        if (_playerInput.Player.Jump.IsPressed() && _jumpButtonHeld)
        {
            if (transform.position.y < _jumpStartY + maxJumpHeight && rb.velocity.y > 0f)
            {
                // Use Impulse so the added force affects the velocity immediately
                rb.AddForce(Vector2.up * (extraJumpForce * Time.deltaTime), ForceMode2D.Impulse);
            }
            else
            {
                // Stop applying extra force if we've reached max height or started falling
                _jumpButtonHeld = false;
            }
        }

        // On jump released - stop applying extra force
        if (_playerInput.Player.Jump.WasReleasedThisFrame())
        {
            _jumpButtonHeld = false;
        }
    }

    /// <summary>
    /// Perform a jump: reset vertical velocity and apply initial impulse
    /// </summary>
    private void Jump()
    {
        // Reset vertical velocity to avoid stacking
        Vector2 v = rb.velocity;
        v.y = 0f;
        rb.velocity = v;

        // Apply initial jump impulse
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
        if (rb.velocity.y > 0f)            // ascending
        {
            rb.gravityScale = ascendGravityScale;
        }
        else if (rb.velocity.y < 0f)       // descending
        {
            rb.gravityScale = descendGravityScale;
        }
        else                               // stationary vertically
        {
            rb.gravityScale = 1f;
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
