using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{   
    public static GroundCheck Instance { get; private set; }
    
    public Vector2 Position2D => transform.position;
    
    public Transform groundCheck => transform;

    GroundCheck()
    {
        Instance = this;
    }

    void Awake()
    {
        Instance = this;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
