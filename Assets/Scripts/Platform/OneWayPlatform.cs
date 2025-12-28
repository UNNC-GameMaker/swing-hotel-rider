using Input;
using Managers;
using Player;
using UnityEngine;

/// <summary>
///     One-way platform (automatically retrieves the player's foot singleton).
///     - The player's foot must go from "low to high" to become solid again.
///     - Two height thresholds: higher than solidOffset -> potentially solid; lower than hollowOffset -> always hollow.
///     - When the down key is pressed, the platform is always hollow (only effective when the foot is close to the
///     platform).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class OneWayPlatform : MonoBehaviour, IInputListener
{
    [Header("Player Layer (for overlap detection)")]
    public LayerMask playerLayer = 1 << 0;

    [Header("Height above platform to become solid")]
    public float solidOffset = 0.05f;

    [Header("Height below platform to become hollow immediately")]
    public float hollowOffset = 0.05f;

    [Header("Distance threshold for down key to take effect")]
    public float downKeyDistance = 1.0f;

    /// <summary>Flag: Only allow becoming solid again if the foot has been below the platform</summary>
    private bool _allowSolid;

    private BoxCollider2D _boxCollider;

    private bool _initialized;

    /// <summary>Current solid state</summary>
    private bool _isSolid;

    private Vector2 _movementInput;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _initialized = false;
        _isSolid = true;
        _boxCollider.isTrigger = false;
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

        // Input: Holding down key
        var pressingDown = _movementInput.y < -0.1f;

        // Overlapping with player?
        bool overlapping = Physics2D.OverlapArea(_boxCollider.bounds.min, _boxCollider.bounds.max, playerLayer);

        // Calculate distance from foot to platform surface
        var distanceToSurface = Mathf.Abs(footTf.position.y - surfaceY);
        var isNearPlatform = distanceToSurface <= downKeyDistance &&
                             Mathf.Abs(footTf.position.x - transform.position.x) < 1.5f;

        // 1. Pressing down and foot is near platform -> Immediately hollow, prevent becoming solid immediately
        if (pressingDown && isNearPlatform)
        {
            SetSolid(false);
            _allowSolid = false;
            return;
        }

        // 2. Foot is below the low threshold -> Hollow, and allow "low to high" logic for next time
        if (footTf.position.y < surfaceY - hollowOffset)
        {
            SetSolid(false);
            _allowSolid = true;
            return;
        }

        // 3. If it was below before, currently above high threshold, and not overlapping -> Become solid
        if (_allowSolid &&
            footTf.position.y > surfaceY + solidOffset &&
            !overlapping)
        {
            SetSolid(true);
            _allowSolid = false; // Need to go "low to high" again to become solid
        }
        // 4. Otherwise keep current state
    }

    /// <summary>
    ///     Unify switching layer and recording state
    /// </summary>
    private void SetSolid(bool solid)
    {
        if (_isSolid == solid) return;
        _isSolid = solid;

        if (solid)
            gameObject.layer = LayerMask.NameToLayer("GroundCollider");
        else
            gameObject.layer = LayerMask.NameToLayer("GroundColliderHollow");
    }

    public void OnInputEvent(InputEvents inputEvent, InputState state)
    {
        
    }

    public void OnInputAxis(InputAxis axis, Vector2 value)
    {
        if (axis == InputAxis.MoveAxis)
        {
            _movementInput = value;
        }
    }

    public int InputPriority => 0;
    public bool IsInputEnabled => true;
}