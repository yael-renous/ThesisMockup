using UnityEngine;
using DG.Tweening;

public class RoomObject : MonoBehaviour
{
   public AudioSource audioSource;
   public GameObject ColoredObject;
   public GameObject RealColoredObject;
   public Material[] ColoredObjectMaterials;
   public string name;
   public GameObject spotlight;
   private float lightIntensity = 10f; // Added public parameter for light intensity
   private float audioStartTime = 0f; // To track when the audio started playing
   public float minPlayTimeInSeconds = 0.5f;
 

   // Awake is called when the script instance is being loaded
   void Awake()
   {
       // Create a new AudioSource component if needed
       if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();
       }
       audioSource.loop = false;

     

       ColoredObject.SetActive(false);
       spotlight.SetActive(false);
   }


   public void showColoredObject(Color color){
    if(ColoredObject != null){
        foreach(Material material in ColoredObjectMaterials){
            material.SetColor("_BaseColor", color);
        }
        RealColoredObject.SetActive(false);
        ColoredObject.SetActive(true);
        Invoke("hideColoredObject", 0.3f);
    }
   }


   public void hideColoredObject(){
    RealColoredObject.SetActive(true);
    ColoredObject.SetActive(false);
   }


   public void showSpotlightandPlay(int audioId){
    // RealColoredObject.SetActive(true);
    spotlight.SetActive(true);
    // Fade in the spotlight
    spotlight.GetComponent<Light>().DOIntensity(lightIntensity, 0.3f).From(0f);
    play(audioId);
    // Fade out the spotlight
    spotlight.GetComponent<Light>().DOIntensity(0f, 0.3f).SetDelay(0.3f)
        .OnComplete(() => spotlight.SetActive(false));
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
       AudioClip audioClip = SceneManager.Instance.GetAudioClip(audioId);
        audioSource.PlayOneShot(audioClip);
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
