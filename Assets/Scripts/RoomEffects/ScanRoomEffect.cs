using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScanRoomEffect : RoomEffect
{

    public Material[] materials;

    public GameObject scannerPrefab;

    private float scannerDuration=10f;

    public bool spotlightVersion = false;

    // Find all RoomObjects in the scene
    public RoomObject[] roomObjects;
    private Dictionary<RoomObject, ParticleSystem> scannerMap;

    void Start()
    {
       
        
        // Initialize scanners for all room objects
       
        scannerMap = new Dictionary<RoomObject, ParticleSystem>();
        
       CreateScanners();
    }

    private void CreateScanners()
    {
        foreach (RoomObject roomObject in roomObjects)
        {
            CreateScanner(roomObject);
        }
    }

    private void CreateScanner(RoomObject targetObject)
    {
        GameObject scanner = Instantiate(scannerPrefab, SceneManager.Instance.micTransform.position, Quaternion.identity);
        scanner.name = "Scanner_" + targetObject.name;
        
        // Set the scanner's layer to match the target object's layer (not including children)
        scanner.layer = targetObject.gameObject.layer;
        
        // Get and configure the ScannerController
        ScannerController scannerController = scanner.GetComponent<ScannerController>();
        if (scannerController != null)
        {
            scannerController.init(targetObject, 0, spotlightVersion); // We'll update the audioId when activating
            
            // Store particle system reference
            ParticleSystem particleSystem = scanner.GetComponent<ParticleSystem>();
            scannerMap[targetObject] = particleSystem;
            
            // Configure the particle system
            ParticleSystemRenderer particleRenderer = scanner.GetComponent<ParticleSystemRenderer>();
            //randomly select a material
            if(!spotlightVersion){
                particleRenderer.material = materials[Random.Range(0, materials.Length)];
            }
            // Stop the particle system initially
            particleSystem.Stop();
        }
    }

    void Update()
    {
    }

    public override void Activate(int audioId)
    {
        foreach (var kvp in scannerMap)
        {
            RoomObject targetObject = kvp.Key;
            ParticleSystem particleSystem = kvp.Value;
            
            // Update the scanner's name with the new audioId
            particleSystem.gameObject.name = audioId.ToString();
            
            // Play the particle system
            particleSystem.Play();
            
            // Destroy the scanner after duration
            Destroy(particleSystem.gameObject, scannerDuration);
                        
        }
        CreateScanners();
    }
}
