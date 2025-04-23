using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RaycastEcho : MonoBehaviour
{
    public int maxBounces = 200;
    public float maxDistance = 1000f;
    public Transform[] raycastOrigins; // Array of origin sources
    private Keyboard keyboard;
    public float lineWidth = 0.1f;
    public float animationSpeed = 1f; // Speed of the animation
    public float returnThreshold = 1f; // Define a threshold for returning to the origin
    public float offsetX = 0f;
    public float offsetY = 0f;
    public float offsetZ = 0f;
    public Color[] lineColors; // Array of colors for the lines
    public Material lineMaterial; // Add this line
    public float lineLifetime = 5f; // Duration in seconds before the line disappears

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard.enterKey.wasPressedThisFrame)
        {
            for (int i = 0; i < raycastOrigins.Length; i++)
            {
                StartCoroutine(AnimateSingleRay(raycastOrigins[i], i));
            }
        }
    }

    IEnumerator AnimateSingleRay(Transform originTransform, int index)
    {
        Debug.Log("CastLaser");

        // Create a new GameObject for the line renderer
        GameObject lineObject = new GameObject("LineRenderer");
        lineObject.transform.parent = this.transform;

        // Add a LineRenderer component
        LineRenderer lr = lineObject.AddComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth * 0.5f;

        // Assign a unique color based on the index
        Color uniqueColor = lineColors[index % lineColors.Length];
        lr.material = lineMaterial; // Use the public material
        lr.material.SetColor("_EmissionColor", uniqueColor);

        Vector3 initialOrigin = originTransform.position;
        Vector3 origin = initialOrigin + new Vector3(offsetX, offsetY, offsetZ);
        Vector3 direction = originTransform.forward;
        float totalDistance = 0f;

        lr.positionCount = 1;
        lr.SetPosition(0, origin);

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
            {
                Vector3 hitPoint = hit.point;
                float segmentDistance = Vector3.Distance(origin, hitPoint);
                float segmentTime = segmentDistance / 343f; // Use speed of sound for timing

                // Animate the segment
                yield return StartCoroutine(AnimateSegment(origin, hitPoint, segmentTime * animationSpeed, lr));

                totalDistance += segmentDistance;
                direction = Vector3.Reflect(direction, hit.normal);
                origin = hitPoint;

                // Check if the ray has returned to the initial origin
                if (Vector3.Distance(origin, initialOrigin) < returnThreshold)
                {
                    Debug.Log("Ray has returned to the origin.");
                    break;
                }
            }
            else
            {
                Vector3 endPoint = origin; // Set endPoint to origin for audio
                float segmentDistance = maxDistance;
                float segmentTime = segmentDistance / 343f; // Use speed of sound for timing

                // Animate the final segment
                yield return StartCoroutine(AnimateSegment(origin, endPoint, segmentTime * animationSpeed, lr));

                totalDistance += segmentDistance;
                break;
            }
        }

        StartCoroutine(LogEchoReturn(totalDistance, lineObject));
    }

    IEnumerator AnimateSegment(Vector3 start, Vector3 end, float duration, LineRenderer lr)
    {
        Debug.Log("duration: " + duration);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector3 currentPoint = Vector3.Lerp(start, end, t);

            lr.positionCount += 1;
            lr.SetPosition(lr.positionCount - 1, currentPoint);

            yield return null;
        }
    }

    private IEnumerator LogEchoReturn(float distance, GameObject lineObject)
    {
        // Calculate the delay based on the distance (e.g., speed of sound is ~343 m/s)
        float delay = distance / 343f;
        yield return new WaitForSeconds(delay);
    

        // Log the echo return
        Debug.Log($"Echo returned after traveling {distance} meters.");
        StartCoroutine(DestroyLineAfterTime(lineObject, lineLifetime));
    }

    // Coroutine to destroy the line after a specified time
    IEnumerator DestroyLineAfterTime(GameObject lineObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(lineObject);
    }
}
