using UnityEngine;

public class CffRoomEffectDemo : RoomEffect
{
    public FlickeringLight[] flickeringLights;
    // public Transform[] RoomObjects;
    public float intensity = 0.04f;
    public float frequency = 10f;
    // public float rotationSpeed = 10f;

    // public enum SyncMode { Direct, Inverse, Phase, Random, Harmonic, Stroboscopic }
    // public SyncMode syncMode = SyncMode.Direct;

    private float previousIntensity;
    private float previousFrequency;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousIntensity = intensity;
        previousFrequency = frequency;
        // foreach (var light in flickeringLights)
        // {
        //     light.intensity = intensity;
        // }
    }

    void FixedUpdate()
    {
        if (intensity != previousIntensity || frequency != previousFrequency)
        {
            foreach (var light in flickeringLights)
            {
                light.SetIntensity(intensity);
                light.SetFrequency(frequency);
            }
            previousIntensity = intensity;
            previousFrequency = frequency;
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     switch (syncMode)
    //     {
    //         case SyncMode.Direct:
    //             DirectSynchronization();
    //             break;
    //         case SyncMode.Inverse:
    //             InverseSynchronization();
    //             break;
    //         case SyncMode.Phase:
    //             PhaseSynchronization();
    //             break;
    //         case SyncMode.Random:
    //             RandomSynchronization();
    //             break;
    //         case SyncMode.Harmonic:
    //             HarmonicSynchronization();
    //             break;
    //         case SyncMode.Stroboscopic:
    //             StroboscopicSynchronization();
    //             break;
    //     }

    //     foreach (var obj in RoomObjects)
    //     {
    //         obj.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
    //     }
    // }

    // void DirectSynchronization()
    // {
    //     rotationSpeed = frequency * 1.0f; // Adjust the factor as needed
    // }

    // void InverseSynchronization()
    // {
    //     rotationSpeed = 1.0f / frequency * 10.0f; // Adjust the factor as needed
    // }

    // void PhaseSynchronization()
    // {
    //     float phaseOffset = Mathf.Sin(Time.time * frequency) * 5.0f; // Example phase modulation
    //     rotationSpeed = frequency + phaseOffset;
    // }

    // void RandomSynchronization()
    // {
    //     rotationSpeed = Random.Range(5.0f, 15.0f); // Example range
    // }

    // void HarmonicSynchronization()
    // {
    //     rotationSpeed = frequency * 2.0f; // Example harmonic relationship
    // }

    // void StroboscopicSynchronization()
    // {
    //     // Set the flicker frequency to match the rotation speed
    //     frequency = rotationSpeed;
    //     foreach (var light in flickeringLights)
    //     {
    //         light.frequency = frequency;
    //     }
    // }

    public override void Activate(AudioClip audioClip)
    {
        Debug.Log("cffRoomEffect activated");
    }
}
