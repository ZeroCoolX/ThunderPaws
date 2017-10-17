using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class RaycastController_veryOld : MonoBehaviour {

    public LayerMask collisionMask;

    //used so when the character is resting on the ground its not floating or 100% just touching it. it looks more natural
    public const float skinWidth = 0.015f;
    public const float dstBetweenHorizontalRays = 0.1f;
    public const float dstBetweenVerticalRays = 0.05f;

    //how many rays are being fired horizontally and vertically
    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public int verticalRayCount;

    //calculate the spacing between rays based on how many we fire and size of bounds
    [HideInInspector]
    public float horizontalRaySpacing = 4;
    [HideInInspector]
    public float verticalRaySpacing = 4;

    [HideInInspector]
    public BoxCollider2D collider;
    public RaycastOrigins raycastOrigins;

    public virtual void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    //Find the corners of our collider
    public void UpdateRaycastOrigins() {
        //get bounds                                  
        Bounds bounds = getBounds();

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    //Calculate spacing between raycasts
    public void CalculateRaySpacing() {
        //get bounds                                  
        Bounds bounds = getBounds();

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        //calculate how many we need based off spacing
        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenHorizontalRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenVerticalRays);

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

    //Stores all the corners of our box collider
    public struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
