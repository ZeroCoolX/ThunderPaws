using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tells the baddie how it should act based on its surroundings
/// Handles checking for targets, determining what state its in, basically what the baddie should do
/// </summary>
public class BaddieAIController : MonoBehaviour {//TODO: Extend off parent AIController
    /// <summary>
    /// Reference to the object the AI is effecting
    /// </summary>
    public Baddie Baddie;
    /// <summary>
    /// Target transform if needed
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Tag to search for if searching
    /// </summary>
    private string _targetTag = "Player";
    /// <summary>
    /// Reference to graphics for rotatipn/flipping
    /// </summary>
    public Transform BaddieGraphics;
    /// <summary>
    /// Axis around which we rotate the arm
    /// </summary>
    public Transform ArmRotationAxis;
    /// <summary>
    /// Indicates the arm is facing left
    /// </summary>
    private bool _armIsLeft = false;

    /// <summary>
    /// AI needs a reference to its weapon so it can control when its firing
    /// </summary>
    private BaddieWeapon _baddieWeapon;

    //TODO: Find a beter way to do this
    /// <summary>
    /// Determines if we should search for the player or not
    /// </summary>
    private bool _searchingForPlayer = false;

    /// <summary>
    /// Max distance the target can be away from this and still be noticed
    /// This changes if this is attacking though and the target is still within less than _noticeThreshold away
    /// </summary>
    private float _noticeThreshold = 15f;
    /// <summary>
    /// Max distance the target can be away from this and still be attacked
    /// </summary>
    private float _attackThreshold = 10f;
    /// <summary>
    /// If the target gets within this distance the object will retreat to a further safer distance
    /// </summary>
    private float _personalSpaceThreshold = 3f;

    // Use this for initialization
    void Start() {
        //Default base state
        Baddie.State = MentalStateEnum.NEUTRAL;
        BaddieGraphics = transform.FindChild("Graphics");
        if (BaddieGraphics == null) {
            //couldn't find player graphics 
            Debug.LogError("Cannot find Graphics on baddie");
            throw new MissingReferenceException();
        }

        var weaponTransform = ArmRotationAxis.FindChild("proto_machine_gun");
        if(weaponTransform != null) {
            _baddieWeapon = weaponTransform.GetComponent<BaddieWeapon>();
            _baddieWeapon.AttackTarget = Target;
        }

        //Search for the target in game if there isn't one set - meaning he died and is respawning
        if (Target == null) {
            //Player might be dead so search, but only if we should search
            if (!_searchingForPlayer) {
                CoolOffBaddie();
            }
            return;
        }

        //twice a second look around for the target
        InvokeRepeating("UpdateState", 0f, 0.5f);
    }

    // Update is called once per frame
    void Update() {
        if (Target == null) {
            //Player might be dead so search
            if (!_searchingForPlayer) {//there is no target so search
                CoolOffBaddie();
            }
            return;
        }
        if(Baddie.State == MentalStateEnum.NEUTRAL) {
            _baddieWeapon.ShouldShoot = false;
        }            
        if(Baddie.State == MentalStateEnum.NOTICE || Baddie.State == MentalStateEnum.ATTACK || Baddie.State == MentalStateEnum.PERSONAL_SPACE) {
            LockOnTarget();
        }
        if(Baddie.State == MentalStateEnum.ATTACK) {
            _baddieWeapon.ShouldShoot = true;
        }
    }


    /// <summary>
    /// Based off target and this position update the state of the baddie
    /// </summary>
    private void UpdateState() {
        try {
            if (Target != null) {
                //Get the distance between them
                float distanceToTarget = Mathf.Abs(transform.position.x - Target.transform.position.x);
                if (distanceToTarget <= _personalSpaceThreshold) {
                    //Update the direction we need ot move to create space
                    Baddie.CreateSpaceDir = IsFacingLeft() ? 1 : -1;
                    Baddie.State = MentalStateEnum.PERSONAL_SPACE;
                } else if (distanceToTarget <= _attackThreshold) {
                    Baddie.State = MentalStateEnum.ATTACK;
                } else if (distanceToTarget <= _noticeThreshold /*&& State != MentalState.ATTACK*/) {//TODO: add that back in later tonight
                    Baddie.State = MentalStateEnum.NOTICE;
                } else {
                    Baddie.State = MentalStateEnum.NEUTRAL;
                }
            }
        } catch (UnityException e) {
            print("Error trying to update baddie state: " + e.Message);
            CoolOffBaddie();
        }
    }

    /// <summary>
    ///  TODO: this is just for now - When the target no longer exists in game - I.E. died, stop updaing states and shooting 
    /// </summary>
    private void CoolOffBaddie() {
        _searchingForPlayer = true;
        //Tell the weapon to stop shooting
        Baddie.State = MentalStateEnum.NEUTRAL;
        //Cancel the state update until we have a target to actually update our state for
        CancelInvoke("UpdateState");
        StartCoroutine(SearchForPlayer());
    }

    /// <summary>
    /// Locate the player in the world
    /// </summary>
    /// <returns></returns>
    IEnumerator SearchForPlayer() {
        //Search for the player
        GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
        if (searchResult == null) {//search only twice a second until found
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        } else {
            //Found the player - set it as the target and stop searching and reinvoke the state updating
            Target = searchResult.transform;
            _searchingForPlayer = false;
            //twice a second look around for the target
            InvokeRepeating("UpdateState", 0f, 0.5f);
            yield break;
        }
    }

    /// <summary>
    /// Face the target and track with where the arm is pointing
    /// </summary>
    private void LockOnTarget() {
        //rotate left or right to face target
        BaddieGraphics.rotation = Quaternion.Euler(0f, IsFacingLeft() ? -180f : 360f, 0f);

        Vector3 diff = Target.position - ArmRotationAxis.position;
        //Normalize the vector x + y + z = 1
        diff.Normalize();
        //find the angle in degrees
        float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //apply the rotation
        ArmRotationAxis.rotation = Quaternion.Euler(0f, 0f, rotZ);//degrees not radians

        //invert the arm only iff necessary
        if (IsFacingLeft() && !_armIsLeft) {
            InvertArm();
        } else if (_armIsLeft && !IsFacingLeft()) {
            InvertArm();
        }
    }

    /// <summary>
    /// Invert the arm when necessary so its never upside down
    /// Allows 360 degree motion
    /// </summary>
    private void InvertArm() {
        //switch the way the arm is labeled as facing
        _armIsLeft = !_armIsLeft;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = ArmRotationAxis.localScale;
        theScale.y *= -1;
        ArmRotationAxis.localScale = theScale;
    }

    /// <summary>
    /// Helper function to tell us what direction we're facing relative to the target
    /// result  less than or equal to 0 indicates facing LEFT
    /// </summary>
    private bool IsFacingLeft() {
        return (Target.position.x - transform.position.x) <= 0;
    }
}
