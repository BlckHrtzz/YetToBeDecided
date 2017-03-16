using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]

public class PlayerTeleport : RaycastController
{
    Player player;
    PlayerController playerController;
    public float teleportDistance = 5.0f;
    Vector3 teleportPoint;
    float faceDirection;                                            //The direction that the player is facing.


    public override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();                            //Reference for Player Class.
        playerController = GetComponent<PlayerController>();        //Reference for PlayerController Class.
    }

    public override void Start()
    {
        base.Start();                                               //Calling the Start() function from RaycastController class.

    }

    void Update()
    {
        faceDirection = playerController.collisions.faceDirection;
        if (Input.GetKeyDown(KeyCode.T))
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.right * playerController.collisions.faceDirection, out hit, teleportDistance);

            if (hit.collider != null)
            {
                teleportPoint = new Vector3(hit.point.x - 0.5f * faceDirection, hit.point.y, hit.point.z);
            }
            else
            {
                teleportPoint = transform.position + Vector3.right * playerController.collisions.faceDirection * teleportDistance;
            }
            transform.position = teleportPoint;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * faceDirection * teleportDistance);
    }
}
