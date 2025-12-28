using Input;
using Managers;
using Player;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour, IInputListener
{
    public enum MovementState
    {
        Idle,
        Left,
        Right,
        JumpAsc,
        JumpDesc
    }

    public MovementState currentState;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        _airJumpsLeft = maxAirJumps;
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        // Unregister from InputManager
        var inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.UnregisterListener(this);
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
            Debug.LogWarning(
                "Ground Layer Mask is not set! Ground check will not work. Please assign it in the Inspector.");

        Debug.Log(
            $"PlayerMovement initialized - GroundCheck: {(groundCheck != null ? groundCheck.name : "NULL")}, LayerMask: {groundLayer.value}");
   
   
        // Register with InputManager
        var inputManager = GameManager.Instance.GetManager<InputManager>();
        if (inputManager != null)
        {
            inputManager.RegisterListener(this);
            Debug.Log("PlayerMovement registered with InputManager");
        }
        else
        {
            Debug.LogError("InputManager not found! PlayerMovement will not receive input events.");
        }
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

    private void OnDrawGizmos()
    {
        if (groundCheck != null && Application.isPlaying)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(groundCheck.position, groundCheckSize);
        }
    }


    private void UpdateDirection()
    {
        // Determine vertical state first
        var verticalState = MovementState.Idle;
        if (Rb.velocity.y > 0 && !IsGrounded)
            verticalState = MovementState.JumpAsc;
        else if (Rb.velocity.y < 0 && !IsGrounded) // Fixed: should check !_isGrounded for falling
            verticalState = MovementState.JumpDesc;

        // Determine horizontal state
        var horizontalState = MovementState.Idle;
        if (_movementInput.x > 0)
            horizontalState = MovementState.Right;
        else if (_movementInput.x < 0) horizontalState = MovementState.Left;

        // Prioritize vertical state when jumping/falling, otherwise use horizontal
        if (verticalState == MovementState.JumpAsc || verticalState == MovementState.JumpDesc)
            currentState = verticalState;
        else
            currentState = horizontalState;
    }

    /// <summary>
    ///     Horizontal movement: approach the target speed using acceleration rather than setting velocity directly
    /// </summary>
    private void HorizontalMove()
    {
        var input = _movementInput.x; // horizontal input from new Input System
        var targetSpeed = input * maxMoveSpeed;
        var speedDiff = targetSpeed - Rb.velocity.x;

        // Choose acceleration or deceleration depending on input
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            // Check if velocity direction is opposite to input
            var isReversing = (input > 0 && Rb.velocity.x < 0) || (input < 0 && Rb.velocity.x > 0);
            accelRate = isReversing ? reverseAcceleration : acceleration;
        }
        else
        {
            accelRate = deceleration;
        }

        // Δv = a * Δt -> apply a force proportional to desired change
        var movement = speedDiff * accelRate * Time.fixedDeltaTime;
        Rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // Clamp horizontal speed to avoid excessive velocity
        var v = Rb.velocity;
        v.x = Mathf.Clamp(v.x, -maxMoveSpeed, maxMoveSpeed);
        Rb.velocity = v;
    }

    /// <summary>
    ///     Ground check using an overlap box at the groundCheck position
    /// </summary>
    private void GroundCheck()
    {
        if (!groundCheck)
        {
            IsGrounded = false;
            return;
        }

        if (groundLayer == 0)
        {
            IsGrounded = false;
            return;
        }

        // Debug: Check what we're actually testing
        var checkPos = groundCheck.position;

        // Now check with layer mask
        var hitCount = Physics2D.OverlapBoxNonAlloc(checkPos, groundCheckSize, 0f, _groundCheckResults, groundLayer);

        var wasGrounded = IsGrounded;
        IsGrounded = false;

        for (var i = 0; i < hitCount; i++)
            if (_groundCheckResults[i] && !_groundCheckResults[i].isTrigger)
                // Make sure we're not detecting ourselves or our children
                if (_groundCheckResults[i].transform != transform &&
                    !_groundCheckResults[i].transform.IsChildOf(transform))
                {
                    IsGrounded = true;
                    // Debug.Log($"Ground detected: {_groundCheckResults[i].name}");
                    break;
                }

        // Reset air jumps when landing
        if (IsGrounded && !wasGrounded)
        {
            _airJumpsLeft = maxAirJumps;
            // Debug.Log($"Landed! Air jumps reset to {maxAirJumps}");

            // Trigger landing animation
            if (_playerAnimation) _playerAnimation.OnLanding();
        }
    }

    /// <summary>
    ///     handle jump input
    /// </summary>
    private void HandleJumpInput()
    {

        // hold: jump higher
        if (_jumpButtonHeld)
        {
            if (transform.position.y < _jumpStartY + maxJumpHeight && Rb.velocity.y > 0f)
                Rb.AddForce(Vector2.up * (extraJumpForce * Time.deltaTime), ForceMode2D.Impulse);
            else
                _jumpButtonHeld = false;
        }
    }

    private void JumpStart()
    {
        if (IsGrounded)
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
    ///     Perform a jump: reset vertical velocity and apply initial impulse
    /// </summary>
    private void Jump()
    {
        // Reset vertical velocity to avoid stacking
        var v = Rb.velocity;
        v.y = 0f;
        Rb.velocity = v;

        // Apply initial jump impulse
        Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _jumpStartY = transform.position.y;
        _jumpButtonHeld = true;

        // Trigger jump start animation
        if (_playerAnimation) _playerAnimation.OnJumpStart();
    }

    /// <summary>
    ///     Switch gravity scale depending on ascending/descending phase to get a nicer jump arc
    /// </summary>
    private void AdjustGravity()
    {
        if (Rb.velocity.y > 0f) // ascending
            Rb.gravityScale = ascendGravityScale;
        else if (Rb.velocity.y < 0f) // descending
            Rb.gravityScale = descendGravityScale;
        else // stationary vertically
            Rb.gravityScale = 1f;
    }
    

    #region Inspector Fields

    [Header("Movement")] [SerializeField] [Tooltip("Maximum move speed (m/s)")]
    private float maxMoveSpeed = 12f;

    [SerializeField] [Tooltip("Acceleration (m/s²). Higher values reach max speed faster")]
    private float acceleration = 60f;

    [SerializeField] [Tooltip("Deceleration (m/s²). Horizontal speed decay when releasing input")]
    private float deceleration = 80f;

    [SerializeField] [Tooltip("Reverse acceleration (m/s²). Acceleration when input direction is opposite to velocity")]
    private float reverseAcceleration = 100f;

    [Header("Jump")] [SerializeField] [Tooltip("Initial jump impulse")]
    private float jumpForce = 16f;

    [SerializeField] [Tooltip("Extra jump force applied per second while holding the jump button")]
    private float extraJumpForce = 12f;

    [SerializeField] [Tooltip("Maximum additional height relative to the jump start point")]
    private float maxJumpHeight = 5f;

    [SerializeField] [Tooltip("Gravity scale used while ascending")]
    private float ascendGravityScale = 1f;

    [SerializeField] [Tooltip("Gravity scale used while descending")]
    private float descendGravityScale = 2.5f;

    [SerializeField] [Tooltip("Number of extra air jumps (1 means double jump)")]
    private int maxAirJumps = 1;

    [Header("Ground Check")] [SerializeField] [Tooltip("Transform used as the center of the ground check box")]
    private Transform groundCheck;

    [SerializeField] [Tooltip("Ground check box size (width, height)")]
    private Vector2 groundCheckSize = new(0.8f, 0.1f);

    [SerializeField] [Tooltip("Layer(s) considered ground")]
    private LayerMask groundLayer;

    #endregion

    #region Private Fields

    private Vector2 _movementInput;
    private PlayerAnimation _playerAnimation;

    // Physics
    private int _airJumpsLeft;
    private float _jumpStartY; // the y position where the last jump started
    private bool _jumpButtonHeld; // whether jump button is still held
    private readonly Collider2D[] _groundCheckResults = new Collider2D[4]; // Reusable array for ground check

    #endregion

    #region Public Properties

    public bool IsGrounded { get; private set; }

    public Rigidbody2D Rb { get; private set; }

    public float MaxMoveSpeed => maxMoveSpeed;

    #endregion

    #region IInputListener Implementation

    public void OnInputEvent(InputEvents inputEvent, InputState state)
    {
        Debug.Log("OnInputEvent: " + inputEvent.ToString());
        switch (inputEvent)
        {
            case InputEvents.Jump:
                switch (state)
                {
                    case InputState.Started:
                        Debug.Log("Starting jump");
                        // Jump button pressed - attempt to jump
                        JumpStart();
                        break;
                    case InputState.Performed:
                        Debug.Log("Hold jump");
                        _jumpButtonHeld = true;
                        break;
                    case InputState.Canceled:
                        // Jump button released - stop adding extra jump force
                        _jumpButtonHeld = false;
                        break;
                }
                break;
            
            case InputEvents.Crouch:
                // Handle crouch if needed
                break;
            
            case InputEvents.Grab:
                // Handle grab if needed
                break;
            
            case InputEvents.Release:
                // Handle release if needed
                break;
        }
    }

    public void OnInputAxis(InputAxis axis, Vector2 value)
    {
        switch (axis)
        {
            case InputAxis.MoveAxis:
                // Update movement input
                _movementInput = value;
                break;
            
            case InputAxis.CameraAxis:
                // Handle camera input if needed
                break;
        }
    }

    public int InputPriority => 0;
    public bool IsInputEnabled => true;

    #endregion
}