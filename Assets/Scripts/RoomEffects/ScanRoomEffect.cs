using UnityEngine;
using UnityEngine.InputSystem;
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

  void Start()
    {
        materials = new Material[]{highPitchMaterial, mediumPitchMaterial, lowPitchMaterial};
        keyboard = Keyboard.current;
    }

    void Update()
    {
      

    }

    public override void Activate(AudioClip audioClip)
    {
      ActivateRoomScan(highPitchMaterial, SceneManager.Instance.projectionTransform);
    }

    private void ActivateRoomScan(Material material, Transform originTransform){
        GameObject scanner = Instantiate(scannerPrefab, originTransform.position, Quaternion.identity);
        ParticleSystem particleSystem = scanner.GetComponent<ParticleSystem>();
        ParticleSystemRenderer particleRenderer = scanner.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = material;
        particleSystem.Play();
        Destroy(scanner, scannerDuration);
    }    
}
