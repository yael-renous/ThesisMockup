using UnityEngine;

public class FireflyEffect : BackgroundEffect
{

    // public float duration = 10f;

    public override void activate()
    {
       
    }

    public override void deactivate()
    {
    }

    public override float getDuration()
    {
        return Random.Range(0.8f, 1.4f);
    }
}
