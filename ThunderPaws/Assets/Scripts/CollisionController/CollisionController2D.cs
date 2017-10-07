using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics controller that can be given to any object with a box collider and it can make use of collision detection
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CollisionController2D : MonoBehaviour {
    /// <summary>
    /// LayerMask to determine which objects we want THIS to collide with
    /// </summary>
    public LayerMask CollisionMask;

    /// <summary>
    /// how much to shrink the box collider bounds by
    /// </summary>
    const float SkinWidth = 0.015f;

    /// <summary>
    /// number of rays used horizontally on each side
    /// </summary>
    public int HorizontalRayCount = 4;
    /// <summary>
    /// number of rays used vertically on each side
    /// </summary>
    public int VerticalRayCount = 4;
    /// <summary>
    /// space in between each horizontal ray
    /// </summary>
    private float HorizontalRaySpacing;
    /// <summary>
    /// space in between each vertical ray
    /// </summary>
    private float VerticalRaySpacing;

    //box collider on the object
    BoxCollider2D BoxCollider;
    //struct containing 4 corner raycast data
    RaycastOrigins RayOrigins;
    //struct containing collision info
    public CollisionInfo Collisions;


	void Start () {
        //box collider on the object
        BoxCollider = GetComponent<BoxCollider2D>();
        //Only calculate on changing of the values
        CalculateRaySpacing();
    }

    void Update() {

    }

    /// <summary>
    /// Update raycast origins to where we're moving to.
    /// Reset collisions.
    /// Calculate both vertical and horizontal collisions.
    /// Move object
    /// </summary>
    /// <param name="velocity"></param>
    public void Move(Vector3 velocity) {
        UpdateRaycasyOrigins();
        Collisions.Reset();

        if(velocity.x != 0) {
            CalculateHorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            CalculateVerticalCollisions(ref velocity);
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
        //get direction of y velocity + up  - down
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
    /// get the bounds of the box collider, shrink by skinwidth, and update raycast origin coordinates
    /// </summary>
    public void UpdateRaycasyOrigins() {
        Bounds bounds = BoxCollider.bounds;
        //shrink in the bounds by -2 on all sides
        bounds.Expand(SkinWidth * -2);

        RayOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        RayOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        RayOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        RayOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing() {
        Bounds bounds = BoxCollider.bounds;
        //shrink in the bounds by -2 on all sides
        bounds.Expand(SkinWidth * -2);

        //Ensure we have at least 2 rays firing in the horizontal and vertical directions
        HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
        VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);

        //(size of face rays come out of) / (1 less than count desired)
        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);

    }

    /// <summary>
    /// Stores information about our raycasts at the 4 corners of our box collider
    /// </summary>
    struct RaycastOrigins {
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }

    /// <summary>
    /// Stores information about the collision - where it occurred..etc
    /// </summary>
    public struct CollisionInfo {
        public bool FromAbove, FromBelow;
        public bool FromLeft, FromRight;

        public void Reset() {
            FromAbove = FromBelow = false;
            FromLeft = FromRight = false;
        }
    }
	
}
