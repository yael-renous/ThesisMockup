using UnityEngine;
using UnityEngine.InputSystem;
public class ScanRoomEffect : RoomEffect
{
    public Material highPitchMaterial;
    public Material mediumPitchMaterial;
    public Material lowPitchMaterial;
    private Material[] materials;
    public Transform[] originTransforms;

    public GameObject breadthScannerPrefab;
    public GameObject depthScannerPrefab;

    public AudioPitchEstimator audioPitchEstimator;

    // public ParticleSystem breadthScanner;
    // public ParticleSystem depthScanner;

    public bool breadthScannerEnabled = false;

    private float scannerDuration = 3f;
    public bool playDebug = false;

  private Keyboard keyboard;

  void Start()
    {
        materials = new Material[]{highPitchMaterial, mediumPitchMaterial, lowPitchMaterial};
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard.hKey.wasPressedThisFrame)
        {
           ActivateRoomScan(highPitchMaterial, originTransforms[0]);
        }
        if (keyboard.lKey.wasPressedThisFrame)
        {
            ActivateRoomScan(mediumPitchMaterial, originTransforms[0]);
        }
        if(keyboard.mKey.wasPressedThisFrame){
            ActivateRoomScan(lowPitchMaterial, originTransforms[0]);
        }

        if(playDebug&&keyboard.enterKey.wasPressedThisFrame){

            //random origin transform
            int randomIndex = Random.Range(0, originTransforms.Length);
            //random material
            int randomMaterialIndex = Random.Range(0, materials.Length);
            ActivateRoomScan(materials[randomMaterialIndex], originTransforms[randomIndex]);
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

    private void ActivateRoomScan(Material material, Transform originTransform){
        if(breadthScannerEnabled){
            ActivateBreadthScanner(material, originTransform);
        }
        else{
            ActivateDepthScanner(originTransform);
        }
    }
    private void ActivateDepthScanner(Transform originTransform){
        GameObject depthScanner = Instantiate(depthScannerPrefab, Vector3.zero, Quaternion.identity, parent:originTransform);

        depthScanner.GetComponent<ParticleSystem>().Play();
        // Destroy(depthScanner, scannerDuration);
    }
    private void ActivateBreadthScanner(Material material, Transform originTransform){
        GameObject breadthScanner = Instantiate(breadthScannerPrefab, originTransform.position, Quaternion.identity);
        ParticleSystem particleSystem = breadthScanner.GetComponent<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = breadthScanner.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = material;
        particleSystem.Play();
        Destroy(breadthScanner, scannerDuration);
    }
    
}
