using UnityEngine;

public class RoomObject : MonoBehaviour
{
   public AudioSource audioSource;
   public GameObject ColoredObject;
   public string name;

   private float audioStartTime = 0f; // To track when the audio started playing
   public float minPlayTimeInSeconds = 0.5f;
   private AudioLowPassFilter lowPass;

   // Awake is called when the script instance is being loaded
   void Awake()
   {
       // Create a new AudioSource component if needed
       if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();
       }
       audioSource.loop = false;

       // Add or get the AudioLowPassFilter
       lowPass = GetComponent<AudioLowPassFilter>();
       if (lowPass == null)
           lowPass = gameObject.AddComponent<AudioLowPassFilter>();
       // Set default cutoff to max (no muffling)
       lowPass.cutoffFrequency = 22000f;

       ColoredObject.SetActive(false);
   }


   public void showColoredObject(){
    if(ColoredObject != null){
        ColoredObject.SetActive(true);
        Invoke("hideColoredObject", 0.3f);
    }
   }

   public void hideColoredObject(){
    ColoredObject.SetActive(false);
   }



   void OnParticleCollision(GameObject other)
   {
        Debug.Log($"{name}: OnParticleCollision");
        // Only play if enough time has passed since last play
        if (Time.time - audioStartTime >= minPlayTimeInSeconds)
        {
            // Debug.Log("RoomObject: OnParticleCollision");
            // Get the parent GameObject's name instead of the particle system's name
            string parentName = other.transform.name;
            // Debug.Log("RoomObject: Parent name: " + parentName);
            // Get the audio ID from the parent GameObject's name
            if (int.TryParse(parentName, out int id))
            {
                // Debug.Log("RoomObject: Playing audio with ID: " + id);
                play(id);
                audioStartTime = Time.time;
            }
        }
   }

   public void play(int audioId)
   {
    //    float timeSinceDetection = Time.time - AudioDetection.Instance.SoundStartTime;
    //    float volume = CalculateEchoVolume(timeSinceDetection);
    //    float cutoff = CalculateEchoCutoff(timeSinceDetection);

    //    audioSource.volume = Mathf.Clamp01(volume);

    //    // Just set the cutoff, filter is always present
    //    lowPass.cutoffFrequency = cutoff;

    //    Debug.Log($"Playing audio {audioClip.length} seconds at volume {volume}, cutoff {cutoff}");
       AudioClip audioClip = SceneManager.Instance.GetAudioClip(audioId);
       Debug.LogWarning($" {name}: Playing audio with ID: " + audioId);
        audioSource.PlayOneShot(audioClip);
    //    Debug.Log($"{name} playing audio");
   }

   private float CalculateEchoVolume(float timeSinceDetection)
   {
       // Higher = faster fade
       return Mathf.Exp(-timeSinceDetection * SceneManager.Instance.volumeDecayRate);
   }

   private float CalculateEchoCutoff(float timeSinceDetection)
   {
       float minCutoff = 500f;
       float maxCutoff = 22000f;
       // Higher = faster muffling
       return Mathf.Lerp(maxCutoff, minCutoff, Mathf.Clamp01(timeSinceDetection * SceneManager.Instance.cutoffDecayRate));
   }

   private void StopAudio()
   {
       if (audioSource.isPlaying)
       {
           Debug.Log("Audio finished playing");
           // hideColoredObject();
           audioSource.Stop();
           audioSource.clip = null;
       }
   }
}
