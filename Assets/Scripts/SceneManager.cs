using UnityEngine;
using UnityEngine.InputSystem;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }
    public RoomEffect[] roomEffects;
    public RoomObject[] roomObjects;
    public Transform projectionTransform;
    public int currentEffectIndex = 0;

    public AudioClip debugAudioClip;
    private Keyboard keyboard;

    [Header("Echo Effect Controls")]
    [Tooltip("How quickly the echo volume fades out. Higher = faster fade.")]
    public float volumeDecayRate = 0.1f;

    [Tooltip("How quickly the cutoff frequency drops (muffling). Higher = faster muffling.")]
    public float cutoffDecayRate = 0.2f; // e.g., 0.2 means fully muffled in 5 seconds

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        keyboard = Keyboard.current;
        AudioDetection.Instance.OnStartSpeaking += OnStartSpeaking;

    }

    private void OnDestroy()
    {
        AudioDetection.Instance.OnStartSpeaking -= OnStartSpeaking;
    }

    // Update is called once per frame
    void Update()
    {
        if(keyboard.enterKey.wasPressedThisFrame){
            currentEffectIndex++;
            if(currentEffectIndex >= roomEffects.Length){
                currentEffectIndex = 0;
            }
        }
        
    }

    public void OnStartSpeaking(AudioClip audioClip)
    {
        Debug.Log("SceneManager: OnStartSpeaking");
        debugAudioClip = audioClip;
        roomEffects[currentEffectIndex].Activate(audioClip);
    }
}
