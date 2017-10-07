using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionFollow : MonoBehaviour {
    /// <summary>
    /// Companion reference
    /// </summary>
    public CompanionBase Companion;

    /// <summary>
    /// Who the companion follows
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Buffer for position dampeneing so movment is not sudden and jerky
    /// </summary>
    private float _dampening = 1f;
    /// <summary>
    /// How far to look ahead from our current position
    /// </summary>
    public float lookAheadFactor = 3;
    /// <summary>
    /// How fast we get to the desired position
    /// </summary>
    public float lookAheadReturnSpeed = 0.5f;
    /// <summary>
    /// Determines if we should be looking for the target or wheather we're in a close enough range
    /// </summary>
    public float lookAheadMoveThreshold = 0.1f;
    //Threshold of camera movement down
    public float yPosClamp = -1;

    private float _offsetZ;
    private Vector3 _lastTargetPosition;
    private Vector3 _currentVelocity;
    private Vector3 _lookAheadPos;

    private float nextTimeToSearch = 0f;
    private float searchDelay = 0.5f;

    void Start () {
        _lastTargetPosition = Target.position;
        //this is probably not necessary
        _offsetZ = (transform.position - Target.position).z;
        transform.parent = null;
    }
	
	void Update () {
		//Dead player check
        if(Target == null) {
            FindPlayer();
            return;
        }

        //Only update lookahead position if accelerating or changing direction
        float xMoveDelta = (Target.position - _lastTargetPosition).x;
        print("xdelta = " + xMoveDelta);
        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;
        if (updateLookAheadTarget) {
            _lookAheadPos = lookAheadFactor * Vector2.right * Mathf.Sign(xMoveDelta);
        }else {
            _lookAheadPos = Vector3.MoveTowards(_lookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = Target.position + _lookAheadPos + Vector3.forward * _offsetZ;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref _currentVelocity, _dampening);

        //If we're not moving - idle
        if(xMoveDelta == 0) {
            Companion.Idle = true;
        }else {
            Companion.Idle = false;
            transform.position = newPos;
            _lastTargetPosition = Target.position;
        }
	}

    private void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
            if (searchResult != null) {
                Target = searchResult.transform;
                nextTimeToSearch = Time.time + searchDelay;
            }
        }
    }
}
