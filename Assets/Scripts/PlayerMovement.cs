using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("player init, position:" + transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("W pressed");
            transform.position += Vector3.forward * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position += Vector3.up * Time.deltaTime;
        }
    }
}
