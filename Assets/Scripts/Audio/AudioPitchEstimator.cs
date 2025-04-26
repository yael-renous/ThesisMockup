using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Fundamental frequency estimation using Summation of Residual Harmonics (SRH)
// T. Drugman and A. Alwan: "Joint Robust Voicing Detection and Pitch Estimation Based on Residual Harmonics", Interspeech'11, 2011.

public class AudioPitchEstimator : MonoBehaviour
{
    [Tooltip("Lowest frequency that can estimate [Hz]")]
    [Range(40, 150)]
    public int frequencyMin = 40;

    [Tooltip("Highest frequency that can estimate [Hz]")]
    [Range(300, 1200)]
    public int frequencyMax = 600;

    [Tooltip("Number of overtones to use for estimation")]
    [Range(1, 8)]
    public int harmonicsToUse = 5;

    [Tooltip("Frequency bandwidth of spectral smoothing filter [Hz]\nWider bandwidth smoothes the estimation, however the accuracy decreases.")]
    public float smoothingWidth = 500;

    [Tooltip("Threshold to judge silence or not\nLarger the value, stricter the judgment.")]
    public float thresholdSRH = 7;

    const int spectrumSize = 1024;
    const int outputResolution = 200; // frequency axis resolution (decreasing this will reduce the calculation load)
    float[] spectrum = new float[spectrumSize];
    float[] specRaw = new float[spectrumSize];
    float[] specCum = new float[spectrumSize];
    float[] specRes = new float[spectrumSize];
    float[] srh = new float[outputResolution];

    public List<float> SRH => new List<float>(srh);

    /// <summary>
    /// Estimates the fundamental frequency
    /// </summary>
    /// <param name="audioSource">Input audio source</param>
    /// <returns>Fundamental frequency [Hz] (float.NaN if it does not exist)</returns>
    public float Estimate(AudioSource audioSource)
    {
        var nyquistFreq = AudioSettings.outputSampleRate / 2.0f;

        // Get audio spectrum
        if (!audioSource.isPlaying) {
            Debug.LogWarning("Audio source is not playing");
            return float.NaN;
        }
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);

        // Calculate the logarithm of the amplitude spectrum
        // All subsequent spectra are handled in logarithmic amplitude (this differs from the original paper)
        for (int i = 0; i < spectrumSize; i++)
        {
            // Add a small value to avoid -∞ when amplitude is zero
            specRaw[i] = Mathf.Log(spectrum[i] + 1e-9f);
        }

        // Cumulative sum of the spectrum (used later)
        specCum[0] = 0;
        for (int i = 1; i < spectrumSize; i++)
        {
            specCum[i] = specCum[i - 1] + specRaw[i];
        }

        // Calculate the residual spectrum
        var halfRange = Mathf.RoundToInt((smoothingWidth / 2) / nyquistFreq * spectrumSize);
        for (int i = 0; i < spectrumSize; i++)
        {
            // Smooth the spectrum (moving average using cumulative sum)
            var indexUpper = Mathf.Min(i + halfRange, spectrumSize - 1);
            var indexLower = Mathf.Max(i - halfRange + 1, 0);
            var upper = specCum[indexUpper];
            var lower = specCum[indexLower];
            var smoothed = (upper - lower) / (indexUpper - indexLower);

            // Remove the smooth component from the original spectrum
            specRes[i] = specRaw[i] - smoothed;
        }

        // Calculate the SRH (Summation of Residual Harmonics) score
        float bestFreq = 0, bestSRH = 0;
        for (int i = 0; i < outputResolution; i++)
        {
            var currentFreq = (float)i / (outputResolution - 1) * (frequencyMax - frequencyMin) + frequencyMin;

            // Calculate the SRH score at the current frequency: Equation (1) from the paper
            var currentSRH = GetSpectrumAmplitude(specRes, currentFreq, nyquistFreq);
            for (int h = 2; h <= harmonicsToUse; h++)
            {
                // The stronger the signal at h times the frequency, the better
                currentSRH += GetSpectrumAmplitude(specRes, currentFreq * h, nyquistFreq);

                // The stronger the signal at the frequency between h-1 and h times, the worse
                currentSRH -= GetSpectrumAmplitude(specRes, currentFreq * (h - 0.5f), nyquistFreq);
            }
            srh[i] = currentSRH;

            // Record the frequency with the highest score
            if (currentSRH > bestSRH)
            {
                bestFreq = currentFreq;
                bestSRH = currentSRH;
            }
        }

        // If the SRH score does not meet the threshold, consider that there is no clear fundamental frequency
        if (bestSRH < thresholdSRH) {
            Debug.Log("No clear fundamental frequency found, SRH score below threshold.");
            return float.NaN;
        }

        Debug.Log($"Estimated fundamental frequency: {bestFreq} Hz with SRH score: {bestSRH}");
        return bestFreq;
    }

    // Get the amplitude at frequency[Hz] from the spectrum data
    float GetSpectrumAmplitude(float[] spec, float frequency, float nyquistFreq)
    {
        var position = frequency / nyquistFreq * spec.Length;
        var index0 = (int)position;
        var index1 = index0 + 1; // Boundary check of the array is omitted
        var delta = position - index0;
        return (1 - delta) * spec[index0] + delta * spec[index1];
    }
}
