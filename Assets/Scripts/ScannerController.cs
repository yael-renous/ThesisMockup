using System.Collections.Generic;
using UnityEngine;

public class ScannerController : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public RoomObject targetObjectA;  // First target object
    public RoomObject targetObjectB;  // Second target object
    public float distanceThreshold = 1.8f;
    private void Start()
    {
        // Get particle system if not assigned
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError("No ParticleSystem found on this GameObject!");
                return;
            }
        }

        // Verify target objects
        if (targetObjectA == null || targetObjectB == null)
        {
            Debug.LogError("Both target objects must be assigned!");
            return;
        }

        Debug.Log("ParticleTriggerDetector initialized successfully");
    }

    private void OnParticleTrigger()
    {
        if (particleSystem == null || targetObjectA == null || targetObjectB == null) return;

        List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
        int numParticles = particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        if (numParticles > 0)
        {
            Vector3 particlePos = particles[0].position;
            
            // Check distance to both target objects
            float distanceToA = Vector3.Distance(particlePos, targetObjectA.transform.position);
            float distanceToB = Vector3.Distance(particlePos, targetObjectB.transform.position);
            
            Debug.Log($"Particle position: {particlePos}");
            Debug.Log($"Distance to A: {distanceToA}, Distance to B: {distanceToB}");
            
            // Get the audio ID from the particle system's name
            if (int.TryParse(gameObject.name, out int audioId))
            {
                // Check which object is closer and trigger its play function
                if (distanceToA < distanceToB && distanceToA < distanceThreshold)
                {
                    Debug.Log("Particle triggered target A!");
                    targetObjectA.play(audioId);
                }
                else if (distanceToB < distanceThreshold)
                {
                    Debug.Log("Particle triggered target B!");
                    targetObjectB.play(audioId);
                }
            }
        }
    }
}