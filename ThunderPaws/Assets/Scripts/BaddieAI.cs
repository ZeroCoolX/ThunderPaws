using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieAI : MonoBehaviour {

    public Transform target;
    private string _targetTag = "Player";
    public float turnSpeed = 10f;
    public Transform armRotationAxis;
    private bool _armIsLeft = false;

    public enum BaddieState { NEUTRAL=0, WARN=1, DANGER=2};

    private BaddieState state;

    [Header("General")]
    // x > 15 baddie won't be able to see player
    public float warningRange = 7f;   //baddie will notice, but not care
    public float dangerRange = 4f;  //baddie will attack on site

    private void Awake() {
        state = BaddieState.NEUTRAL;
    }

    private void Start() {
        //twice a second look around for the target
        InvokeRepeating("UpdateState", 0f, 0.5f);
    }

    private void Update() {
        if(target == null) {
            Debug.LogError("BaddieAI.cs: target is null!");
            return;
        }

        if(state == BaddieState.WARN || state == BaddieState.DANGER) {
            LockOnTarget();
        }

        switch (state) {
            case BaddieState.NEUTRAL:
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case BaddieState.WARN:
                gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
                break;
            case BaddieState.DANGER:
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                break;
        }
    }

    private void UpdateState() {
        //get the player object
        //get the distance between them
        float distanceToTarget = transform.position.x - target.transform.position.x;
        if(target != null) {
            if(distanceToTarget <= dangerRange) {
                //attack!
                state = BaddieState.DANGER;
            }else if(distanceToTarget <= warningRange) {
                //notice
                state = BaddieState.WARN;
            } else {
                state = BaddieState.NEUTRAL;
            }
        }
    }

    private void LockOnTarget() {
        //rotate left or right to face target
        float faceDir = target.position.x - transform.position.x;
        transform.rotation = Quaternion.Euler(0f, faceDir <= 0 ? -180f : 360f, 0f);

        Vector3 diff = target.position - armRotationAxis.position;
        //Normalize the vector x + y + z = 1
        diff.Normalize();
        //find the angle in degrees
        float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //apply the rotation
        armRotationAxis.rotation = Quaternion.Euler(0f, 0f, rotZ);//degrees not radians

        //invert the arm only iff necessary
        if (faceDir <= 0 && !_armIsLeft) {
            InvertArm();
        }else if(_armIsLeft && faceDir > 0) {
            InvertArm();
        }
    }

    private void InvertArm() {
        //switch the way the arm is labeled as facing
        _armIsLeft = !_armIsLeft;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = armRotationAxis.localScale;
        theScale.y *= -1;
        armRotationAxis.localScale = theScale;
    }



}
