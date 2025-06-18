using UnityEngine;

public class FirefliesRoomEffect : BackgroundEffect
{
    public GameObject fireflies;
    public AudioSource audioSource;
    public AudioClip audioClip;
    public float duration = 5f;

    private bool isActive = false;
    private float panChangeTimer = 0f;
    private float nextPanChangeTime = 0.5f; // initial interval
    private int panDirection = 1; // 1 for ++, -1 for --

    public override float getDuration()
    {
        return duration;
    }

    public override void activate()
    {
        fireflies.SetActive(true);
        audioSource.loop = true;
        audioSource.clip = audioClip;
        audioSource.Play();
        isActive = true;
        panChangeTimer = 0f;
        nextPanChangeTime = Random.Range(0.2f, 1.0f);
        panDirection = Random.value > 0.5f ? 1 : -1; // Random initial direction
    }

    public override void deactivate()
    {    
        fireflies.SetActive(false);
        isActive = false;
    }

    void Update()
    {
        if (!isActive) return;

        panChangeTimer += Time.deltaTime;
        if (panChangeTimer >= nextPanChangeTime)
        {
            // At each interval, randomly choose new direction
            panDirection = Random.value > 0.5f ? 1 : -1;

            // Reset timer and choose a new random interval
            panChangeTimer = 0f;
            nextPanChangeTime = Random.Range(0.2f, 1.0f);
        }

        // Always step in the current direction
        float panStep = 0.1f * panDirection;
        audioSource.panStereo = Mathf.Clamp(audioSource.panStereo + panStep * Time.deltaTime * 10f, -1f, 1f);
    }
}
