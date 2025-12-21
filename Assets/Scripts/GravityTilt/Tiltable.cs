using System.Collections.Generic;
using UnityEngine;

public class Tiltable : MonoBehaviour
{
    #region static

    public static readonly List<Tiltable> AllTiltables = new();

    #endregion

    public Rigidbody2D Rb => rb;
    public float Weight => weight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (!AllTiltables.Contains(this) && autoActivate) AllTiltables.Add(this);
    }

    private void OnDisable()
    {
        AllTiltables.Remove(this);
    }

    public void Activate()
    {
        AllTiltables.Add(this);
    }

    #region private variables

    [SerializeField] private float weight = 1f;

    [SerializeField] private bool autoActivate;

    [SerializeField] private Rigidbody2D rb;

    #endregion
}