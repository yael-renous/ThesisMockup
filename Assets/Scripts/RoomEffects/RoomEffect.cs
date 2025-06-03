using UnityEngine;

public abstract class RoomEffect : MonoBehaviour
{
    public AudioClip debugAudioClip;
    public abstract void Activate(int audioId);
    public virtual void Activate(AudioClip audioClip){
        Debug.LogError("RoomEffect: Activate(AudioClip) is not implemented");
    }
    
}
