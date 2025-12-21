using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private GroundCheck()
    {
        Instance = this;
    }

    public static GroundCheck Instance { get; private set; }

    public Vector2 Position2D => transform.position;

    public Transform FootCollider => transform;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}