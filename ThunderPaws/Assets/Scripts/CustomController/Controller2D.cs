using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    //used so when the character is resting on the ground its not floating or 100% just touching it. it looks more natural
    const float skinWidth = 0.015f;

    //how many rays are being fired horizontally and vertically
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    //calculate the spacing between rays based on how many we fire and size of bounds
    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;

    private void Start() {
        collider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        UpdateRaycastOrigins();
        CalculateRaySpacing();

        for(int i = 0; i < verticalRayCount; ++i) {
            Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);
        }
    }

    //   get the bounds of the collider                                     
    //                        min.x, max.y | max.x, max.y                                          
    //                        min.x, min.y | max.x, min.y
    private Bounds getBounds() {
        Bounds bounds = collider.bounds;
        //shrink the bounds by skin width
        bounds.Expand(skinWidth * -2);
        return bounds;
    } 

    void UpdateRaycastOrigins() {
        //get bounds                                  
        Bounds bounds = getBounds();

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

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


    //Stores all the corners of our box collider
    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
