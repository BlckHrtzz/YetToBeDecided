using UnityEngine;
using System.Collections;

public class PlayerController : RaycastController
{
    public CollisionInfo collisions;
    public LayerMask collisionMask;
    public float maxClimbAngle = 80.0f;
    public float maxDescendAngel = 80.0f;
    Vector2 playerAxisInput;
   


    public override void Start()
    {
        base.Start();
        collisions.faceDirection = 1;
    }
    public void Move(Vector3 velocity, bool isStanding)
    {
        Move(velocity, Vector2.zero, isStanding);
    }
    public void Move(Vector3 velocity, Vector2 axisInput, bool isStanding = false)
    {
        UpdateRayorigins();
        collisions.ResetCollision();
        playerAxisInput = axisInput;

        if (velocity.x != 0)
        {
            collisions.faceDirection = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y < 0)
        {
            DescendingSlopes(ref velocity);
        }

        HorizontalCollision(ref velocity);
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        if (isStanding == true)
        {
            collisions.bottom = true;
        }

        transform.Translate(velocity);

    }

    void HorizontalCollision(ref Vector3 velocity)
    {
        float directionX = collisions.faceDirection;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        if (Mathf.Abs(velocity.x) < 0)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < maxHorizontalRays; i++)
        {
            Vector3 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector3.up * (horizontalRaySpacing * i);
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, Vector3.right * directionX, out hit, rayLength, collisionMask))
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                collisions.colliderTag = hit.collider.tag;                                                      //For getting the tag of Collider...

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    float dstToSlopeStart = 0;
                    if (slopeAngle != collisions.oldSlopeAngle)
                    {
                        dstToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= dstToSlopeStart * directionX;
                    }

                    ClimbingSlopes(ref velocity, slopeAngle);
                    velocity.x += dstToSlopeStart * directionX;
                }
                else
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;
                    if (collisions.isClimbingSlope)
                    {
                        velocity.y = Mathf.Abs(velocity.x) * Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad);
                    }

                    collisions.right = directionX == 1;
                    collisions.left = directionX == -1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < maxVerticalRays; i++)
        {
            Vector3 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector3.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask))
            {
                if (hit.collider.tag == "Through")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (collisions.isFallingThrough)
                    {
                        continue;
                    }
                    if (playerAxisInput.y == -1 && Input.GetButtonDown("Jump"))
                    {
                        collisions.isFallingThrough = true;
                        Invoke("ResetFall", 0.5f);
                        continue;
                    }


                }
                Debug.DrawRay(rayOrigin, Vector3.up * directionY, Color.red);
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
                if (collisions.isClimbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.top = directionY == 1;
                collisions.bottom = directionY == -1;
            }
        }

        if (collisions.isClimbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;

            Vector3 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector3.up * velocity.y;
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask))
            {

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }

    }

    void ClimbingSlopes(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float slopClimbVelocityY = moveDistance * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
        if (velocity.y <= slopClimbVelocityY)
        {
            velocity.y = slopClimbVelocityY;
            velocity.x = moveDistance * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);

            collisions.bottom = true;
            collisions.isClimbingSlope = true;
            collisions.slopeAngle = slopeAngle;

        }
    }

    void DescendingSlopes(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector3 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, collisionMask))
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngel)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = moveDistance * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
                        velocity.x = moveDistance * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.bottom = true;
                        collisions.isDescendingSlope = true;
                        collisions.slopeAngle = slopeAngle;
                    }
                }
            }
        }
    }

    void ResetFall()
    {
        collisions.isFallingThrough = false;
    }

    public struct CollisionInfo
    {
        public bool top, bottom;
        public bool left, right;
        public int faceDirection;
        public bool isClimbingSlope, isDescendingSlope;
        public float slopeAngle, oldSlopeAngle;
        public string colliderTag;
        public bool isFallingThrough;
        public void ResetCollision()
        {
            top = bottom = false;
            left = right = false;

            isClimbingSlope = isDescendingSlope = false;
            oldSlopeAngle = slopeAngle;
            slopeAngle = 0;
            colliderTag = "";
        }


    }
}
