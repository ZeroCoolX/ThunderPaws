using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionFollow : FollowBase {
    /// <summary>
    /// Companion reference
    /// </summary>
    public CompanionBase Companion;

    void Start () {
        InitializeSearchName("Player");
        base.Start();
    }
	
	void Update () {
		//Dead player check
        if(Target == null) {
            FindPlayer();
            return;
        }
        //Only update lookahead position if accelerating or changing direction
        float xMoveDelta = (Target.position - LastTargetPosition).x;
        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadMoveThreshold;
        if (updateLookAheadTarget) {
            LookAheadPos = LookAheadFactor * Vector2.right * Mathf.Sign(xMoveDelta);
        } else {
            LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, Dampening);

        //If we're not moving - idle
        if (xMoveDelta == 0) {
            Companion.Idle = true;
        } else {
            Companion.Idle = false;
            transform.position = newPos;
            LastTargetPosition = Target.position;
        }

        
    }
}
