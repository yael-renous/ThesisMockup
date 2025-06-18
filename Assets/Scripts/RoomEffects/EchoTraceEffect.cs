using UnityEngine;
using System.Collections;

public class EchoTraceEffect : RoomEffect
{
    #region Inspector Parameters

    [Header("Ray Settings")]
     public int numLasers = 10;
     public int maxBounces = 5;
     public float maxDistance = 500;

    [Header("Line Settings")]
     public float lineWidth = 0.05f;
     public float animationSpeed = 60f;
     public float lineLifetime = 0.04f;
     public bool showFullLine = false;
    public Color[] lineColors;
    public Material lineMaterial;

    [Header("Offset Settings")]
    public float offsetX = 0f;
    public float offsetY = 0f;
    public float offsetZ = 0f;

    public bool showObjectsHit = true;

  
    // public Material colorObjectMaterial;

    #endregion
  
    private float randomSpeed = 0f;
    public override void Activate(int audioId)
    {
        Color uniqueColor = lineColors[audioId % lineColors.Length];
        randomSpeed = Random.Range(0.5f, 1.5f)*animationSpeed;

        AnimateRaysInSphere(SceneManager.Instance.micTransform, audioId, uniqueColor);
    }

    void AnimateRaysInSphere(Transform originTransform, int audioId, Color uniqueColor)
    {
        float angleIncrement = 2 * Mathf.PI / numLasers;

        for (int i = 0; i < numLasers; i++)
        {
            float angle = i * angleIncrement;
            Vector3 direction = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            );
            StartCoroutine(AnimateSingleRay(originTransform, i, direction, audioId, uniqueColor));
        }
    }

    IEnumerator AnimateSingleRay(Transform originTransform, int index, Vector3 direction, int audioId, Color uniqueColor)
    {
        GameObject lineObject = new GameObject("LineRenderer");
        lineObject.transform.parent = this.transform;
        lineObject.layer = LayerMask.NameToLayer("projection");

        LineRenderer lr = lineObject.AddComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth * 0.5f;

        lr.material = lineMaterial;
        lr.material.SetColor("_EmissionColor", uniqueColor);

        Vector3 origin = originTransform.position + new Vector3(offsetX, offsetY, offsetZ);
        origin = new Vector3(origin.x, 0.1f, origin.z);
        float totalDistance = 0f;

        lr.positionCount = 1;
        lr.SetPosition(0, origin);

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
            {
                RoomObject roomObject = hit.collider.GetComponent<RoomObject>();
                Vector3 hitPoint = hit.point;
                float segmentDistance = Vector3.Distance(origin, hitPoint);
                float segmentTime = segmentDistance / 343f;

                yield return StartCoroutine(AnimateSegment(origin, hitPoint, segmentTime * randomSpeed, lr));
                if (roomObject != null)
                {
                    if(showObjectsHit){
                    roomObject.showColoredObject(uniqueColor);
                    }
                    roomObject.play(audioId);
                }

                totalDistance += segmentDistance;
                direction = Vector3.Reflect(direction, hit.normal);
                direction.y = 0;
                direction.Normalize();
                origin = hitPoint;
            }
            else
            {
                float segmentDistance = maxDistance;
                float segmentTime = segmentDistance / 343f;
                yield return StartCoroutine(AnimateSegment(origin, origin, segmentTime * randomSpeed, lr));
                totalDistance += segmentDistance;
                break;
            }
        }

        StartCoroutine(LogEchoReturn(totalDistance, lineObject));
    }

    IEnumerator AnimateSegment(Vector3 start, Vector3 end, float duration, LineRenderer lr)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector3 currentPoint = Vector3.Lerp(start, end, t);

            if (showFullLine)
            {
                lr.positionCount += 1;
                lr.SetPosition(lr.positionCount - 1, currentPoint);
            }
            else
            {
                lr.positionCount = 2;
                lr.SetPosition(0, start);
                lr.SetPosition(1, currentPoint);
            }

            yield return null;
        }
    }

    private IEnumerator LogEchoReturn(float distance, GameObject lineObject)
    {
        float delay = distance / 343f;
        yield return new WaitForSeconds(delay);
        StartCoroutine(DestroyLineAfterTime(lineObject, lineLifetime));
    }

    IEnumerator DestroyLineAfterTime(GameObject lineObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(lineObject);
    }
}
