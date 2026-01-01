using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGController : MonoBehaviour
{
    public Transform Cam;


    public float factorX;
    public float factorY;

    public float offsetX;
    public float offsetY;
    void Update()
    {
        transform.localPosition = new Vector3(Cam.position.x * factorX + offsetX, Cam.position.y * factorY + offsetY, 0);
    }
}
