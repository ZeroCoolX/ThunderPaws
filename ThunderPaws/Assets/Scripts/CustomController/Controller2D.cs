using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    public LayerMask collisionMask;

    //used so when the character is resting on the ground its not floating or 100% just touching it. it looks more natural
    const float skinWidth = 0.015f;

    //max angle we can run up or down without sliding
    float maxClimbAngle = 80f;
    float maxDescendAngle = 75f;

    //how many rays are being fired horizontally and vertically
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    //calculate the spacing between rays based on how many we fire and size of bounds
    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    private void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 velocity) {
        //handle collisions
        UpdateRaycastOrigins();
        //Blank slate each time
        collisions.Reset();
        collisions.velocityOld = velocity;

        if(velocity.y < 0) {
            DescendSlope(ref velocity);
        }
        if(velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }
        //Move the object
        transform.Translate(velocity);
    }


    //Pass in a reference to the actual parameter variable so any change inside the method changes the passed in variable
    void HorizontalCollisions(ref Vector3 velocity) {
        //get direction of velocity
        float directionX = Mathf.Sign(velocity.x);
        //positive value of velocity + skinWidth to get out of the collider
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; ++i) {
                                                            //moving left                 moving right
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i); 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {
                //get the angle of the surface we hit
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(i == 0 && slopeAngle <= maxClimbAngle) {//bottom most ray
                    if (collisions.descendingSlope) {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    if(slopeAngle != collisions.slopeAngleOld) {//started climbing a new angle
                        distanceToSlopeStart = hit.distance - skinWidth;
                        //subtract so when we call ClimbSlop, it only uses velocity x it has when it reaches the slope
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    //reset it
                    velocity.x += distanceToSlopeStart * directionX;
                }

                //only check other rays if not climbing slope or slope angle > maxclimb angle
                if(!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                    //set y velocity to the distance between where we fired, and where the raycast intersected with an obstacle
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    //also update velocity on y axis
                    if (collisions.climbingSlope) {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //if we hit something going left or right, store the info
                    collisions.left = directionX < 0;
                    collisions.right = directionX > 0;
                }
            }
        }
    }

    //Pass in a reference to the actual parameter variable so any change inside the method changes the passed in variable
    void VerticalCollisions(ref Vector3 velocity) {
        //get direction of velocity
        float directionY = Mathf.Sign(velocity.y);
        //positive value of velocity + skinWidth to get out of the collider
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; ++i) {  
                                                    //moving down                 moving up
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x); //include .x because we want to do it from where we will be once we've moved
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                //set y velocity to the distance between where we fired, and where the raycast intersected with an obstacle
                velocity.y = (hit.distance - skinWidth) * directionY;
                //we change the ray length so that if there is a higher ledge on the left, but not the right, we dont pass through the higher point.
                rayLength = hit.distance;

                //also update velocity x
                if (collisions.climbingSlope) {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                //if we hit something going up or down, store the info
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        //handle slope changes within a current slope
        if (collisions.climbingSlope) {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle) {//this means we'vce collided with a new slope
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    //Speed when climbing slope same as normal. Treat velocity.x as the total distance  up the slope we want to move
    //then that distance and slope angle = velocity x and y
    void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sign(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        //check if we're jumping on the slope
        if (velocity.y <= climbVelocityY) {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x); //but maintain direction right or left
            //assume we're standing on the ground if climbing slope
            collisions.below = true;
            //store the slope info
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    //Same as climbing just inverse
    void DescendSlope(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        //cast a ray down and if we're moving left, bottom right, otherwise bottom right
        Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            //if we have a flat surface, dont care
            if(slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
                if(Mathf.Sign(hit.normal.x) == directionX) {//check if moving down the slope
                    //distance to slope is wihtin the distance we need to move to get ot it, so the slope should take effect
                    if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sign(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x); //but maintain direction right or left
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    //Find the corners of our collider
    void UpdateRaycastOrigins() {
        //get bounds                                  
        Bounds bounds = getBounds();

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    //Calculate spacing between raycasts
    void CalculateRaySpacing() {
        //get bounds                                  
        Bounds bounds = getBounds();
        //at least 2 in the horizontal and vertical directions
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
        //if vertical count is 2, then space between them is size of bounds / 1 = entire space. if count is 3 then bounds / 2 = half space in between...etc
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

    }

    /* Helper to get the bounds of the collider                                     
        min.x, max.y | max.x, max.y                                          
        min.x, min.y | max.x, min.y
    */
    private Bounds getBounds() {
        Bounds bounds = collider.bounds;
        //shrink the bounds by skin width
        bounds.Expand(skinWidth * -2);
        return bounds;
    }


    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;

        public void Reset() {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0f;
        }
    }

    //Stores all the corners of our box collider
    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
