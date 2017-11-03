using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tells the baddie how it should act based on its surroundings
/// Handles checking for targets, determining what state its in, basically what the baddie should do
/// </summary>
public class BaddieAIController : BaddieBaseAIController {
    /// <summary>
    /// Reference to the correct type of this AI
    /// </summary>
    public Baddie Baddie;

    /// <summary>
    /// Indicates what the baddie cannot see through
    /// </summary>
    private readonly List<string> _cannotSeeThrough = new List<string> {"OBSTACLE-SOLID"};

    /// <summary>
    /// Max distance the target can be away from this and still be noticed
    /// This changes if this is attacking though and the target is still within less than _noticeThreshold away
    /// </summary>
    private float _noticeThreshold = 15f;
    /// <summary>
    /// Max distance the target can be away from this and still be attacked
    /// </summary>
    private float _attackThreshold = 13f;
    /// <summary>
    /// If the target gets within this distance the object will retreat to a further safer distance
    /// </summary>
    private float _personalSpaceThreshold = 5f;

    // Use this for initialization
    void Start() {
        InitializeAIValues("MachineGun_Badddie");
        if (Lifeform.GetType() == typeof(Baddie)) {
            Baddie = Lifeform as Baddie;
        }else {
            throw new InvalidCastException("Expected typeof Baddie but found typeof " + Lifeform.GetType());
        }
        //Default base state
        Baddie.State = MentalStateEnum.NEUTRAL;
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
        }else {
            LockOnTarget();
        }
        if(Baddie.State == MentalStateEnum.ATTACK || Baddie.State == MentalStateEnum.PURSUE_ATTACK) {
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
                Baddie.TargetOnLeft = IsFacingLeft();
                Baddie.Target = Target;

                //First check if there is an obstacle between them - baddies can't see through walls unless they have some previous knowledge: Attacking, Pursuing...etc
                if (Baddie.State == MentalStateEnum.NEUTRAL) {
                    //Raycast to check if we could potentially the target
                    float distance = Vector2.Distance(transform.position, Target.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Target.position - transform.position, distance, Baddie.GetControllerLayerMask());
                    //This indicates the target is within range, however we hit something we cannot see through 
                    if (hit) {
                        if (_cannotSeeThrough.Contains(hit.collider.gameObject.tag)) {
                            return;
                        }
                    }
                }

                if (distanceToTarget <= _personalSpaceThreshold) {
                    //Update the direction we need ot move to create space
                    Baddie.State = MentalStateEnum.PERSONAL_SPACE;
                } else if (distanceToTarget <= _attackThreshold) {
                    Baddie.State = MentalStateEnum.ATTACK;
                } else if (distanceToTarget <= _noticeThreshold) {
                    //The target was within the Attack zone, but retreated
                    //Follow the target as long as its within the NOTICE zone
                    //The Target moves 2units faster than this object so they will make their escape, but at least follow them for a little while
                    if(Baddie.State == MentalStateEnum.ATTACK || Baddie.State == MentalStateEnum.PURSUE_ATTACK) {
                        Baddie.State = MentalStateEnum.PURSUE_ATTACK;
                    } else {
                        Baddie.State = MentalStateEnum.NOTICE;
                    }
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

    protected override IEnumerator SearchForPlayer() {
        //Search for the player
        GameObject searchResult = GameObject.FindGameObjectWithTag(_targetTag);
        if (searchResult == null) {//search only twice a second until found
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        } else {
            //Found the player - set it as the target and stop searching and reinvoke the state updating
            Target = searchResult.transform;
            _baddieWeapon.AttackTarget = Target;
            _searchingForPlayer = false;
            //twice a second look around for the target
            InvokeRepeating("UpdateState", 0f, 0.5f);
            yield break;
        }
    }

    /// <summary>
    /// Locate the player in the world
    /// </summary>
    /// <returns></returns>


}
