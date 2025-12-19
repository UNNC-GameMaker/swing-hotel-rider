using Managers;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    private TiltManager _manager;
    [SerializeField] private float gravity = 4f;

    void Start()
    {
        _manager = GameManager.Instance.GetManager<TiltManager>();
    }

    void FixedUpdate()
    {
        float forceX = gravity * Mathf.Sin(_manager.tiltTarget * Mathf.Deg2Rad);
        float forceY = -gravity * Mathf.Sin(_manager.tiltTarget * Mathf.Deg2Rad);

        forceY += gravity;
        
        Vector2 force =  new Vector2(forceX, forceY);

        foreach (var tiltable in Tiltable.AllTiltables)
        {
            Rigidbody2D rb = tiltable.Rb;
            if (rb)
            {
                rb.AddForce(force * rb.mass, ForceMode2D.Impulse);
            }
        }
    }
}
