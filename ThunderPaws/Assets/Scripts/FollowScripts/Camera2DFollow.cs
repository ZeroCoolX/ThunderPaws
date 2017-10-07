using System;
using UnityEngine;

namespace UnityStandardAssets._2D {
    public class Camera2DFollow : FollowBase {

        private void Start() {
            InitializeSearchName("Player");
            base.Start();
        }


        // Update is called once per frame
        private void Update() {
            //Dead player check
            if (Target == null) {
                FindPlayer();
                return;
            }

            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (Target.position - LastTargetPosition).x;

            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadMoveThreshold;

            if (updateLookAheadTarget) {
                LookAheadPos = LookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
            } else {
                LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, Dampening);

            //clamp the camera - value doesn't go below or above 
            newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);

            transform.position = newPos;

            LastTargetPosition = Target.position;
        }
    }
}
