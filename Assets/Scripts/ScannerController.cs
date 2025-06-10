using UnityEngine;
using System.Linq;

public class ScannerController : MonoBehaviour
{
    public int audioId;
    public float raycastDistance = 0.5f; // Distance to check for collisions
    public int raycastCount = 8; // Number of rays to cast in a circle
    public LayerMask collisionMask; // Layer mask for objects to detect
    public float checkInterval = 0.1f; // How often to check for collisions
    public bool showDebugLogs = true; // Toggle for debug logs
    public bool showDebugRays = true; // Toggle for debug ray visualization
    public Color rayColor = Color.yellow; // Color for debug rays

    private ParticleSystem particleSystem;
    private float currentRadius;
    private ParticleSystem.Particle[] particles;
    private Vector3[] raycastDirections;
    private float lastCheckTime;
    private float lastRadius;
    private int totalCollisions = 0;
    private float lastLogTime;
    private const float STATS_INTERVAL = 5f; // Log stats every 5 seconds

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            Debug.LogError("No ParticleSystem found on this GameObject!");
            return;
        }
        
        particles = new ParticleSystem.Particle[1];
        
        // Pre-calculate raycast directions
        raycastDirections = new Vector3[raycastCount];
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = i * (360f / raycastCount);
            raycastDirections[i] = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        }

        if (showDebugLogs)
        {
            Debug.Log($"ScannerController initialized with {raycastCount} raycast directions");
            Debug.Log($"Collision check interval: {checkInterval}s");
            Debug.Log($"Collision mask: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(collisionMask.value, 2)))}");
            
            // Check if any objects are on the collision mask layers
            var objectsInLayer = FindObjectsOfType<GameObject>()
                .Where(go => (collisionMask.value & (1 << go.layer)) != 0)
                .Select(go => go.name)
                .ToArray();
            
            if (objectsInLayer.Length > 0)
            {
                Debug.Log($"Found {objectsInLayer.Length} objects on collision mask layers:");
                foreach (var objName in objectsInLayer)
                {
                    Debug.Log($"- {objName}");
                }
            }
            else
            {
                Debug.LogWarning("No objects found on the collision mask layers! Make sure your target objects are on the correct layers.");
            }
        }
    }

    void Update()
    {
        if (particleSystem == null) return;

        // Get current particle size
        int numParticles = particleSystem.GetParticles(particles);
        if (numParticles == 0) return;

        float newRadius = particles[0].GetCurrentSize(particleSystem) * 0.5f;
        

        currentRadius = newRadius;

        // Only check for collisions if enough time has passed and the radius has changed
        if (Time.time >= lastCheckTime + checkInterval && !Mathf.Approximately(currentRadius, lastRadius))
        {
            CheckCollisions();
            lastCheckTime = Time.time;
            lastRadius = currentRadius;
        }

   
     
    }

    private void CheckCollisions()
    {
     

        bool collisionDetected = false;
        // Use cached directions for raycasts
        for (int i = 0; i < raycastCount; i++)
        {
            if (showDebugRays)
            {
                Debug.DrawRay(transform.position, raycastDirections[i] * currentRadius, rayColor);
            }

            if (Physics.Raycast(transform.position, raycastDirections[i], out RaycastHit hit, currentRadius, collisionMask))
            {
                collisionDetected = true;
                totalCollisions++;
                Debug.Log($"Collision #{totalCollisions} with {hit.collider.gameObject.name} at distance {hit.distance:F3}");

                if (showDebugLogs)
                {
                    Debug.Log($"Collision #{totalCollisions} with {hit.collider.gameObject.name} at distance {hit.distance:F3}");
                    Debug.Log($"- Object layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                    Debug.Log($"- Has collider: {hit.collider != null}");
                }
            }
        }

        if (showDebugLogs && !collisionDetected)
        {
            Debug.Log("No collisions detected in this check");
        }
    }



    // public void SetAudioClipId(int audioId){
    //     this.audioId = audioId;
    // }

    // void OnParticleCollision(GameObject other)
    // {
    //     Debug.Log($"ScannerController: OnParticleCollision: {audioId}");
    // }
}
