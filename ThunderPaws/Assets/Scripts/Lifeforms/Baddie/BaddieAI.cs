﻿using System.Collections;
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

    public float moveSpeed = 6f;
    Vector3 velocity;
    float accelerationTimeAirborn = 0.2f;//change direction a little slower when in the air
    float accelerationTimeGrounded = 0.1f;
    float velocityXSmoothing;
    float gravity;
    public float maxJumpHeight = 4f;//how high
    public float minJumpHeight = 1f;
    public float timeToJumpApex = 0.4f;//how long till they reach highest point

    //determines the actions taken
    public enum BaddieState { NEUTRAL=0, WARNING=1, ATTACK=2};
    private BaddieState _state;
    public BaddieState state { get { return _state; } }

    [Header("Ranges")]
    // x > 15 baddie won't be able to see player
    public float dangerRange = 10f;  //baddie will attack on site

    Color debugColor = Color.white;

    private void Awake() {
        _state = BaddieState.NEUTRAL;
    }

    private void Start() {
        baddieGraphics = transform.FindChild("Graphics");
        if (baddieGraphics == null) {
            //couldn't find player graphics 
            Debug.LogError("Cannot find Graphics on baddie");
            throw new MissingReferenceException();
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
        if(_state == BaddieState.NEUTRAL) {
            CalculateVelocity();
            //move around
           // Explore();
        }else if(_state == BaddieState.ATTACK) {
            LockOnTarget();
        }

        //Just for now TODO: change this 
        switch (_state) {
            case BaddieState.NEUTRAL:
                gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
                break;
            case BaddieState.WARNING:
                gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
                break;
            case BaddieState.ATTACK:
                gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                break;
        }

    }

    private void UpdateState() {
        //get the distance between them
        float distanceToTarget = transform.position.x - target.transform.position.x;
        if(target != null) {
            if(Mathf.Abs(distanceToTarget) <= dangerRange) {
                debugColor = Color.red;
                //attack!
                _state = BaddieState.ATTACK;
            } else {
                debugColor = Color.white;
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

    private void CalculateVelocity() {
        //must calculate x first before dealing with wall sliding
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        float targetVelocityX = moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,accelerationTimeGrounded);
        velocity.y += gravity * Time.deltaTime;
    }

    private void Explore() {
        transform.Translate(velocity * Time.deltaTime);
    }

    //When the target no longer exists in game - I.E. died, stop updaing states and shooting 
    private void CoolOffBaddie() {
        _searchingForPlayer = true;
        //Tell the weapon to stop shooting
        _state = BaddieState.NEUTRAL;
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
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
