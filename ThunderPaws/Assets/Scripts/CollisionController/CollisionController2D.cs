using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics controller that can be given to any object with a box collider and it can make use of collision detection
/// </summary>
public class CollisionController2D : RaycastController {
    /// <summary>
    /// LayerMask to determine which objects we want THIS to collide with
    /// </summary>
    public LayerMask CollisionMask;
    /// <summary>
    /// Struct containing collision info
    /// </summary>
    public CollisionInfo Collisions;

    /// <summary>
    /// Player input
    /// </summary>
    public Vector2 PlayerInput;

    public override void Start() {
        base.Start();
    }

    public void Move(Vector3 velocity) {
        Move(velocity, Vector2.zero);
    }

    /// <summary>
    /// Update raycast origins to where we're moving to.
    /// Reset collisions.
    /// Calculate both vertical and horizontal collisions.
    /// Move object
    /// Optional input parameter is for one way platforms. Need a reference to the player input to know if we should drop through platforms
    /// </summary>
    /// <param name="velocity"></param>
    public void Move(Vector3 velocity, Vector2 input) {
        PlayerInput = input;
        UpdateRaycasyOrigins();
        Collisions.Reset();
        if(velocity.x != 0) {
            CalculateHorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            CalculateVerticalCollisions(ref velocity);
        }
        //Only calculaate near ledge if we're standing on something. 
        if (velocity.x != 0 && Collisions.FromBelow) {
            CalculateNearLedge(ref velocity);
        }
        //Move the object
        transform.Translate(velocity);
    }

    /// <summary>
    /// Starting at either bottom left or right depending on which horizontal direction object is moving
    /// Draw ray from origin on the object out to see if it collides with anything
    /// If it does - set ray distance to that for all rays left to cast
    /// Set velocity.x to the distance to the nearest collision
    /// </summary>
    /// <param name="velocity"></param>
    public void CalculateHorizontalCollisions(ref Vector3 velocity) {
        //get direction of x velocity + up  - down
        float directionX = Mathf.Sign(velocity.x);
        //length of ray
        float rayLength = Mathf.Abs(velocity.x) + SkinWidth;

        for (int i = 0; i < HorizontalRayCount; ++i) {
            //check in which direction we're moving
            //down = start bottom left, up = start top left
            Vector2 rayOrigin = (directionX == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight;
            //moves it along the x values - top and bottom faces
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);
            if (hit) {
                //Check if we're within a platform - I.E. jumping up or falling down through one way platforms
                if (hit.distance == 0) {
                    continue;
                }
                //distance from us to the object <= velocity.x so set it to that
                velocity.x = (hit.distance - SkinWidth) * directionX;
                //change ray length once we hit the first thing because we shouldn't cast rays FURTHER than this min one
                rayLength = hit.distance;

                //Set collision info
                Collisions.FromLeft = (directionX == -1);
                Collisions.FromRight = (directionX == 1);
            }
        }
    }

    /// <summary>
    /// Starting at either bottom or top depending on which vertical direction object is moving
    /// Draw ray from origin on the object out to see if it collides with anything
    /// If it does - set ray distance to that for all rays left to cast
    /// Set velocity.y to the distance to the nearest collision
    /// </summary>
    /// <param name="velocity"></param>
    public void CalculateVerticalCollisions(ref Vector3 velocity) {
        //get direction of y velocity + up  - down
        float directionY = Mathf.Sign(velocity.y);
        //length of ray
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; ++i) {
            //check in which direction we're moving
            //down = start bottom left, up = start top left
            Vector2 rayOrigin = (directionY == -1) ? RayOrigins.BottomLeft : RayOrigins.TopLeft;
            //moves it along the x values - top and bottom faces
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);//addition of velocity.x allows the ray to be drawn where we WILL move to - otherwise by the time the ray is drawn we'd move past it
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);
            if (hit) {
                //Check for one way platforms - or completely through ones
                if (hit.collider.tag == "OBSTACLE-THROUGH") {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    }
                }
                ///Do not collide if we're currently falling through the platform
                if (Collisions.FallingThroughPlatform) {
                    continue;
                }
                //Give the player half a second chance to fall through the platform
                if (PlayerInput.y == -1 && hit.collider.tag == "OBSTACLE-THROUGH") {
                    Collisions.FallingThroughPlatform = true;
                    Invoke("ResetFallingThroughPlatform", 0.25f);
                    continue;
                }

                //distance from us to the object <= velocity.y so set it to that
                velocity.y = (hit.distance - SkinWidth) * directionY;
                //change ray length once we hit the first thing because we shouldn't cast rays FURTHER than this min one
                rayLength = hit.distance;

                //Set collision info
                Collisions.FromBelow = (directionY == -1);
                Collisions.FromAbove = (directionY == 1);
            }
        }
    }

    /// <summary>
    /// After a set interval reset the collisions
    /// </summary>
    void ResetFallingThroughPlatform() {
        Collisions.FallingThroughPlatform = false;
    }

    /// <summary>
    /// Specific vertical collision checking used for AI determining if they're near a ledge
    /// </summary>
    /// <param name="velocity"></param>
    public void CalculateNearLedge(ref Vector3 velocity) {
        //get direction of y velocity + up  - down
        float directionY = Mathf.Sign(velocity.y);
        //length of ray
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        //check in which direction we're moving
        Vector2 rayOrigin = (Mathf.Sign(velocity.x) == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight;
        //moves it along the x values - top and bottom faces
        rayOrigin += Vector2.right * velocity.x;//We either need the bottom left or bottom right
        Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, CollisionMask);
        if (!hit) {
            //We are standing on a ledge where the bottom left or bottom right is hanging over the edge
            Collisions.NearLedge = true;
        }
    }


    /// <summary>
    /// Stores information about the collision - where it occurred..etc
    /// </summary>
    public struct CollisionInfo {
        public bool FromAbove, FromBelow;
        public bool FromLeft, FromRight;
        public bool NearLedge;
        public bool FallingThroughPlatform;

        public void Reset() {
            FromAbove = FromBelow = false;
            FromLeft = FromRight = false;
            NearLedge = false;
            FallingThroughPlatform = false;
        }
    }
	
}
