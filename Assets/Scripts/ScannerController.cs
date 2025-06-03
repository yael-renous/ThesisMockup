using UnityEngine;

public class ScannerController : MonoBehaviour
{
    public int audioId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetAudioClipId(int audioId){
        this.audioId = audioId;
    }
}
