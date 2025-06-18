using System.Collections.Generic;
using UnityEngine;

public class ScannerController : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public RoomObject targetObject;
    public int audioId;

    public bool spotlightVersion = false;


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

        // Verify target object
        if (targetObject == null)
        {
            Debug.LogError("No target object assigned!");
            return;
        }

        // Debug.Log("ScannerController initialized successfully");
    }

    public void init(RoomObject targetObject, int audioId, bool spotlightVersion)
    {
        this.targetObject = targetObject;
        this.audioId = audioId;
        this.spotlightVersion = spotlightVersion;
        // Configure particle system trigger
        var trigger = particleSystem.trigger;

        trigger.AddCollider(targetObject.GetComponent<Collider>());
    }

    private void OnParticleTrigger()
    {
        // Debug.Log("OnParticleTrigger called");
        if (particleSystem == null || targetObject == null) return;


            // Get the audio ID from the particle system's name
            if (int.TryParse(gameObject.name, out int audioId))
            {
                if(spotlightVersion){
                    targetObject.showSpotlightandPlay(audioId);
                }
                else{
                    targetObject.play(audioId);
                }   
            }
    }
}