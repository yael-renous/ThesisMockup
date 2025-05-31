using UnityEngine;
using System;
using System.Collections;

public class AudioDetection : MonoBehaviour
{

    //TODO- send the correct audio - check audioclip.create to create a copy of the "recording"
    // use the clip position to send the correct audio (like in loudness check)
    public static AudioDetection Instance { get; private set; }
   

    // === Configurable Parameters ===
    [Header("Audio Detection Parameters")]
    public int rollingBufferLengthSeconds = 10; // Length of the rolling audio buffer in seconds
    public float silenceDurationToStop = 1.0f; // Duration (seconds) of silence to consider as stop speaking
    public float minThreshold = 0.01f; // Loudness threshold to detect speaking
    public int sampleWindow = 64; // Number of samples to check for loudness
    public float segmentDurationSeconds = 2.0f; // Duration of each speech segment in seconds

    // === Internal State ===
    private AudioClip microphoneClip;
    private string micName;
    private int sampleRate;
    private bool isSpeaking = false;
    private float lastLoudTime = 0f;
    private int speakStartPosition = -1;
    private int speakEndPosition = -1;
    private float speakStartTime = 0f;
    private float speakEndTime = 0f;
    private float segmentTimer = 0f;
    private int lastSegmentEndPosition = 0;
    public Action<AudioClip> OnStartSpeaking;
    private bool isSpeechTimerActive = false;
    private int speechStartSample = -1;
    private bool isRecordingSegment = false;
    private int segmentStartSample = -1;
    public float SoundStartTime { get; private set; } = -1f;

    // public float loudnessSensitivity = 100f;
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
        micName = Microphone.devices[0];
        sampleRate = AudioSettings.outputSampleRate;
        SetupMicrophone();
        segmentTimer = 0f;
        lastSegmentEndPosition = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMicrophoneInput();
    }

    public void SetupMicrophone()
    {
        if (Microphone.IsRecording(micName))
        {
            Microphone.End(micName);
        }
        microphoneClip = Microphone.Start(micName, true, rollingBufferLengthSeconds, sampleRate);
        Debug.Log($"Microphone started -- {micName}");
    }

    private void ProcessMicrophoneInput()
    {
        int micPosition = Microphone.GetPosition(micName);
        float loudness = GetLoudnessFromAudioClip(micPosition, microphoneClip);
            // Debug.Log("loudness: " + loudness);

        if (!isRecordingSegment && loudness > minThreshold)
        {
            // Start recording a 2-second segment
            isRecordingSegment = true;
            segmentTimer = 0f;
            segmentStartSample = micPosition;
        }
   
        if (isRecordingSegment)
        {
            segmentTimer += Time.deltaTime;
            if (segmentTimer >= segmentDurationSeconds)
            {
                int segmentEndSample = Microphone.GetPosition(micName);
                ExtractAndSendSpeechSegment(segmentStartSample, segmentEndSample);
                isRecordingSegment = false;
                segmentTimer = 0f;
                segmentStartSample = -1;
            }
        }
    }

    private void ExtractAndSendSpeechSegment(int startSample, int endSample)
    {
        int totalSamples = microphoneClip.samples;
        int channels = microphoneClip.channels;
        int segmentLength;
        if (endSample < startSample)
        {
            segmentLength = (totalSamples - startSample) + endSample;
        }
        else
        {
            segmentLength = endSample - startSample;
        }
        if (segmentLength <= 0) return;

        float[] segmentData = new float[segmentLength * channels];
        if (endSample < startSample)
        {
            float[] temp1 = new float[(totalSamples - startSample) * channels];
            float[] temp2 = new float[endSample * channels];
            microphoneClip.GetData(temp1, startSample);
            microphoneClip.GetData(temp2, 0);
            Array.Copy(temp1, 0, segmentData, 0, temp1.Length);
            Array.Copy(temp2, 0, segmentData, temp1.Length, temp2.Length);
        }
        else
        {
            microphoneClip.GetData(segmentData, startSample);
        }

        AudioClip segmentClip = AudioClip.Create("SpeechSegment", segmentLength, channels, sampleRate, false);
        segmentClip.SetData(segmentData, 0);
        SoundStartTime = Time.time;
        OnStartSpeaking?.Invoke(segmentClip);
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
        return averageLoudness;
    }
}
