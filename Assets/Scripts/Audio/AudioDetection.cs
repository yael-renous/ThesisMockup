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

    public float lowPitchThreshold = 200f;
    public float mediumPitchThreshold = 600f;

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

        if (!isRecordingSegment && loudness > minThreshold)
        {
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
        
        if (startSample < 0 || startSample >= totalSamples || endSample < 0 || endSample >= totalSamples)
        {
            return;
        }

        int segmentLength;
        if (endSample < startSample)
        {
            segmentLength = (totalSamples - startSample) + endSample;
        }
        else
        {
            segmentLength = endSample - startSample;
        }

        if (segmentLength <= 0 || segmentLength > totalSamples)
        {
            return;
        }

        float[] segmentData = new float[segmentLength * channels];
        
        try
        {
            if (endSample < startSample)
            {
                float[] temp1 = new float[(totalSamples - startSample) * channels];
                float[] temp2 = new float[endSample * channels];
                
                bool success1 = microphoneClip.GetData(temp1, startSample);
                bool success2 = microphoneClip.GetData(temp2, 0);
                
                if (!success1 || !success2)
                {
                    return;
                }
                
                Array.Copy(temp1, 0, segmentData, 0, temp1.Length);
                Array.Copy(temp2, 0, segmentData, temp1.Length, temp2.Length);
            }
            else
            {
                bool success = microphoneClip.GetData(segmentData, startSample);
                
                if (!success)
                {
                    Debug.LogError($"Failed to get audio data for normal segment. startSample={startSample}");
                    return;
                }
            }

            AudioClip segmentClip = AudioClip.Create("SpeechSegment", segmentLength, channels, sampleRate, false);
            segmentClip.SetData(segmentData, 0);
            string pitch = ClassifyPitch(segmentData, sampleRate);
            Debug.LogWarning("Pitch: " + pitch);
            SoundStartTime = Time.time;
            OnStartSpeaking?.Invoke(segmentClip);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing audio segment: {e.Message}\nStack trace: {e.StackTrace}");
        }
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

    private string ClassifyPitch(float[] audioData, int sampleRate)
    {
        float frequency = EstimatePitchSRH(audioData, sampleRate);
        
        if (float.IsNaN(frequency))
        {
            Debug.LogWarning("No clear pitch detected");
            return "Unknown";
        }
        
        Debug.LogWarning("Detected Frequency: " + frequency + " Hz");
        
        // Classify the pitch
        if (frequency < lowPitchThreshold) return "Low";
        else if (frequency < mediumPitchThreshold) return "Medium";
        else return "High";
    }

    private float EstimatePitchSRH(float[] audioData, int sampleRate)
    {
        int frequencyMin = 80;
        int frequencyMax = 800;
        int harmonicsToUse = 5;
        float smoothingWidth = 500;
        float thresholdSRH = 3.0f;
        int outputResolution = 200;
        int spectrumSize = 1024;
        
        if (audioData.Length < spectrumSize)
        {
            Debug.LogWarning("Audio data too short for pitch estimation");
            return float.NaN;
        }

        // Create FFT spectrum from audio data
        float[] spectrum = new float[spectrumSize];
        float[] fftData = new float[spectrumSize * 2]; // Real and imaginary parts
        
        // Copy audio data and apply windowing
        for (int i = 0; i < spectrumSize; i++)
        {
            if (i < audioData.Length)
            {
                // Apply Hanning window
                float window = 0.5f * (1.0f - Mathf.Cos(2.0f * Mathf.PI * i / (spectrumSize - 1)));
                fftData[i * 2] = audioData[i] * window; // Real part
                fftData[i * 2 + 1] = 0; // Imaginary part
            }
            else
            {
                fftData[i * 2] = 0;
                fftData[i * 2 + 1] = 0;
            }
        }

        // Simple FFT implementation for magnitude spectrum
        SimpleFFT(fftData, spectrumSize);
        
        // Calculate magnitude spectrum
        for (int i = 0; i < spectrumSize; i++)
        {
            float real = fftData[i * 2];
            float imag = fftData[i * 2 + 1];
            spectrum[i] = Mathf.Sqrt(real * real + imag * imag);
        }

        var nyquistFreq = sampleRate / 2.0f;
        float[] specRaw = new float[spectrumSize];
        float[] specCum = new float[spectrumSize];
        float[] specRes = new float[spectrumSize];

        // Calculate the logarithm of the amplitude spectrum
        for (int i = 0; i < spectrumSize; i++)
        {
            specRaw[i] = Mathf.Log(spectrum[i] + 1e-9f);
        }

        // Cumulative sum of the spectrum
        specCum[0] = 0;
        for (int i = 1; i < spectrumSize; i++)
        {
            specCum[i] = specCum[i - 1] + specRaw[i];
        }

        // Calculate the residual spectrum
        var halfRange = Mathf.RoundToInt((smoothingWidth / 2) / nyquistFreq * spectrumSize);
        for (int i = 0; i < spectrumSize; i++)
        {
            var indexUpper = Mathf.Min(i + halfRange, spectrumSize - 1);
            var indexLower = Mathf.Max(i - halfRange + 1, 0);
            var upper = specCum[indexUpper];
            var lower = specCum[indexLower];
            var smoothed = (upper - lower) / (indexUpper - indexLower);
            specRes[i] = specRaw[i] - smoothed;
        }

        // Calculate the SRH score
        float bestFreq = 0, bestSRH = 0;
        for (int i = 0; i < outputResolution; i++)
        {
            var currentFreq = (float)i / (outputResolution - 1) * (frequencyMax - frequencyMin) + frequencyMin;
            var currentSRH = GetSpectrumAmplitude(specRes, currentFreq, nyquistFreq, spectrumSize);
            
            for (int h = 2; h <= harmonicsToUse; h++)
            {
                currentSRH += GetSpectrumAmplitude(specRes, currentFreq * h, nyquistFreq, spectrumSize);
                currentSRH -= GetSpectrumAmplitude(specRes, currentFreq * (h - 0.5f), nyquistFreq, spectrumSize);
            }

            if (currentSRH > bestSRH)
            {
                bestFreq = currentFreq;
                bestSRH = currentSRH;
            }
        }

        if (bestSRH < thresholdSRH)
        {
            return float.NaN;
        }

        return bestFreq;
    }

    private float GetSpectrumAmplitude(float[] spec, float frequency, float nyquistFreq, int spectrumSize)
    {
        var position = frequency / nyquistFreq * spectrumSize;
        var index0 = (int)position;
        var index1 = index0 + 1;
        
        if (index0 >= spec.Length - 1 || index0 < 0) return 0;
        
        var delta = position - index0;
        return (1 - delta) * spec[index0] + delta * spec[index1];
    }

    private void SimpleFFT(float[] data, int n)
    {
        // Bit-reverse copy
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1)
                j ^= bit;
            j ^= bit;

            if (i < j)
            {
                // Swap real parts
                float temp = data[i * 2];
                data[i * 2] = data[j * 2];
                data[j * 2] = temp;
                
                // Swap imaginary parts
                temp = data[i * 2 + 1];
                data[i * 2 + 1] = data[j * 2 + 1];
                data[j * 2 + 1] = temp;
            }
        }

        // FFT
        for (int len = 2; len <= n; len <<= 1)
        {
            float wlen_r = Mathf.Cos(2 * Mathf.PI / len);
            float wlen_i = -Mathf.Sin(2 * Mathf.PI / len);
            
            for (int i = 0; i < n; i += len)
            {
                float w_r = 1, w_i = 0;
                for (int j = 0; j < len / 2; j++)
                {
                    float u_r = data[(i + j) * 2];
                    float u_i = data[(i + j) * 2 + 1];
                    float v_r = data[(i + j + len / 2) * 2] * w_r - data[(i + j + len / 2) * 2 + 1] * w_i;
                    float v_i = data[(i + j + len / 2) * 2] * w_i + data[(i + j + len / 2) * 2 + 1] * w_r;
                    
                    data[(i + j) * 2] = u_r + v_r;
                    data[(i + j) * 2 + 1] = u_i + v_i;
                    data[(i + j + len / 2) * 2] = u_r - v_r;
                    data[(i + j + len / 2) * 2 + 1] = u_i - v_i;
                    
                    float new_w_r = w_r * wlen_r - w_i * wlen_i;
                    w_i = w_r * wlen_i + w_i * wlen_r;
                    w_r = new_w_r;
                }
            }
        }
    }
}
