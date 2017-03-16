using UnityEngine;
using System.Collections;

public class PlatformPathController : MonoBehaviour
{
    public float platformSpeed;
    public bool cyclingEnabled;
    public float waitTime;
    [Range(0, 2)]
    public float easeAmount;
    public Vector3[] localWaypoints;
    float nextMoveTime;
    Vector3[] globalWaypoints;
    int fromWaypointIndex;
    float percentBetweenWaypoints;

    void Start()
    {
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    float CalculateEasing(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    public Vector3 CalcPlatformVelocity()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float dstBetweenWaypoint = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += platformSpeed * Time.deltaTime / 2;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easingBetweenWaypoint = CalculateEasing(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easingBetweenWaypoint);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;
            if (!cyclingEnabled)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }
        return newPos - transform.position;
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.yellow;
            float size = 0.3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypoinPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypoinPos - Vector3.up * size, globalWaypoinPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypoinPos - Vector3.right * size, globalWaypoinPos + Vector3.right * size);
                Gizmos.DrawLine(globalWaypoinPos - Vector3.forward * size, globalWaypoinPos + Vector3.forward * size);

            }
        }
    }
}
