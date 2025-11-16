using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLink : MonoBehaviour
{
    private float tiltAngle;
    // Start is called before the first frame update
    void Start()
    {
        tiltAngle = 0;
        Debug.Log("BaseLink Init");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
