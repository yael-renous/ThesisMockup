using UnityEngine;

public class RoomObjectController : MonoBehaviour
{
   private AudioSource audioSource;
   public AudioDetection audioDetection;

   private AudioClip audioClipToPlay=null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // audioDetection.OnStartSpeaking += OnStartSpeaking;
        audioSource.loop = false;
        audioSource.spatialBlend = 1.0f;
        
        
    }

    private void OnDestroy()
    {
        // audioDetection.OnStartSpeaking -= OnStartSpeaking;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void play(AudioClip audioClip)
    {
        Debug.Log("Playing audio");
        audioClipToPlay = audioClip;
        audioSource.clip = audioClipToPlay;
        audioSource.Play();
        Invoke("StopAudio", audioClipToPlay.length);
    }

    private void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            Debug.Log("Audio finished playing");
            audioSource.Stop();
        }
    }
}
