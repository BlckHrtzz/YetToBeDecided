using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]

public class PlayerDash : MonoBehaviour
{
    Vector3 dashVelocity;
    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashTime = 0.2f;
    float dashAcceleration;
    float dashVelocityX;
    public float dashCoolDownTime = 0.5f;
    float smoothFactor;

    [Header("Vertical Movements")]
    public bool gravityEnabled = false;
    public float dashVelocityY = 0.4f;

    [Header("Air Dash Settings")]
    public bool airDashEnabled = true;
    public bool airDashLimit = true;
    public float airDashTotal = 1;

    Player player;
    PlayerController playerController;

    [HideInInspector]
    public bool isDashing = false;
    [HideInInspector]
    public float oldVelocityX;
    bool dashAllowed = false;
    bool dash = false;
    float dashTimer;
    float dashCoolDownTimer;
    bool runDashCoolDownTimer = false;
    float airDashRemaining;



    void Start()
    {
        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();
        dashAcceleration = 2 * dashDistance / Mathf.Pow(dashTime, 2);                   //Calculation for Dash Acceleration.
        dashVelocity.x = Mathf.Sqrt(2 * dashAcceleration * dashDistance);                    //Calculation for dash velocity.
    }


    void Update()
    {
        if (playerController.collisions.bottom)
        {
            ResetAirDash();
        }

        SetDashAllowed();

        if (isDashing)
        {
            dashAllowed = false;
            if (player.velocity.x == 0)
            {
                dashTimer = 0;
            }

            if (dashTimer > 0)
            {
                dashTimer -= Time.deltaTime;
            }
            else
            {
                runDashCoolDownTimer = true;
                isDashing = false;

                //player.velocity.x = oldVelocityX;
                Debug.Log("This is old" + oldVelocityX);
                Debug.Log(player.velocity);
            }
        }

        if (runDashCoolDownTimer)
        {
            dashAllowed = false;
            if (dashCoolDownTimer > 0)
            {
                dashCoolDownTimer -= Time.deltaTime;
            }
            else
            {
                runDashCoolDownTimer = false;
                SetDashAllowed();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !isDashing && dashAllowed)
        {
            dash = true;
        }
    }

    void FixedUpdate()
    {
        if (dash)
        {
            dash = false;

            isDashing = true;
            playerController.Move(dashVelocity * playerController.collisions.faceDirection * Time.deltaTime, player.playerAxisInput);
            Debug.Log(player.velocity);

            dashTimer = dashTime;
            dashCoolDownTimer = dashCoolDownTime;

            if (airDashLimit && airDashRemaining > 0)
            {
                airDashRemaining--;
            }
        }

        if (isDashing)
        {
            // ... make sure the Y velocity is set if the gravity is disabled for the dash.
            if (gravityEnabled)
            {
                // Set the speed.

                player.velocity.y = dashVelocityY;
            }
        }

    }

    void SetDashAllowed()
    {
        if (!playerController.collisions.bottom && !airDashEnabled)
        {
            dashAllowed = false;
        }
        else if (!playerController.collisions.bottom && airDashLimit)
        {
            if (airDashRemaining > 0)
            {
                dashAllowed = true;
            }
            else
            {
                dashAllowed = false;
            }
        }
        else
        {
            dashAllowed = true;
        }
    }

    void ResetAirDash()
    {
        airDashRemaining = airDashTotal;
    }
}
