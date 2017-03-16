using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class RaycastController : MonoBehaviour
{
    public BoxCollider coll;

    public RaycastOrigins raycastOrigins;
    [HideInInspector]
    public const float skinWidth = 0.015f;
    [HideInInspector]
    public float maxVerticalRays;
    [HideInInspector]
    public float maxHorizontalRays;
    [HideInInspector]
    public float dstBetweenRays = 0.25f;

    [HideInInspector]
    public float verticalRaySpacing;
    [HideInInspector]
    public float horizontalRaySpacing;


    public virtual void Awake()
    {
        coll = GetComponent<BoxCollider>();

    }
    public virtual void Start()
    {
        CalcRaySpacing();
    }

    public void UpdateRayorigins()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);
        float midZ = bounds.center.z;

        raycastOrigins.topLeft = new Vector3(bounds.min.x, bounds.max.y, midZ);
        raycastOrigins.topRight = new Vector3(bounds.max.x, bounds.max.y, midZ);
        raycastOrigins.bottomLeft = new Vector3(bounds.min.x, bounds.min.y, midZ);
        raycastOrigins.bottomRight = new Vector3(bounds.max.x, bounds.min.y, midZ);
    }

    void CalcRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);
        float midZ = bounds.center.z;

        float boundsHeight = bounds.size.y;
        float boundsWidth = bounds.size.x;

        maxHorizontalRays = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        maxVerticalRays = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

        verticalRaySpacing = boundsWidth / (maxVerticalRays - 1);
        horizontalRaySpacing = boundsHeight / (maxHorizontalRays - 1);
    }

    public struct RaycastOrigins
    {
        public Vector3 bottomRight, bottomLeft;
        public Vector3 topRight, topLeft;
    }
}
