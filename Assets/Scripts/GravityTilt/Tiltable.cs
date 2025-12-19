using System.Collections.Generic;
using UnityEngine;

public class Tiltable : MonoBehaviour
{
    public static readonly List<Tiltable> AllTiltables = new List<Tiltable>();

    public float weight = 1f;
    
    private Rigidbody2D _rb;
    public Rigidbody2D Rb => _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (!AllTiltables.Contains(this))
        {
            AllTiltables.Add(this);
        }
    }

    private void OnDisable()
    {
        AllTiltables.Remove(this);
    }
}
