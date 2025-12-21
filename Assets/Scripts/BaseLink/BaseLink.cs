using UnityEngine;

public class BaseLink : MonoBehaviour
{
    private float tiltAngle;

    // Start is called before the first frame update
    private void Start()
    {
        tiltAngle = 0;
        Debug.Log("BaseLink Init");
    }

    // Update is called once per frame
    private void Update()
    {
    }
}