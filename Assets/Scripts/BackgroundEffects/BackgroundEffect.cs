using UnityEngine;

public abstract class BackgroundEffect : MonoBehaviour
{

   public abstract void activate();
   public abstract void deactivate();
   public abstract float getDuration();
}
