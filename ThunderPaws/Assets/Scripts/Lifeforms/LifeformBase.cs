﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LifeformBase : MonoBehaviour {
    /// <summary>
    /// How fast the object moves
    /// </summary>
    protected float MoveSpeed;
    /// <summary>
    /// How high can object jump
    /// </summary>
    protected float JumpHeight;
    /// <summary>
    /// How long it takes to reach JumpHeight
    /// </summary>
    protected float TimeToJumpApex;
    /// <summary>
    /// Used for dampening movenentS
    /// </summary>
    protected float AccelerationTimeAirborne;
    /// <summary>
    /// Used for dampening movement
    /// </summary>
    protected float AccelerationTimeGrounded;

    /// <summary>
    /// Calculated based off jump constraints
    /// </summary>
    protected float Gravity;
    /// <summary>
    /// Calculated based off gravity and jump constraints
    /// </summary>
    protected float JumpVelocity;
    /// <summary>
    /// Object movement
    /// </summary>
    protected Vector3 Velocity;
    /// <summary>
    /// Just used as a reference for the Mathf.SmoothDamp function
    /// </summary>
    protected float VelocityXSmoothing;

    /// <summary>
    /// collision detection controller
    /// </summary>
    protected CollisionController2D Controller;

    protected List<PickupableEnum> Pickups;

    /// <summary>
    /// Set all constant physics values
    /// Calculate dynamic values like Gravity and JumpVelocity
    /// </summary>
    /// <param name="moveSpeed"></param>
    /// <param name="jumpHeight"></param>
    /// <param name="timeToJumpApex"></param>
    /// <param name="accelerationTimeAirborne"></param>
    /// <param name="accelerationTimeGrounded"></param>
    protected void InitializePhysicsValues(float moveSpeed, float jumpHeight, float timeToJumpApex, float accelerationTimeAirborne, float accelerationTimeGrounded, float gravity = 0) {
        MoveSpeed = moveSpeed;
        JumpHeight = jumpHeight;
        TimeToJumpApex = timeToJumpApex;
        AccelerationTimeAirborne = accelerationTimeAirborne;
        AccelerationTimeGrounded = accelerationTimeGrounded;
        //Phsyics controller used for all collision detection
        Controller = GetComponent<CollisionController2D>();
        //Calculate gravity and jump velocity
        if (gravity == 0) {
            Gravity = -(2 * JumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
        }else {
            Gravity = gravity;
        }
        JumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;
        print("Gravity: " + Gravity + "\n Jump Velocity: " + JumpVelocity);
    }

    /// <summary>
    /// Add the gravity constant to .y component of velocity
    /// Do not accumulate gravity if colliding with anything vertically
    /// </summary>
    protected abstract void ApplyGravity();

    /// <summary>
    /// Apply whatever pickup was found
    /// </summary>
    public abstract void ApplyPickup(PickupableEnum pickupType);

    /// <summary>
    /// Add whatever pickup we collided with to the list
    /// </summary>
    /// <param name="pickup"></param>
    protected void AddPickupable(PickupableEnum pickup) {
        Pickups.Add(pickup);
    }

}
