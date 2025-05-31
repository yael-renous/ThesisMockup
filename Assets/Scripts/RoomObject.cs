using UnityEngine;

public class RoomObject : MonoBehaviour
{
   public AudioSource audioSource;
   private AudioClip audioClipToPlay = null;
   public GameObject ColoredObject;

   private float audioStartTime = 0f; // To track when the audio started playing
   public float minPlayTimeInSeconds = 0.5f;
   // Awake is called when the script instance is being loaded
   void Awake()
   {
       // Create a new AudioSource component
       if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();
       }
            audioSource.loop = false;
        
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
        audioClipToPlay = SceneManager.Instance.debugAudioClip;
 
        play(audioClipToPlay);
       Debug.Log("Particle collision detected");
   }

   public void play(AudioClip audioClip)
   {
       if (audioSource.isPlaying)
       {
           float elapsedTime = Time.time - audioStartTime;
           if (elapsedTime < minPlayTimeInSeconds)
           {
               Debug.Log("Audio has not played for the minimum required time.");
               return; // Exit the method if the audio hasn't played long enough
           }
           // Stop the current audio if it has played long enough
           audioSource.Stop();
       }

       Debug.Log("Playing audio " + audioClip.length + " seconds");
    //    audioClipToPlay = audioClip;
    //    audioSource.clip = audioClipToPlay;
       audioSource.PlayOneShot(audioClip);
    //    audioStartTime = Time.time; // Record the start time
    //    Invoke("StopAudio", 2.1f);
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
