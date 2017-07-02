using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerController : RaycastController {
    public Vector3 move;
    public LayerMask passengerMask;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update () {
        UpdateRaycastOrigins();
        Vector3 velocity = move * Time.deltaTime;

        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach(PassengerMovement passenger in passengerMovement) {
            //Using the dictionary so that only one "GetCOmponent" call occurs for any passenger instead of once per update - which would be horrendous for performance
            if (!passengerDictionary.ContainsKey(passenger.transform)) {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if (passenger.moveBeforePlatform == beforeMovePlatform) {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    //any controller 2d being effected by the patform - above below w/e
    void CalculatePassengerMovement(Vector3 velocity) {
        //Hashset of passengers we've already moved
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //Vertically moving platform
        if(velocity.y != 0) {
            //positive value of velocity + skinWidth to get out of the collider
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; ++i) {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i); //include .x because we want to do it from where we will be once we've moved
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                //found a passenger
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);//ensures each passenger is only moved 1 time per frame
                        //let the gap between the passenger and platform close - then move the rest of the velocity
                        float pushX = (directionY == 1) ? velocity.x : 0;//in the horizontal direction only move the passenger if they're standing on it
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;//distance between plat and passenger
                        //add a new passengermovement
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        //Horizontally moving platforms
        if (velocity.x != 0) {
            //positive value of velocity + skinWidth to get out of the collider
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; ++i) {//no way this matters
                                                          //moving left                 moving right
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                //found a passenger
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);//ensures each passenger is only moved 1 time per frame
                        //let the gap between the passenger and platform close - then move the rest of the velocity
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;//in the horizontal direction only move the passenger if they're standing on it
                        float pushY = -skinWidth;
                        //add a new passengermovement
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        //Passenger on top of horizontally or downward moving platform - cast ray up
        if(directionY == -1 || velocity.y == 0 && velocity.x !=0) {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; ++i) {
                //moving down                 moving up
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i); 
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                //found a passenger
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);//ensures each passenger is only moved 1 time per frame
                        //let the gap between the passenger and platform close - then move the rest of the velocity
                        float pushX = velocity.x;
                        float pushY = velocity.y;
                        //add a new passengermovement
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

}
