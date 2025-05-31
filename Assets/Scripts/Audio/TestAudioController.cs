using UnityEngine;
using System.Collections;

public class TestAudioController : MonoBehaviour
{
    public AudioSource leftSpeakerObject;
    public AudioSource rightSpeakerObject;

    private AudioSource micOutput;
    private AudioClip micClip;
    private bool isMicPlaying = false;

    void Start()
    {
        string micName = Microphone.devices[0]; // pick first available
        micClip = Microphone.Start(micName, true, 10, 44100);
        Debug.Log("Mic name: " + micName);
        // Create AudioSource to play mic output later
        // micOutput = gameObject.AddComponent<AudioSource>();
        micOutput = leftSpeakerObject;
        micOutput.clip = micClip;
        micOutput.loop = true;
        micOutput.playOnAwake = false;
        StartCoroutine(WaitForMicAndPlay());

        // Set up test sounds for left and right speakers (optional)
        if (leftSpeakerObject != null)
        {
            leftSpeakerObject.spatialBlend = 0; // 2D sound
            leftSpeakerObject.panStereo = -1.0f; // full left
            // leftSpeakerObject.Play();
        }

        if (rightSpeakerObject != null)
        {
            rightSpeakerObject.spatialBlend = 0;
            rightSpeakerObject.panStereo = 1.0f; // full right
            // rightSpeakerObject.Play();
        }
    }

    System.Collections.IEnumerator WaitForMicAndPlay()
    {
        // Wait until the microphone has started recording
        while (!(Microphone.GetPosition(null) > 0)) {
            yield return null;
        }
        micOutput.timeSamples = Microphone.GetPosition(null);
        micOutput.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && !isMicPlaying)
        {
            Debug.Log("x down");
        // int micPosition = Microphone.GetPosition(null);
        // if (micPosition > 0)
        // {
        //     micOutput.timeSamples = micPosition;
        // }

            Debug.Log("Playing mic output");
            micOutput.Play();
            isMicPlaying = true;
            Invoke("StopMic", 5f); // Stop after 5 seconds
        }
    }

    void StopMic()
    {
        Debug.Log("Stopping mic output");
        micOutput.Stop();
        isMicPlaying = false;
    }
}