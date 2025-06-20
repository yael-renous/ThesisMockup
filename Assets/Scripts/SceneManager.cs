using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour
{
    #region Singleton
    public static SceneManager Instance { get; private set; }
    #endregion

    #region Inspector Variables
    [Header("Room Effects")]
    public RoomEffect[] roomEffects;
    public Transform micTransform;
    public int currentEffectIndex = 0;
    [Tooltip("Time in minutes between effect changes")]
    public float effectChangeInterval = 5f;

    [Header("Background Effects")]
    public BackgroundEffect[] backgroundEffects;
    public int currentBackgroundEffectIndex = 0;
    // public float backgroundEffectChangeInterval = 10f;
    
    [Header("Debug Controls")]
    [Tooltip("Enable debug mode to manually control effects")]
    public bool debugMode = false;
    [Tooltip("Manual control of room effect index (only works in debug mode)")]
    public int debugRoomEffectIndex = 0;
    [Tooltip("Manual control of background effect index (only works in debug mode)")]
    public int debugBackgroundEffectIndex = 0;
    
    [Header("Audio Settings")]
    public int chirpAudioId = -10;
    public AudioClip chirpAudioClip;
    public AudioClip debugAudioClip; //deprecated

    [Header("Visual Effects")]
    public ParticleSystem[] batScanners;

    [Header("Static Mode Settings")]
    [Tooltip("Time in seconds of silence before entering static mode")]
    public float staticModeThreshold = 5f;
    [Tooltip("Time in seconds between bat scanner sequences")]
    public float batScannerInterval = 10f;
    #endregion

    #region Private Variables
    private Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
    private int nextAudioId = 0;
    private Keyboard keyboard;
    private float lastEffectChangeTime = 0f;
    private float lastBackgroundEffectChangeTime = 0f;
    // Static Mode Variables
    private float lastCheckTime = 0f;
    public float checkInterval = 2f; // Check every second
    private bool staticMode = true;
    private int batScannerPlayCount = 0;
    private float lastBatScannerTime = 0f;
    private const int MAX_BAT_SCANNER_PLAYS = 4;
    #endregion

    #region Unity Lifecycle Methods
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
        EnterStaticMode();
        lastEffectChangeTime = Time.time;
        lastBackgroundEffectChangeTime = Time.time;
        backgroundEffects[currentBackgroundEffectIndex].activate();
    }

    private void OnDestroy()
    {
        AudioDetection.Instance.OnStartSpeaking -= OnStartSpeaking;
    }

    void Update()
    {
        if(debugMode)
        {
            HandleKeyboardInput();
        }
        else
        {
            HandleEffectChange();
            HandleStaticModeCheck();
            HandleBackgroundEffectChange();
        }
    }
    #endregion

    #region Input Handling
    private void HandleKeyboardInput()
    {
        if(keyboard.enterKey.wasPressedThisFrame)
        {
     
                // Apply debug indices from inspector
                SetRoomEffect(debugRoomEffectIndex);
                SetBackgroundEffect(debugBackgroundEffectIndex);
        }
    }

    private void SetRoomEffect(int index)
    {
        if (index >= 0 && index < roomEffects.Length)
        {
            currentEffectIndex = index;
            debugRoomEffectIndex = index;
            Debug.Log($"SceneManager: Debug - Set room effect to index {index}");
        }
        else
        {
            Debug.LogWarning($"SceneManager: Debug - Invalid room effect index: {index}. Valid range: 0-{roomEffects.Length - 1}");
        }
    }

    private void SetBackgroundEffect(int index)
    {
        if (index >= 0 && index < backgroundEffects.Length)
        {
            backgroundEffects[currentBackgroundEffectIndex].deactivate();
            currentBackgroundEffectIndex = index;
            debugBackgroundEffectIndex = index;
            backgroundEffects[currentBackgroundEffectIndex].activate();
            Debug.Log($"SceneManager: Debug - Set background effect to index {index}");
        }
        else
        {
            Debug.LogWarning($"SceneManager: Debug - Invalid background effect index: {index}. Valid range: 0-{backgroundEffects.Length - 1}");
        }
    }
    #endregion

    #region Static Mode Logic
    private void HandleStaticModeCheck()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            if(!staticMode)
            {
                CheckForStaticMode();
            }
            else
            {
                // PlayStaticBatScanners();
            }
            lastCheckTime = Time.time;
        }
    }

    private void CheckForStaticMode()
    {
        float timeSinceLastRecording = Time.time - AudioDetection.Instance.SoundStartTime;
        if (timeSinceLastRecording >= staticModeThreshold)
        {
            EnterStaticMode();
        }
    }

    private void EnterStaticMode()
    {
        staticMode = true;
        // Reset bat scanner sequence
        batScannerPlayCount = 0;
        lastBatScannerTime = Time.time;
        currentEffectIndex = 0;
    }

    public void PlayStaticBatScanners()
    {
        float currentTime = Time.time;
        Debug.Log($"SceneManager: PlayStaticBatScanners: {batScannerPlayCount}");
        // If we haven't played any bat scanners yet or enough time has passed since the last sequence
        if (batScannerPlayCount == 0 || (currentTime - lastBatScannerTime >= batScannerInterval))
        {
            // Reset the sequence
            batScannerPlayCount = 0;
            lastBatScannerTime = currentTime;
        }

        // If we haven't played all 4 times in the current sequence
        if (batScannerPlayCount < MAX_BAT_SCANNER_PLAYS)
        {
            foreach (var scanner in batScanners)
            {
                scanner.Play();
            }
            batScannerPlayCount++;
        }
    }
    #endregion

    #region Audio Management
    public void OnStartSpeaking(AudioClip audioClip)
    {
        staticMode = false;
        int audioId = nextAudioId++;
        audioClips[audioId] = audioClip;
        roomEffects[currentEffectIndex].Activate(audioId);
    }

    public AudioClip GetAudioClip(int audioId)
    {
        if(audioId == chirpAudioId)
        {
            return chirpAudioClip;
        }
        if (audioClips.TryGetValue(audioId, out AudioClip clip))
        {
            return clip;
        }
        return debugAudioClip; //TODO: remove this
    }

    public AudioClip GetRandomAudioClip()
    {
        if(audioClips.Count == 0)
        {
            return null;
        }
        int randomIndex = Random.Range(0, audioClips.Count);
        Debug.Log($"SceneManager: GetRandomAudioClip: {randomIndex}");
        Debug.Log($"SceneManager: GetRandomAudioClip: {audioClips[randomIndex].name}");
        return audioClips[randomIndex];
    }

    #endregion

    #region Effect Management
    private void HandleEffectChange()
    {
        // Skip automatic effect changes if debug mode is enabled
        if (debugMode)
            return;

        float currentTime = Time.time;
        float timeSinceLastChange = (currentTime - lastEffectChangeTime) / 60f; // Convert to minutes

        if (timeSinceLastChange >= effectChangeInterval)
        {
            ChangeEffect();
            lastEffectChangeTime = currentTime;
        }
    }

    private void ChangeEffect()
    {
        effectChangeInterval = Random.Range(0.6f, 2f);
        currentEffectIndex = (currentEffectIndex + 1) % roomEffects.Length;
        Debug.Log($"SceneManager: Changed effect to index {currentEffectIndex}");
    }

    private void HandleBackgroundEffectChange()
    {
        // Skip automatic background effect changes if debug mode is enabled
        if (debugMode)
            return;

        float currentTime = Time.time;
        float timeSinceLastChange = (currentTime - lastBackgroundEffectChangeTime) / 60f; // Convert to minutes

        if (timeSinceLastChange >= backgroundEffects[currentBackgroundEffectIndex].getDuration())
        {
            ChangeBackgroundEffect();
            lastBackgroundEffectChangeTime = currentTime;
        }
    }

    private void ChangeBackgroundEffect()
    {
        backgroundEffects[currentBackgroundEffectIndex].deactivate();
        currentBackgroundEffectIndex = (currentBackgroundEffectIndex + 1) % backgroundEffects.Length;
        backgroundEffects[currentBackgroundEffectIndex].activate();

        if (currentBackgroundEffectIndex >= backgroundEffects.Length)
        {
            currentBackgroundEffectIndex = 0;
        }
    }
    #endregion
}
