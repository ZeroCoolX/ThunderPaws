using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionWeapon : AbstractWeapon {

    /// <summary>
    /// Layermask indicating what to hit
    /// </summary>
    public LayerMask WhatToHit;
    /// <summary>
    /// Testing value right now - single hard coded target
    /// </summary>
    public Transform Target;
    private bool _shouldShoot = false;

    // Use this for initialization
    void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.F)) {
            _shouldShoot = !_shouldShoot;
        }
        if (_shouldShoot) {
            Shoot();
        }
	}

    /// <summary>
    /// Fire a projectile
    /// </summary>
    private void Shoot() {
        //Store mouse position (B)
        Vector2 targetPosition = new Vector2(Target.position.x, Target.position.y);
        //Store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
        //Collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, targetPosition - firePointPosition, 100, WhatToHit);


        //Generate bullet effect
        if (Time.time >= TimeToSpawnEffect) {
            //Bullet effect position data
            Vector3 hitPosition;
            Vector3 hitNormal;
            //Arbitrarily large number so the bullet trail flys off the camera
            hitPosition = (targetPosition - firePointPosition) * 100;
            if (shot.collider != null) {
                //If we most likely hit something store the normal so the particles make sense when they shoot out
                hitNormal = shot.normal;
                hitPosition = shot.point;
            } else {
                //Rediculously huge so we can use it as a sanity check for the effect
                hitNormal = new Vector3(999, 999, 999);
            }

            //Actually instantiate the effect
            GenerateEffect(hitPosition, hitNormal, WhatToHit);
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
        }
    }
}
