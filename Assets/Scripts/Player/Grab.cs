using GameObjects;
using UnityEngine;

public class Grab : MonoBehaviour
{
    private Grabbable _closestGrabbable;

    private IInteract _closestInteract;

    private void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Update()
    {
        if (_grabbedRb == null)
        {
            // Detect grabbable objects in range
            var hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, grabRange, _overlapResults);

            Grabbable closestGrabbable = null;
            IInteract closestInteract = null;

            for (var i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];

                // Find closest Grabbable object
                if (col.gameObject.CompareTag("Grabbable"))
                {
                    var grabbable = col.gameObject.GetComponent<Grabbable>();
                    if (grabbable)
                    {
                        if (!closestGrabbable)
                        {
                            closestGrabbable = grabbable;
                        }
                        else
                        {
                            if (Vector2.Distance(transform.position, grabbable.transform.position) <
                                Vector2.Distance(transform.position, closestGrabbable.transform.position))
                                closestGrabbable = grabbable;
                        }
                    }
                }

                // Find closest Interact object
                if (col.gameObject.CompareTag("Interactable"))
                {
                    var interact = col.gameObject.GetComponent<IInteract>();
                    if (interact != null)
                        if (closestInteract == null)
                            closestInteract = interact;
                }
            }

            // Update cursor position
            if (closestInteract != null)
            {
                cursor.SetActive(true);
                cursor.transform.position = closestInteract.Transform.position;
            }
            else if (closestGrabbable != null)
            {
                cursor.SetActive(true);
                cursor.transform.position = closestGrabbable.transform.position;
            }
            else
            {
                cursor.SetActive(false);
            }

            // Store closest objects for grab callback
            _closestInteract = closestInteract;
            _closestGrabbable = closestGrabbable;
        }
        else
        {
            // Hide cursor when holding an object
            cursor.SetActive(false);
        }

        // 旧输入系统：按下 Fire1 交互/抓取
        if (Input.GetButtonDown("Fire1"))
        {
            HandleGrabPressed();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize grab range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRange);
    }

    /// <summary>
    ///     Called when grab/interaction button is pressed
    /// </summary>
    private void HandleGrabPressed()
    {
        if (_grabbedRb == null)
        {
            // Try to interact first, then grab
            if (_closestInteract != null)
                _closestInteract.Interact();
            else if (_closestGrabbable != null) GrabObj(_closestGrabbable);
        }
        else
        {
            // Release the held object
            ReleaseObj();
        }
    }

    /// <summary>
    ///     Grab an object and parent it to the player
    /// </summary>
    private void GrabObj(Grabbable grabbable)
    {
        if (grabbable == null) return;

        _grabbedRb = grabbable.rb;
        cursor.SetActive(false);

        // Make object kinematic and reset physics
        _grabbedRb.isKinematic = true;
        _grabbedRb.velocity = Vector2.zero;
        _grabbedRb.angularVelocity = 0;

        // Parent to player and position above
        _grabbedRb.transform.SetParent(transform);
        _grabbedRb.transform.localPosition = Vector3.up;

        // Disable colliders while grabbed
        var colliders = _grabbedRb.transform.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = false;

        // Call OnGrab callback
        grabbable.OnGrab();
    }

    /// <summary>
    ///     Release the grabbed object and apply force
    /// </summary>
    private void ReleaseObj()
    {
        if (_grabbedRb == null) return;

        // Restore physics
        _grabbedRb.isKinematic = false;
        _grabbedRb.transform.SetParent(null);

        // Apply release force based on player direction
        var direction = 1f;
        if (_playerAnimation != null && _playerAnimation.spriteRenderer != null)
            direction = _playerAnimation.spriteRenderer.flipX ? -1f : 1f;
        var force = new Vector3(releaseForce.x * direction, releaseForce.y, 0);
        _grabbedRb.velocity = _playerRb.velocity;
        _grabbedRb.AddForce(force, ForceMode2D.Impulse);

        // Re-enable colliders
        var colliders = _grabbedRb.transform.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = true;

        // Call OnRelease callback
        var grabbable = _grabbedRb.GetComponent<Grabbable>();
        if (grabbable != null) grabbable.OnRelease();

        _grabbedRb = null;
    }

    #region Inspector Fields

    [SerializeField] [Tooltip("Radius for grab field")]
    private float grabRange = 0.5f;

    [SerializeField] [Tooltip("Cursor GameObject to show grab target")]
    private GameObject cursor;

    [SerializeField] [Tooltip("Force applied when releasing objects")]
    private Vector3 releaseForce = Vector3.up;

    #endregion

    #region Private Fields

    private Rigidbody2D _playerRb;
    private PlayerAnimation _playerAnimation;
    private Rigidbody2D _grabbedRb; // Currently grabbed object's rigidbody
    private readonly Collider2D[] _overlapResults = new Collider2D[1024]; // Never trust players

    #endregion
}