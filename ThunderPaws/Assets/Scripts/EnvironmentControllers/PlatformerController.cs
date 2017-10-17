using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerController : RaycastController_veryOld {//TODO: add /// documentation
    public LayerMask passengerMask;

    public Vector3[] localWaypoints;
    public Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    //the platforms slow and speed up as they reach the waypoints for a more natural feel
    [Range(0,2)]
    public float easeAmount;

    //global index of the waypoint we're moving away from
    int fromWaypointIndex;
    float percentBetweenWaypoints; //0-1
    float nextMoveTime;

    //Stores all objects being effected by the platform
    List<PassengerMovement> passengerMovement;
    //Efficient way of storing the passenger component references without many calls per update
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start() {
        base.Start();

        //Assign waypoints for platforms
        globalWaypoints = new Vector3[localWaypoints.Length];
        for(int i = 0; i < localWaypoints.Length; ++i) {
            //waypoints the platform actually moves between
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    // Update is called once per frame
    void Update () {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);

        //Move only passengers that should be before the platform
        MovePassengers(true);
        //Move the platform
        transform.Translate(velocity);
        //Move only passengers that should be after the platform
        MovePassengers(false);
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement() {
        if(Time.time < nextMoveTime) {
            return Vector3.zero;
        }

        //makes the variable fromWaypointsIndex reset to 0 everytime it reaches globalWaypoints.Length.
        fromWaypointIndex %= globalWaypoints.Length;

        //need to know which waypoint we're coming from, which we're going to, and the percentage between the two
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        //distance between the two
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;//this is so the percentage increases more slowly the further away waypoints are
        Mathf.Clamp01(percentBetweenWaypoints);//because if it's outside of 0 or 1 we get strange results for our ease function
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);//find point from fromWaypoint to toWaypoint based off our percentage
        //check if we reached the waypoint
        if (percentBetweenWaypoints >= 1) {
            percentBetweenWaypoints = 0;
            ++fromWaypointIndex;
            if (!cyclic) {
                if (fromWaypointIndex >= globalWaypoints.Length - 1) {//ran out of waypoints
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }
        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach(PassengerMovement passenger in passengerMovement) {
            //Using the dictionary so that only one "GetCOmponent" call occurs for any passenger instead of once per update - which would be horrendous for performance
            if (!passengerDictionary.ContainsKey(passenger.transform)) {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if (passenger.moveBeforePlatform == beforeMovePlatform) {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.input, passenger.standingOnPlatform);
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
                if (hit && hit.distance != 0) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);//ensures each passenger is only moved 1 time per frame
                        //let the gap between the passenger and platform close - then move the rest of the velocity
                        float pushX = (directionY == 1) ? velocity.x : 0;//in the horizontal direction only move the passenger if they're standing on it
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;//distance between plat and passenger
                        //add a new passengermovement
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true, hit.transform.GetComponent<PlayerController2D>().directionalInput));
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
                if (hit && hit.distance != 0) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);//ensures each passenger is only moved 1 time per frame
                        //let the gap between the passenger and platform close - then move the rest of the velocity
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;//in the horizontal direction only move the passenger if they're standing on it
                        float pushY = -skinWidth;
                        //add a new passengermovement
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true, hit.transform.GetComponent<PlayerController2D>().directionalInput));
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
                if (hit && hit.distance != 0) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);//ensures each passenger is only moved 1 time per frame
                        //let the gap between the passenger and platform close - then move the rest of the velocity
                        float pushX = velocity.x;
                        float pushY = velocity.y;
                        //add a new passengermovement
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false, hit.transform.GetComponent<PlayerController2D>().directionalInput));
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
        public Vector2 input;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform, Vector2 _input) {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
            input = _input;
        }
    }

    private void OnDrawGizmos() {
        if(localWaypoints != null) {
            Gizmos.color = Color.red;
            float size = 0.3f;

            for(int i = 0; i < localWaypoints.Length; ++i) {
                //convert local position into global position
                Vector3 globalWaypointPos = Application.isPlaying ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                //Draw centered at above a little cross
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

}
