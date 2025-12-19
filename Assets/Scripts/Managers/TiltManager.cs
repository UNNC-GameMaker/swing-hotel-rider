using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class TiltManager : Manager
{
    
    public float tiltFactor = 100f;
    
    public float tiltTarget;

    public float tiltSpeed;

    public float totalTilt;
    
    
    [SerializeField] private readonly float _centerX = 10f;
    
    public override void Init()
    {
        Debug.Log("TiltManager Init");
        GameManager.Instance.RegisterManager(this);
    }

    void Update()
    {
        foreach (var tiltObject in Tiltable.AllTiltables)
        {
            totalTilt += (tiltObject.transform.position.x - _centerX) * tiltObject.weight;
        }
        
        tiltTarget = Mathf.Lerp(tiltTarget, totalTilt / tiltFactor, tiltSpeed * Time.deltaTime);

    }
}
