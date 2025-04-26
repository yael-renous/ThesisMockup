using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    Light light;
    public float frequency = 5f; // Frequency in Hz (toggles per second)
    public float intensity = 0.01f; 
    private float timer = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();
        light.intensity = intensity;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1.0f / frequency)
        {
            light.enabled = !light.enabled; // Toggle the light
            timer = 0.0f; // Reset the timer
        }
    }
}
