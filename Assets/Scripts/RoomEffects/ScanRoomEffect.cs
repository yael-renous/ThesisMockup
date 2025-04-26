using UnityEngine;
using UnityEngine.InputSystem;
public class ScanRoomEffect : RoomEffect
{
    public Material highPitchMaterial;
    public Material mediumPitchMaterial;
    public Material lowPitchMaterial;
    public Transform[] originTransforms;

    public GameObject breadthScannerPrefab;
    public GameObject depthScannerPrefab;

    public AudioPitchEstimator audioPitchEstimator;

    // public ParticleSystem breadthScanner;
    // public ParticleSystem depthScanner;

    public bool breadthScannerEnabled = false;

    private float scannerDuration = 3f;

  private Keyboard keyboard;

  void Start()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard.hKey.wasPressedThisFrame)
        {
           ActivateRoomScan(highPitchMaterial);
        }
        if (keyboard.lKey.wasPressedThisFrame)
        {
            ActivateRoomScan(mediumPitchMaterial);
        }
        if(keyboard.mKey.wasPressedThisFrame){
            ActivateRoomScan(lowPitchMaterial);
        }
    }

    private AudioSource debugAudioSource;
    public override void Activate(AudioClip audioClip)
    {
        // Debug.Log("ScanRoomEffect activated");
        // //check audio clip pitch
        // //create audio source
        // if(debugAudioSource == null){
        //     debugAudioSource = gameObject.AddComponent<AudioSource>();
        //     debugAudioSource.clip = audioClip;
        //     debugAudioSource.Play();
        //     debugAudioSource.loop = true;
        //     debugAudioSource.volume = 1f;
        // }
        // // Debug.Log(audioClip.frequency);
        // float pitch = audioPitchEstimator.Estimate(debugAudioSource);
        // Debug.Log(pitch);
    }

    private void ActivateRoomScan(Material material){
        if(breadthScannerEnabled){
            ActivateBreadthScanner(material);
        }
        else{
            ActivateDepthScanner();
        }
    }
    private void ActivateDepthScanner(){
        GameObject depthScanner = Instantiate(depthScannerPrefab, Vector3.zero, Quaternion.identity, parent:originTransforms[0]);

        depthScanner.GetComponent<ParticleSystem>().Play();
        // Destroy(depthScanner, scannerDuration);
    }
    private void ActivateBreadthScanner(Material material)
    {
        GameObject breadthScanner = Instantiate(breadthScannerPrefab, originTransforms[0].position, Quaternion.identity);
        ParticleSystem particleSystem = breadthScanner.GetComponent<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = breadthScanner.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = material;
        particleSystem.Play();
        Destroy(breadthScanner, scannerDuration);
    }
    
}
