using UnityEngine;
using System;
using System.Collections;

public class AudioDetection : MonoBehaviour
{
    public static AudioDetection Instance { get; private set; }
    public RecordAudio recordAudio;
    public int sampleWindow = 64;
    private AudioClip microphoneClip;

    public float minThreshold = 0.01f;
    public float maxThreshold = 1.0f;

    private bool isSpeaking = false;
    private string micName;
    public Action<AudioClip> OnStartSpeaking;


    // public float loudnessSensitivity = 100f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the instance alive across scenes
        }
        else
        {
            Destroy(gameObject); // Ensures only one instance exists
        }
    }

    void Start()
    {
        micName=Microphone.devices[0];
        SetupMicrophone();
    }

    // Update is called once per frame
    void Update()
    {
        checkSpeaking();
    }

    private void checkSpeaking()
    {
        float loudness = GetLoudnessFromMicrophone();
        if (loudness > minThreshold)
        {
            isSpeaking = true;
            Debug.Log("speaking");
            Microphone.End(micName);
           recordAudio.StartRecording();
           Invoke("StopRecording", 20f);
        }
        // else if(isSpeaking)
        // {
        //     Debug.Log("not speaking");
        //     MicRecorder.Instance.StopRecording();
        //     isSpeaking = false;
        // }
    }

    private void StopRecording()
    {
        recordAudio.StopRecording();
    }

    private void StopAudio()
    {
       SetupMicrophone();
    }

    private void CaptureAudioClip()
    {
        if (Microphone.IsRecording(micName))
        {
            Microphone.End(micName);
        }

        micName = Microphone.devices[0];
        microphoneClip = Microphone.Start(micName, false, 20, AudioSettings.outputSampleRate);
        Debug.Log("New audio clip started-- " + micName);

        // Wait for the microphone to start recording
        StartCoroutine(WaitForMicrophoneToStart());
    }

    private IEnumerator WaitForMicrophoneToStart()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to ensure microphone has started
        OnStartSpeaking?.Invoke(microphoneClip);
    }

    public void SetupMicrophone()
    {
        // micName = Microphone.devices[0];
        if (Microphone.IsRecording(micName))
        {
            Microphone.End(micName);
        }
        int sampleRate = 44100;
        microphoneClip = Microphone.Start(micName, true, 20, AudioSettings.outputSampleRate);
        Debug.Log("Microphone started-- " + micName);
    }

    public float GetLoudnessFromMicrophone()
    {
        float loudness = GetLoudnessFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), microphoneClip);
        return loudness;
    }

    public float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;
        if (startPosition < 0)
        {
            startPosition = 0;
        }

        float[] waveData = new float[sampleWindow];
      
        bool success = clip.GetData(waveData, startPosition);
        if (!success)
        {
            Debug.LogError("startPosition: " + startPosition);
            Debug.LogError("clipPosition: " + clipPosition);
            Debug.LogError("sampleWindow: " + sampleWindow);
            return 0;
        }

        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]);
        }
        float averageLoudness = totalLoudness / sampleWindow;
        // Debug.Log("averageLoudness-- " + averageLoudness);
        return averageLoudness;
    }
}
