using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("quit");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))

        {
            UnityEditor.EditorApplication.isPlaying = false;

            //Application.Quit();

        } 
    }
}
