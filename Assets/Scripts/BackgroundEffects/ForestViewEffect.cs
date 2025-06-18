using UnityEngine;
using DG.Tweening;

public class ForestViewEffect : BackgroundEffect
{
    public GameObject ForestView;
    public Camera[] cameras;
    public Transform camerasParentTransform;    

    public Vector3 leftBound;
    public Vector3 rightBound;

    public float moveSpeed = 1f;
    public float clipPlaneSpeed = 1f;

    // public Transform cameraTransform;

    private bool isActive = false;
    public GameObject roomCubeParent;

    private float[] originalFarClipPlanes;
        private LayerMask[] originalLayerMasks;
        private Vector3 originalCameraParentPosition;
        private Quaternion originalCameraParentRotation;
  

    public float duration = 5f;
    public float transitionDuration = 1f; // Duration for the DOTween animations

    private bool isMoving = false;
    private bool isRotating = false;
    private int movementAxis = 0;
    private int rotationAxis = 0;
    private float movementSpeed = 2f;
    private float rotationSpeed = 30f;
    private bool isMovingForward = true;
    private bool isRotatingForward = true;

    void Start()
    {
        originalCameraParentPosition = camerasParentTransform.position;
        originalCameraParentRotation = camerasParentTransform.rotation;
        originalFarClipPlanes = new float[cameras.Length];
        originalLayerMasks = new LayerMask[cameras.Length];
        for (int i = 0; i < cameras.Length; i++)
        {
            originalFarClipPlanes[i] = cameras[i].farClipPlane;
            originalLayerMasks[i] = cameras[i].cullingMask;
        }
    }
    void Update()
    {
        if (isActive)
        {
            if (isMoving)
            {
                Vector3 currentPos = camerasParentTransform.position;
                float step = movementSpeed * Time.deltaTime;
                
                switch (movementAxis)
                {
                    case 0: // X axis
                        if (isMovingForward)
                        {
                            currentPos.x += step;
                            if (currentPos.x >= rightBound.x) isMovingForward = false;
                        }
                        else
                        {
                            currentPos.x -= step;
                            if (currentPos.x <= leftBound.x) isMovingForward = true;
                        }
                        break;
                    case 1: // Y axis
                        if (isMovingForward)
                        {
                            currentPos.y += step;
                            if (currentPos.y >= rightBound.y) isMovingForward = false;
                        }
                        else
                        {
                            currentPos.y -= step;
                            if (currentPos.y <= leftBound.y) isMovingForward = true;
                        }
                        break;
                    case 2: // Z axis
                        if (isMovingForward)
                        {
                            currentPos.z += step;
                            if (currentPos.z >= rightBound.z) isMovingForward = false;
                        }
                        else
                        {
                            currentPos.z -= step;
                            if (currentPos.z <= leftBound.z) isMovingForward = true;
                        }
                        break;
                }
                camerasParentTransform.position = currentPos;
            }

            if (isRotating)
            {
                Vector3 currentRotation = camerasParentTransform.rotation.eulerAngles;
                float step = rotationSpeed * Time.deltaTime;
                
                switch (rotationAxis)
                {
                    case 0: // X axis
                        currentRotation.x += step;
                        break;
                    case 1: // Y axis
                        currentRotation.y += step;
                        break;
                    case 2: // Z axis
                        currentRotation.z += step;
                        break;
                }
                camerasParentTransform.rotation = Quaternion.Euler(currentRotation);
            }
        }
    }
    public override void activate()
    {
        //turn off room cube parent
        roomCubeParent.SetActive(false);
        //turn on forest view
        ForestView.SetActive(true);
        //move camera to forest view
        isActive = true;
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].farClipPlane = 1000;
        }

        // Randomly choose between movement or rotation
        bool shouldMove = Random.value > 0.5f;
        bool shouldRotate = Random.value > 0.5f;
        
        if (shouldMove)
        {
            isMoving = true;
            movementAxis = Random.Range(0, 3);
            isMovingForward = Random.value > 0.5f;
            camerasParentTransform.position = originalCameraParentPosition;
        }
        
        if (shouldRotate)
        {
            isRotating = true;
            rotationAxis = Random.Range(0, 3);
            camerasParentTransform.rotation = originalCameraParentRotation;
        }
    }

    public override void deactivate()
    {
        //reset camera parent transform
        camerasParentTransform.position = originalCameraParentPosition;
        camerasParentTransform.rotation = originalCameraParentRotation;
        //turn off forest view
        ForestView.SetActive(false);
        //turn on room cube parent
        roomCubeParent.SetActive(true);
        //turn off isActive
        isActive = false;
        isMoving = false;
        isRotating = false;
        //reset camera far clip planes
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].farClipPlane = originalFarClipPlanes[i];
        }
    }

   public override float getDuration()
   {
    return duration;
   }
   
}
