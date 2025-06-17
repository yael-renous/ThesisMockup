using UnityEngine;
using DG.Tweening;

public class CameraClipping : MonoBehaviour
{
    public float tweenDuration = 2f; // Duration of the tween in seconds
    public Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (cam != null)
        {
            cam.farClipPlane = 0f;
            DOTween.To(() => cam.farClipPlane, x => cam.farClipPlane = x, 500f, tweenDuration).SetEase(Ease.InQuint);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
