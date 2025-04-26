using UnityEngine;

public abstract class RoomEffect : MonoBehaviour
{
    public AudioClip debugAudioClip;
    public abstract void Activate(AudioClip audioClip);
    
}
