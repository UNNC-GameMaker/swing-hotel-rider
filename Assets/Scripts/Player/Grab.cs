using System.Collections.Generic;
using Building;
using GameObjects;
using Input;
using Managers;
using Player;
using UnityEngine;

public class Grab : MonoBehaviour, IInputListener
{
    private Grabbable _closestGrabbable;

    private Interactable _closestInteract;

    private void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _sfxManager = GameManager.Instance.GetManager<SFXManager>();
    }

    private void OnEnable()
    {
        // Register with InputManager
        GameManager.Instance.GetManager<InputManager>().RegisterListener(this);
    }

    private void OnDisable()
    {
        // Unregister from InputManager
        GameManager.Instance.GetManager<InputManager>().UnregisterListener(this);
    }

    private void Update()
    {
        if (_grabbedObjects.Count < _inventorySize)
        {
            // Detect grabbable objects in range
            var hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, grabRange, _overlapResults);
            Grabbable closestGrabbable = null;
            Interactable closestInteract = null;
            for (var i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];
                // Find the closest Grabbable object
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
                    var interact = col.gameObject.GetComponent<Interactable>();
                    if (interact != null) closestInteract ??= interact;
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
            // Hide cursor when holding max objects
            cursor.SetActive(false);
            _closestInteract = null;
            _closestGrabbable = null;
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize grab range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRange);
        
        // Draw a filled circle at a smaller radius for better visibility
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        DrawCircle(transform.position, grabRange, 32);
        
        #if UNITY_EDITOR
        // Show grab range value
        UnityEditor.Handles.Label(transform.position + Vector3.up * grabRange, $"Grab Range: {grabRange:F2}");
        
        // Show the closest grabbable if exists
        if (_closestGrabbable != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _closestGrabbable.transform.position);
            UnityEditor.Handles.Label(_closestGrabbable.transform.position, "Closest Grabbable");
        }
        
        // Show closest interact if exists
        if (_closestInteract != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _closestInteract.Transform.position);
            UnityEditor.Handles.Label(_closestInteract.Transform.position, "Closest Interact");
        }
        
        // Show grabbed objects connection
        foreach (var rb in _grabbedObjects)
        {
            if (rb != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, rb.transform.position);
                UnityEditor.Handles.Label(rb.transform.position, "GRABBED");
            }
        }
        #endif
    }
    
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    /// <summary>
    ///     Called when grab/interaction button is pressed
    /// </summary>
    private void HandleGrabPressed()
    {
            // Try to interact first, then grab
            if (_closestInteract != null)
                _closestInteract.OnInteract();
            else if (_closestGrabbable != null) GrabObj(_closestGrabbable);
    }

    /// <summary>
    ///     Grab an object and parent it to the player
    /// </summary>
    private void GrabObj(Grabbable grabbable)
    {
        if (grabbable == null) return;
        if (_grabbedObjects.Count >= _inventorySize) return;

        Rigidbody2D rb = grabbable.rb;
        _grabbedObjects.Add(rb);
        cursor.SetActive(false);

        // Make object kinematic and reset physics
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        // Parent to player and position above
        rb.transform.SetParent(transform);
        
        // Stack objects visually
        float stackOffset = 1.0f;
        float itemHeight = 0.8f;
        rb.transform.localPosition = Vector3.up * (stackOffset + (_grabbedObjects.Count - 1) * itemHeight);

        // Disable colliders while grabbed
        var colliders = rb.transform.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = false;

        // Call OnGrab callback
        grabbable.OnGrab();
        PlayGrabSfx();
    }

    /// <summary>
    ///     Release the grabbed object and apply force
    /// </summary>
    private void ReleaseObj()
    {
        if (_grabbedObjects.Count == 0) return;

        // Get the last object (LIFO)
        var rb = _grabbedObjects[^1];
        _grabbedObjects.RemoveAt(_grabbedObjects.Count - 1);

        // Restore physics
        rb.isKinematic = false;
        rb.transform.SetParent(null);

        // Apply release force based on player direction
        var direction = 1f;
        if (_playerAnimation != null && _playerAnimation.spriteRenderer != null)
            direction = _playerAnimation.spriteRenderer.flipX ? -1f : 1f;
        var force = new Vector3(releaseForce.x * direction, releaseForce.y, 0);
        rb.velocity = _playerRb.velocity;
        rb.AddForce(force, ForceMode2D.Impulse);

        // Re-enable colliders
        var colliders = rb.transform.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = true;

        // Call OnRelease callback
        var grabbable = rb.GetComponent<Grabbable>();
        if (grabbable != null) grabbable.OnRelease();
        PlayReleaseSfx();
    }

    private void PlayGrabSfx()
    {
        if (_sfxManager != null)
        {
            _sfxManager.PlayClipUniversal("PlayerGrab");
        }
    }

    private void PlayReleaseSfx()
    {
        if (_sfxManager != null)
        {
            _sfxManager.PlayClipUniversal("Player Release");
        }
    }

    #region Inspector Fields

    [SerializeField] [Tooltip("Radius for grab field")]
    private float grabRange = 0.9f;

    [SerializeField] [Tooltip("Cursor GameObject to show grab target")]
    private GameObject cursor;

    [SerializeField] [Tooltip("Force applied when releasing objects")]
    private Vector3 releaseForce = Vector3.up;

    #endregion

    #region Private Fields

    private Rigidbody2D _playerRb;
    private PlayerAnimation _playerAnimation;
    private readonly List<Rigidbody2D> _grabbedObjects = new List<Rigidbody2D>();
    private readonly int _inventorySize = 4;
    private readonly Collider2D[] _overlapResults = new Collider2D[1024]; // Never trust players
    private SFXManager _sfxManager;

    #endregion

    #region IInputListener Implementation

    public void OnInputEvent(InputEvents inputEvent, InputState state)
    {
        switch (inputEvent)
        {
            case InputEvents.Grab:
                if (state == InputState.Started)
                {
                    // Grab button pressed
                    HandleGrabPressed();
                }
                break;
            
            case InputEvents.Release:
                if (state == InputState.Started)
                {
                    // Release button pressed - throw the object
                    if (_grabbedObjects.Count > 0)
                    {
                        ReleaseObj();
                    }
                }
                break;
        }
    }

    public void OnInputAxis(InputAxis axis, Vector2 value)
    {
        // Grab doesn't need axis input
    }

    public int InputPriority => 0;
    public bool IsInputEnabled => true;

    #endregion
}
