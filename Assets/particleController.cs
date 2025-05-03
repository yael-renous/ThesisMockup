using UnityEngine;

public class particleController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     void OnParticleCollision(GameObject other)
   {
       Debug.Log("Particle HIT");
   }
}
