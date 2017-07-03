using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PlayerV2 : MonoBehaviour {

    //Determines what gravity and jumpVelocity are set to
    public float jumpHeight = 4f;//how high
    public float timeToJumpApex = 0.4f;//how long till they reach highest point
    float accelerationTimeAirborn = 0.2f;//change direction a little slower when in the air
    float accelerationTimeGrounded = 0.1f;
    float moveSpeed = 6f;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = 0.25f;
    float timeToWallUnstick;

    float gravity;
    float jumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

	void Start () {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print("Gravity: " + gravity + " and JumpVelocity: " + jumpVelocity);
	}

    private void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left) ? -1 : 1;

        //must calculate x first before dealing with wall sliding
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborn);

        bool wallSliding = false;
        //check if we're sliding down the wall
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
            wallSliding = true;

            if(velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }

            if(timeToWallUnstick > 0) {
                //reset velocity so the player can stick to the wall
                velocityXSmoothing = 0;
                velocity.x = 0;

                //enables the user to be pressing the direction key they want to go before jumping - makes wall jumping way easier
                if (input.x != wallDirX && input.x != 0) {
                    timeToWallUnstick -= Time.deltaTime;
                } else {
                    timeToWallUnstick = wallStickTime;
                }
            }else {
                timeToWallUnstick = wallStickTime;
            }
        }

        //Stop the accumulation of gravity if we're move moving up or down
        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {//jump is pressed and the player is standing on something TODO: double jump
            if (wallSliding) {
                if(wallDirX == input.x) {//tryiung to move in the same direction we're facing
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }else if(input.x == 0) {//just hopping off the wall
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpClimb.y;
                }else {//leap off in the opposite direction
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            //normal jump
            if (controller.collisions.below) {
                velocity.y = jumpVelocity;
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

}
