using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScanRoomEffect : RoomEffect
{
    public Material highPitchMaterial;
    public Material mediumPitchMaterial;
    public Material lowPitchMaterial;
    private Material[] materials;

    public GameObject scannerPrefab;

    private float scannerDuration = 3f;
    [SerializeField] private bool playDebug = false;
    private Keyboard keyboard;

    // Find all RoomObjects in the scene
    public RoomObject[] roomObjects;
    private Dictionary<RoomObject, ParticleSystem> scannerMap;

    void Start()
    {
        materials = new Material[]{highPitchMaterial, mediumPitchMaterial, lowPitchMaterial};
        keyboard = Keyboard.current;
        
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
        GameObject scanner = Instantiate(scannerPrefab, SceneManager.Instance.projectionTransform.position, Quaternion.identity);
        scanner.name = "Scanner_" + targetObject.name;
        
        // Get and configure the ScannerController
        ScannerController scannerController = scanner.GetComponent<ScannerController>();
        if (scannerController != null)
        {
            scannerController.init(targetObject, 0); // We'll update the audioId when activating
            
            // Store particle system reference
            ParticleSystem particleSystem = scanner.GetComponent<ParticleSystem>();
            scannerMap[targetObject] = particleSystem;
            
            // Configure the particle system
            ParticleSystemRenderer particleRenderer = scanner.GetComponent<ParticleSystemRenderer>();
            particleRenderer.material = highPitchMaterial;
            
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
