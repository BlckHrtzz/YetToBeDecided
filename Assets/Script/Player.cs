using UnityEngine;
using System.Collections;
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerDash))]
public class Player : MonoBehaviour
{
    PlayerController playerController;
    PlayerDash playerDash;
    [HideInInspector]
    public Vector3 velocity;
    [Header("Y Movement Factors")]
    [Tooltip("Maximum height to which player can jump")]
    public float maxJumpHeight = 5.0f;
    [Tooltip("Minimum height to which player can jump")]
    public float minJumpHeight = 1.0f;
    [Tooltip("Time Taken to reach the Jump Peak")]
    public float timeToJumpPeak = 0.1f;
    [Tooltip("Number of jumps allowed")]
    public int maxJumpAllowed = 2;
    float maxJumpVelocity;                                              //The maximum velocity of jump; Will be Calculated in the start function.
    float minJumpVelocity;                                              //The minimum velocity of jump; Will be Calculated in the start function.
    float gravity;                                                      //Gravity applied to player; Will be Calculated in the start function. 
    int jumpCount;                                                      //To keep the check of the jumps made.

    [Header("X Movement Factors")]
    [Tooltip("Set the speed of the Player")]
    public float moveSpeed = 10.0f;
    [Tooltip("Set the acceleration of the Player while on air")]
    public float airAcceleration = 0.2f;
    [Tooltip("Set the acceleration of the Player while on ground")]
    public float groundAcceleration = 0.1f;
    float xSmoothingFactor;                                                 //just for reference for smoothDamp function. Will Be calculated by Unity...
    [HideInInspector]
    public Vector2 playerAxisInput;

    [Header("Wall Jump Factors")]
    [Tooltip("Set True if player is allowed to do WALL JUMPS")]
    public bool wallJumpEnabled;
    [Tooltip("Set True if player is allowed to SLIDE on walls")]
    public bool slidingEnabled;
    [Tooltip("Set True if player is allowed to STICK on walls or else Player will leave the wall after wallStickTime(i.e. Next Option)")]
    public bool wallStickEnabled;
    [Tooltip("Set for how long player should hold the wall if upper option is disabled")]
    public float wallStickTime = 0.5f;
    [Tooltip("Set the speed at which the player should slide on the walls")]
    public float wallSlidingSpeed = 3f;
    [Space]
    [Tooltip("Set True if player is allowed to do jump on the same walls")]
    public bool wallClimbEnabled;
    public Vector3 wallClimb;
    [Tooltip("Set True if player is allowed to do short jump to leave the walls")]
    public bool wallHopEnabled;
    public Vector3 wallHop;
    [Tooltip("Set True if player is allowed to jump from one wall to another wall")]
    public bool wallLeapEnabled;
    public Vector3 wallLeap;

    float timeToWallUnstick;

    bool isFacingRight = true;
    Vector3 faceDirection;
    Transform playerModel;


    void Start()
    {
        playerModel = this.gameObject.transform.GetChild(0);
        playerController = GetComponent<PlayerController>();
        playerDash = GetComponent<PlayerDash>();
        gravity = -(2 * maxJumpHeight / Mathf.Pow(timeToJumpPeak, 2));
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpPeak;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }


    void Update()
    {
        playerAxisInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Flip(playerAxisInput.x);
        float targetVelocity = playerAxisInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref xSmoothingFactor, (playerController.collisions.bottom) ? groundAcceleration : airAcceleration);

        int wallDirection = (playerController.collisions.left) ? -1 : 1;

        bool isWallSliding = false;

        if ((playerController.collisions.left || playerController.collisions.right) && !playerController.collisions.bottom && velocity.y < 0 && wallJumpEnabled)
        {
            isWallSliding = true;
            if (velocity.y < -wallSlidingSpeed)
            {
                velocity.y = -wallSlidingSpeed;
            }

            if (timeToWallUnstick > 0)
            {
                velocity.x = 0;
                xSmoothingFactor = 0;

                if (playerAxisInput.x != wallDirection && playerAxisInput.x != 0 && !wallStickEnabled)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }

        if (Input.GetButtonDown("Jump") && playerAxisInput.y != -1)
        {
            if (isWallSliding)
            {
                if (wallDirection == playerAxisInput.x && wallClimbEnabled)
                {
                    velocity.x = -wallDirection * wallClimb.x;
                    velocity.y = wallClimb.y;
                }
                else if (playerAxisInput.x == 0 && wallHopEnabled)
                {
                    velocity.x = -wallDirection * wallHop.x;
                    velocity.y = wallHop.y;
                }
                else if (wallLeapEnabled && wallDirection != playerAxisInput.x && playerAxisInput.x != 0)
                {
                    velocity.x = -wallDirection * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            if (playerController.collisions.bottom)
            {
                velocity.y = maxJumpVelocity;
                jumpCount++;
            }
            else if (!playerController.collisions.bottom && !isWallSliding && jumpCount < maxJumpAllowed && Input.GetButtonDown("Jump"))
            {
                velocity.y = maxJumpVelocity - 1f;
                jumpCount++;
            }
        }
        else if (playerController.collisions.bottom)
        {
            jumpCount = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space) && playerAxisInput.y != -1)
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }

        if (!playerDash.isDashing)
        {
            velocity.y += gravity * Time.deltaTime;
            playerController.Move(velocity * Time.deltaTime, playerAxisInput);
        }

        if (playerController.collisions.bottom || playerController.collisions.top)
        {
            velocity.y = 0;
        }
    }

    void Flip(float playerInput)
    {

        if ((playerInput > 0 && !isFacingRight) || (playerInput < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            faceDirection = playerModel.transform.localScale;
            faceDirection.x *= -1;
            playerModel.transform.localScale = faceDirection;
        }
        //transform.localScale = faceDirection;
    }

    /* void ToGoThreeDimension()
     {
         if (Input.GetKeyDown(KeyCode.R))                           //Required Shit to go Three Dimension:).....
         {
             transform.Rotate(0, -90, 0, Space.World);
             playerController.UpdateRayorigins();
             print("Rotated");
         }
     }*/

}
