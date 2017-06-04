using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieAI : MonoBehaviour {

    public Transform target;
    private string _targetTag = "Player";
    public float turnSpeed = 10f;

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

        //LockOnTarget();

        switch (state) {
            case BaddieState.NEUTRAL:
                gameObject.GetComponent<Renderer>().material.color = Color.white;
                break;
            case BaddieState.WARN:
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case BaddieState.DANGER:
                gameObject.GetComponent<Renderer>().material.color = Color.red;
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





}
