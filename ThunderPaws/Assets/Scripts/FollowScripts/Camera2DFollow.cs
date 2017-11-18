using System;
using UnityEngine;

namespace UnityStandardAssets._2D {
    public class Camera2DFollow : FollowBase {

        private float _currentXOffset;

        private void Start() {
            _currentXOffset = OffsetX;
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
            //if(xMoveDelta != 0) {
            //    if (PlayerScript.FacingRight && Mathf.Sign(xMoveDelta) > 0 || !PlayerScript.FacingRight && Mathf.Sign(xMoveDelta) < 0) {
            //        _currentXOffset = OffsetX * Mathf.Sign(xMoveDelta);
            //    } else {
            //        _currentXOffset = OffsetX * Mathf.Sign(_currentXOffset);
            //    }
            //    newPos = new Vector3(newPos.x + _currentXOffset, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);
            //    transform.position = newPos;

            //    LastTargetPosition = Target.position;
            //}else {
                newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);
            //}
            //TODO: The camera offset to allow more space in front of the player is not great-  but manageable atm - must fix in the future
            //newPos = new Vector3(newPos.x + _currentXOffset, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);
           // newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);

            transform.position = newPos;

            LastTargetPosition = Target.position;
        }
    }
}
