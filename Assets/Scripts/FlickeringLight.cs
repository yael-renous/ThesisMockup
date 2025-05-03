using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    Light light;
    private float frequency = 5f; // Frequency in Hz (toggles per second)
    public float intensity = 0.01f; 
    private float timer = 0.0f;



public int fixedFramesPerFlash = 10; // How many FixedUpdates before flipping light

private int fixedFrameCounter = 0;

// void FixedUpdate()
// {
//     fixedFrameCounter++;

//     if (fixedFrameCounter >= fixedFramesPerFlash )
//     {
//         light.enabled = !light.enabled; // Toggle the light
//         fixedFrameCounter = 0;
//     }
// }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();
        light.intensity = intensity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer >= 1.0f / frequency/2)
        {
            light.enabled = !light.enabled; // Toggle the light
            timer = 0.0f; // Reset the timer
        }
    }

    public void SetIntensity(float intensity)
    {
        this.intensity = intensity;
        light.intensity = intensity;
    }

    public void SetFrequency(float frequency)
    {
        this.fixedFramesPerFlash = (int)(1.0f / frequency);
        this.frequency = frequency;
    }
}
