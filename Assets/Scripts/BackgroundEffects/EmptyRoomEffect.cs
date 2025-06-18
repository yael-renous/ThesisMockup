using UnityEngine;

public class EmptyRoomEffect : BackgroundEffect
{
    public GameObject roomCubeParent;
    public float duration = 10f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void activate()
    {
        roomCubeParent.SetActive(true);
    }

    public override void deactivate()
    {}

    public override float getDuration()
    {
        return duration;
    }
}
