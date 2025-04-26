using UnityEngine;
using System;
using System.Collections;

public class AudioDetection : MonoBehaviour
{

    //TODO- send the correct audio - check audioclip.create to create a copy of the "recording"
    // use the clip position to send the correct audio (like in loudness check)
    public static AudioDetection Instance { get; private set; }
    public RecordAudio recordAudio;
    public int sampleWindow = 64;
    private AudioClip microphoneClip;

    public float minThreshold = 0.01f;
    public float maxThreshold = 1.0f;
    public int recordingTime = 3455; //TODO- always creates 5 second clip even if user is speaking for less or more

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
            CaptureAudioClip();
        //    recordAudio.StartRecording();
        //    Invoke("StopRecording", 20f);
        }
        else if(isSpeaking)
        {
            Debug.Log("not speaking");
            StopAudio();
            // StopRecording();
            // MicRecorder.Instance.StopRecording();
            isSpeaking = false;
        }
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
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        micName = Microphone.devices[0];
        microphoneClip = Microphone.Start(micName, false, recordingTime, AudioSettings.outputSampleRate); //TODO- always creates 5 second clip even if user is speaking for less or more
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
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
        // int sampleRate = 44100;
        microphoneClip = Microphone.Start(micName, true, recordingTime, AudioSettings.outputSampleRate);
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
