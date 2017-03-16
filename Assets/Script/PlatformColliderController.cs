using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlatformPathController))]
public class PlatformColliderController : RaycastController
{
    public Vector3 move;
    public LayerMask passengerMask;
    PlatformPathController pathController;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, PlayerController> passengerDictionary = new Dictionary<Transform, PlayerController>();

    public override void Start()
    {
        base.Start();
        pathController = GetComponent<PlatformPathController>();
    }

    void Update()
    {
        UpdateRayorigins();

        Vector3 velocity = pathController.CalcPlatformVelocity();

        CalcMovingPassenger(velocity);

        MovePassenger(true);
        transform.Translate(velocity);
        MovePassenger(false);
    }

    void MovePassenger(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<PlayerController>());
            }
            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    void CalcMovingPassenger(Vector3 velocity)
    {
        HashSet<Transform> movedPassenger = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);



        // Vertically Moving Platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;
            for (int i = 0; i < maxVerticalRays; i++)
            {
                Vector3 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector3.right * (verticalRaySpacing * i);
                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.up * directionY, Color.cyan);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, passengerMask))
                {
                    if (hit.distance != 0)
                    {
                        if (!movedPassenger.Contains(hit.transform))
                        {
                            movedPassenger.Add(hit.transform);
                            float pushX = (directionY == 1) ? velocity.x : 0;
                            float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                            passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                        }
                    }
                }
            }
        }

        //Horizontal Moving Platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;
            for (int i = 0; i < maxHorizontalRays; i++)
            {
                Vector3 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector3.up * horizontalRaySpacing * i;
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, Vector3.right * directionX, out hit, rayLength, passengerMask))
                {
                    if (hit.distance != 0)
                    {
                        if (!movedPassenger.Contains(hit.transform))
                        {
                            movedPassenger.Add(hit.transform);
                            float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                            float pushY = -skinWidth;
                            passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                        }
                    }
                }
            }
        }

        //if passenger is on top 
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;
            for (int i = 0; i < maxVerticalRays; i++)
            {
                Vector3 rayOrigin = raycastOrigins.topLeft + (Vector3.right * verticalRaySpacing * i);
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, Vector3.up, out hit, rayLength, passengerMask))
                {
                    if (hit.distance != 0)
                    {
                        if (!movedPassenger.Contains(hit.transform))
                        {
                            movedPassenger.Add(hit.transform);
                            float pushX = velocity.x;
                            float pushY = velocity.y;
                            passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                        }
                    }
                }
            }
        }
    }

    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }
}
