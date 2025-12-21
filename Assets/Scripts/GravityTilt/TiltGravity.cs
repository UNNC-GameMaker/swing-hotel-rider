using Managers;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float gravity = 4f;
    private TiltManager _manager;

    private void Start()
    {
        _manager = GameManager.Instance.GetManager<TiltManager>();
    }

    private void FixedUpdate()
    {
        var forceX = gravity * Mathf.Sin(_manager.TiltTarget * Mathf.Deg2Rad);
        var forceY = -gravity * Mathf.Sin(_manager.TiltTarget * Mathf.Deg2Rad);

        forceY += gravity;

        var force = new Vector2(forceX, forceY);

        foreach (var tiltable in Tiltable.AllTiltables)
        {
            var rb = tiltable.Rb;
            if (rb) rb.AddForce(force * rb.mass, ForceMode2D.Impulse);
        }
    }
}