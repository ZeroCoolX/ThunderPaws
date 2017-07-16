using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PlayerV2 : MonoBehaviour {

    //Determines what gravity and jumpVelocity are set to
    public float maxJumpHeight = 4f;//how high
    public float minJumpHeight = 1f;
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
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    void Start() {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + " and JumpVelocity: " + maxJumpVelocity);
    }

    private void Update() {
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        //Stop the accumulation of gravity if we're move moving up or down
        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }
    }

    public void SetDirectionalInput(Vector2 input) {
        directionalInput = input;
    }

    public void OnJumpInputDown() {
        if (wallSliding) {
            if (wallDirX == directionalInput.x) {//tryiung to move in the same direction we're facing
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            } else if (directionalInput.x == 0) {//just hopping off the wall
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpClimb.y;
            } else {//leap off in the opposite direction
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        //normal jump
        if (controller.collisions.below) {
            velocity.y = maxJumpVelocity;
        }
    }

    public void OnJumpInputUp() {
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    private void HandleWallSliding() {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        //check if we're sliding down the wall
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0) {
                //reset velocity so the player can stick to the wall
                velocityXSmoothing = 0;
                velocity.x = 0;

                //enables the user to be pressing the direction key they want to go before jumping - makes wall jumping way easier
                if (directionalInput.x != wallDirX && directionalInput.x != 0) {
                    timeToWallUnstick -= Time.deltaTime;
                } else {
                    timeToWallUnstick = wallStickTime;
                }
            } else {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    private void CalculateVelocity() {
        //must calculate x first before dealing with wall sliding
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborn);
        velocity.y += gravity * Time.deltaTime;
    }

}