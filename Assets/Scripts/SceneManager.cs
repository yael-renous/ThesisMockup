using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }
    public RoomEffect[] roomEffects;

    public int currentRoomIndex = 1;

    public AudioClip debugAudioClip;

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
        AudioDetection.Instance.OnStartSpeaking += OnStartSpeaking;

    }

    private void OnDestroy()
    {
        AudioDetection.Instance.OnStartSpeaking -= OnStartSpeaking;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartSpeaking(AudioClip audioClip)
    {
        Debug.Log("SceneManager: OnStartSpeaking");
        roomEffects[currentRoomIndex].Activate(audioClip);
    }
}
