using UnityEngine;

public class autorotate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Rotation speed for each axis
    public float rotationSpeedX = 30f; // degrees per second
    public float rotationSpeedY = 45f; // degrees per second

    // Update is called once per frame
    void Update()
    {
        // Calculate rotation for this frame
        float rotX = rotationSpeedX * Time.deltaTime;
        float rotY = rotationSpeedY * Time.deltaTime;

        // Apply rotation around X and Y axes
        transform.Rotate(rotX, rotY, 0f);
    }
}
