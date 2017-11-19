using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTurret : AbstractWeapon {
    public Transform AttackTarget;

    /// <summary>
    /// Gets set by the BaddieAI
    /// </summary>
    public bool ShouldShoot = false;
    /// <summary>
    /// Layermask indicating what to hit
    /// </summary>
    public LayerMask WhatToHit;
    /// <summary>
    /// This only pertains to the homing missile.
    /// Optional value that specifies when the missile should start tracking the player (after x seconds)
    /// </summary>
    public float FreeFlyTime = 0.5f;

    protected void Start() {
        base.Start();
    }

    private void Update() {
        if (AttackTarget != null) {
            //If the target is within the killzone, shoot
            if (ShouldShoot && Time.time > _timeToFire) {
                //Update time to fire
                _timeToFire = Time.time + 1 / FireRate;
                Shoot();
            }
        }
    }


    /// <summary>
    /// Fire a projectile straight up - the tracking on the missile itself will begin updating aafter a set interval
    /// </summary>
    private void Shoot() {
        //Store mouse position (B)
        //Generate bullet effect
        if (Time.time >= TimeToSpawnEffect) {
            //Actually instantiate the effect
            GenerateEffect(Vector2.up * 100, new Vector3(999, 999, 999), WhatToHit, "BADDIEBULLET", FreeFlyTime);
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
        }
    }
}
