using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAnimation : MonoBehaviour
{
    // TODO: make these private later I think
    // TODO: Fix triggers, bruh 
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public bool isHoldingThings;
    private readonly int _animationIdle = Animator.StringToHash("StopTrigger");

    // walk/run is based on current speed
    private readonly int _animationIdleVariant = Animator.StringToHash("idleVariant");
    private readonly int _animationJumpAscend = Animator.StringToHash("JumpAscTrigger");
    private readonly int _animationJumpDescend = Animator.StringToHash("JumpDescTrigger");
    private readonly int _animationJumpLanding = Animator.StringToHash("JumpLandingTrigger");
    private readonly int _animationJumpStart = Animator.StringToHash("JumpTrigger");
    private readonly int _animationRun = Animator.StringToHash("RunTrigger");
    private readonly int _animationRunHolding = Animator.StringToHash("RunHoldingTrigger");
    private readonly int _animationWalk = Animator.StringToHash("MoveTrigger");
    private readonly int _animationWalkVariant = Animator.StringToHash("walkVariant");
    private readonly int _fallSpeed = Animator.StringToHash("FallSpeed");
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

        Debug.Log("get Animator = " + animator);
        Debug.Log("get SpriteRenderer = " + spriteRenderer);
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
        switch (_movementState)
        {
            case PlayerMovement.MovementState.Idle:
                if (!IsInState("Idle1") && !IsInState("Idle2")) BackToIdle();
                break;

            case PlayerMovement.MovementState.Left:
            case PlayerMovement.MovementState.Right:
                HandleMovementAnimation();
                break;

            case PlayerMovement.MovementState.JumpAsc:
                if (!IsInState("JumpAsc")) animator.SetTrigger(_animationJumpAscend);
                break;

            case PlayerMovement.MovementState.JumpDesc:
                if (!IsInState("JumpDesc")) animator.SetTrigger(_animationJumpDescend);
                animator.SetFloat(_fallSpeed, -_playerMovement.Rb.velocity.y);
                break;
        }
    }

    private void HandleMovementAnimation()
    {
        Debug.Log(_currentHorizontalSpeed);
        // Determine if player is running (above a speed threshold) or walking
        var runSpeedThreshold = _playerMovement.MaxMoveSpeed * 0.9f;
        if (_currentHorizontalSpeed > runSpeedThreshold)
        {
            // Running
            if (isHoldingThings)
            {
                if (!IsInState("RunHolding")) animator.SetTrigger(_animationRunHolding);
            }
            else
            {
                if (!IsInState("Run")) animator.SetTrigger(_animationRun);
            }
        }
        else if (_currentHorizontalSpeed > 0.1f)
        {
            // Walking
            if (!IsInState("Walk1") && !IsInState("Walk2")) StartWalking();
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

    // choose from 2 idle animation
    private void RandomizeIdle()
    {
        animator.SetFloat(_animationIdleVariant, (float)Math.Round(Random.value));
    }

    // choose from 2 walking animation
    private void RandomizeWalk()
    {
        animator.SetFloat(_animationWalkVariant, (float)Math.Round(Random.value));
    }

    private void StartWalking()
    {
        Debug.Log("StartWalking");
        RandomizeWalk();
        animator.SetTrigger(_animationWalk);
    }

    private void BackToIdle()
    {
        RandomizeIdle();
        animator.SetTrigger(_animationIdle);
    }

    private void TriggerJumpStart()
    {
        animator.SetTrigger(_animationJumpStart);
    }

    private void TriggerJumpLanding()
    {
        animator.SetTrigger(_animationJumpLanding);
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