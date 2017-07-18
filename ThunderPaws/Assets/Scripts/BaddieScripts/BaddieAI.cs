using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieAI : MonoBehaviour {

    //will most always be the player
    public Transform target;
    private string _targetTag = "Player";

    //mechanics of movement
    public float turnSpeed = 10f;
    public Transform armRotationAxis;
    private bool _armIsLeft = false;
    public Transform baddieGraphics;

    private bool _searchingForPlayer = false;

    //determines the actions taken
    public enum BaddieState { NEUTRAL=0, ATTACK=1};
    private BaddieState _state;
    public BaddieState state { get { return _state; } }

    [Header("Ranges")]
    // x > 15 baddie won't be able to see player
    public float dangerRange = 10f;  //baddie will attack on site


    private void Awake() {
        _state = BaddieState.NEUTRAL;
    }

    private void Start() {
        baddieGraphics = transform.FindChild("Graphics");
        if (baddieGraphics == null) {
            //couldn't find player graphics 
            Debug.LogError("Cannot find Graphics on baddie");
        }

        //Search for the target in game if there isn't one set - meaning he died and is respawning
        if (target == null) {
            //Player might be dead so search
            if (!_searchingForPlayer) {//there is no target so search
                CoolOffBaddie();
            }
            return;
        }

        //twice a second look around for the target
        InvokeRepeating("UpdateState", 0f, 0.5f);
    }

    private void Update() {
        if(target == null) {
            //Player might be dead so search
            if (!_searchingForPlayer) {//there is no target so search
                CoolOffBaddie();
            }
            return;
        }

        if(_state == BaddieState.ATTACK) {
            LockOnTarget();
        }

        //Just for now TODO: change this 
        switch (_state) {
            case BaddieState.NEUTRAL:
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case BaddieState.ATTACK:
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                break;
        }
    }

    private void UpdateState() {
        //get the distance between them
        float distanceToTarget = transform.position.x - target.transform.position.x;
        if(target != null) {
            if(Mathf.Abs(distanceToTarget) <= dangerRange) {
                //attack!
                _state = BaddieState.ATTACK;
            } else {
                _state = BaddieState.NEUTRAL;
            }
        }
    }

    IEnumerator SearchForPlayer() {
        //Search for the player
        GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
        if(searchResult == null) {//search only twice a second until found
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        }else {
            //Found the player - set it as the target and stop searching and reinvoke the state updating
            target = searchResult.transform;
            _searchingForPlayer = false;
            //twice a second look around for the target
            InvokeRepeating("UpdateState", 0f, 0.5f);
            yield break;
        }
    }

    //When the target no longer exists in game - I.E. died, stop updaing states and shooting 
    private void CoolOffBaddie() {
        _searchingForPlayer = true;
        //Tell the weapon to stop shooting
        _state = BaddieState.NEUTRAL;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        //Cancel the state update until we have a target to actually update our state for
        CancelInvoke("UpdateState");
        StartCoroutine(SearchForPlayer());
    }

    private void LockOnTarget() {
        //rotate left or right to face target
        float faceDir = target.position.x - transform.position.x;
        baddieGraphics.rotation = Quaternion.Euler(0f, faceDir <= 0 ? -180f : 360f, 0f);

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
