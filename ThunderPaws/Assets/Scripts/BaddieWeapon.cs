using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieWeapon : AbstractWeapon {
    [Header("Mutator Attributes")]
    public float shotYMutatorLow = 0.5f;
    public float shotYMutatorHigh = 1.5f;
    private BaddieAI baddieAI;

    public LayerMask whatToHit;

    protected void Start() {
        base.Start();
        baddieAI = gameObject.transform.parent.transform.parent.GetComponent<BaddieAI>();
        if(baddieAI == null) {
            Debug.LogError("Weapon.cs: No BaddieAI script found on Baddie");
        }
    }

    private void Update() {
        //if the target is within the killzone, shoot
        if (baddieAI.state == BaddieAI.BaddieState.ATTACK && Time.time > _timeToFire) {//TODO: more intelligent ai
            //update time to fire
            _timeToFire = Time.time + 1 / fireRate;
            Shoot();
        }
    }

    //Uses the defined high and low values to get a random number between them multiplied by either 1 or -1 for high shots or low shots
    //Useful - but not atm
    private float GetShotMutator() {
        return (Random.Range(shotYMutatorLow, shotYMutatorHigh) * (Random.Range(0,2)*2-1));

    }

    //fire a projectile
    private void Shoot() {
        //store mouse position (B)
        Vector2 targetPosition = new Vector2(baddieAI.target.position.x, baddieAI.target.position.y /*+ GetShotMutator()*/);
        //store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        //collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, targetPosition - firePointPosition, 100, whatToHit);


        //generate bullet effect
        if (Time.time >= timeToSpawnEffect) {
            //bullet effect position data
            Vector3 hitPosition;
            Vector3 hitNormal;

            hitPosition = (targetPosition - firePointPosition) * 100; //arbitrarily large number so the bullet trail flys off the camera
            if (shot.collider != null) {
                hitNormal = shot.normal;//if we most likely hit something store the normal so the particles make sense when they shoot out
                hitPosition = shot.point;
            } else {
                hitNormal = new Vector3(999, 999, 999); //rediculously huge so we can use it as a sanity check for the effect
            }

            //actually instantiate the effect
            GenerateEffect(hitPosition, hitNormal, whatToHit);
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

}

