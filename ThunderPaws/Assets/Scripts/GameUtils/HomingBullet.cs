using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingBullet : BulletBase {
    /// <summary>
    /// The distance from the target to which the missile no longer tracks the target and fires straight into whatever direction its currently going
    /// </summary>
    private float _criticalZoneDistance = 3f;

    /// <summary>
    /// Indicates the missile has passed through the critical zone and should no longer track the target
    /// </summary>
    private bool _criticalZoneEntered = false;

    /// <summary>
    /// how many seconds until the rocket's "homing" logic kicks in
    /// </summary>
    private float _freeFlyTime;
    /// <summary>
    /// Rate aat which we should wait
    /// </summary>
    public float FreeFlyDelay = 0.5f;

    /// <summary>
    /// Just used as a reference for the Mathf.SmoothDamp function
    /// </summary>
    protected float VelocitySmoothing;

    /// <summary>
    /// If this bullet type is a homing type, it needs a target
    /// </summary>
    public Transform Target;

    private Vector3 _veloicty;

    private void Start() {
        base.Start();
        Target = GameObject.FindGameObjectWithTag("Player").transform;
        _freeFlyTime = Time.time + FreeFlyDelay;
        MaxLifetime = 30;
    }

    void Update() {
        if (Target == null) {
            if(GameObject.FindGameObjectWithTag("Player") != null) {
                Target = GameObject.FindGameObjectWithTag("Player").transform;
            }
            return;
        } 
        //Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, TargetDirection);
        Vector3 dir;
        if (possibleHit.collider != null) {
            //Mini raycast to check handle ellusive targets
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, TargetDirection, 0.2f, WhatToHit);
            if (distCheck.collider != null) {
                HitTarget(transform.position, distCheck.collider);
                //We don't want to stop the bullet trajectory if we're hitting the trigger.
                //If we're on the ground - which is the only time the rocket jump boost can be applied, the bullet should hit the ground instead of the trigger
                if (distCheck.collider.gameObject.tag != "ROCKETJUMPTRIGGER") {
                    return;
                }
            }

            //Last check is simplest check
            dir = TargetPos - transform.position;
            float distanceThisFrame = MoveSpeed * Time.deltaTime;
            //Length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                //Make sure the player didn't dodge out of the way
                distCheck = Physics2D.Raycast(transform.position, TargetDirection, 0.2f, WhatToHit);
                if (distCheck.collider != null) {
                    HitTarget(transform.position, distCheck.collider);
                    //We don't want to stop the bullet trajectory if we're hitting the trigger.
                    //If we're on the ground - which is the only time the rocket jump boost can be applied, the bullet should hit the ground instead of the trigger
                    if (distCheck.collider.gameObject.tag != "ROCKETJUMPTRIGGER") {
                        return;
                    }
                }
            }
        }

        float distanceToTarget = Vector3.Distance(Target.position,transform.position);
        //Track the target as long as we're outside the critical zone
        if (!_criticalZoneEntered && distanceToTarget > _criticalZoneDistance) {
            _veloicty = TargetDirection.normalized * MoveSpeed;
            if (Time.time > _freeFlyTime) {
                UpdateTracking();
            }
        } else {
            //Set the current direction infinetly far so the missile moves in that direction forever
            _criticalZoneEntered = true;
            TargetDirection *= 900f;
        }
        //Move as a constant speed
        transform.Translate(_veloicty * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Updates the targetdirection continuously so that it will track the target
    /// </summary>
    private void UpdateTracking() {
        TargetDirection = Target.position - transform.position;
        var diff = TargetDirection;
        //Normalize the vector x + y + z = 1
        diff.Normalize();

        //find the angle in degrees
        float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        //apply the rotation
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);//degrees not radians
    }

    /// <summary>
    /// Destroy and generate effects
    /// </summary>
    /// <param name="hitPos"></param>
    /// <param name="hitObject"></param>
    protected override void HitTarget(Vector3 hitPos, Collider2D hitObject) {//TODO: apply AOE damage for homing missile.
        //Damage whoever we hit - or rocket jump
        Player player;
        switch (hitObject.gameObject.tag) {
            case "Player":
                Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
                player = hitObject.GetComponent<Player>();
                if (player != null) {
                    player.DamageHealth(Damage);
                }
                break;
            case "BULLET":
                Debug.Log("We hit " + hitObject.name + " and and detonated!");
                player = hitObject.GetComponentInParent<Player>();
                if (player != null) {
                    player.AllowRocketJump();
                }
                break;
            case "BADDIE":
                Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
                Baddie baddie = hitObject.GetComponent<Baddie>();
                if (baddie != null) {
                    //Naturally someone would realize they're being attacked if they were shot so retaliate
                    if (baddie.State != MentalStateEnum.ATTACK) {
                        baddie.State = MentalStateEnum.ATTACK;
                    }
                    baddie.DamageHealth(Damage);
                }
                break;
        }
        if (!hitObject.gameObject.tag.Equals("ROCKETJUMPTRIGGER")) {
            //Mask it so when we hit something the particles shoot OUT from it.
            Transform hitParticles = Instantiate(HitPrefab, hitPos, Quaternion.FromToRotation(Vector3.up, TargetNormal)) as Transform;
            //Destroy hit particles
            Destroy(hitParticles.gameObject, 1f);
            Destroy(gameObject);
        }
    }
}
