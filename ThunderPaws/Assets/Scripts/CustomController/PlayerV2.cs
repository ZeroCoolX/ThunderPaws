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
        //Stop the accumulation of gravity if we're move moving up or down
        if(controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) {//jump is pressed and the player is standing on something TODO: double jump
            velocity.y = jumpVelocity;
        }

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborn);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

}
