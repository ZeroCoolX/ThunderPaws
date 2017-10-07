using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : AbstractWeapon {
    [Header("Camera Attributes")]
    //handle camera shake for shots
    public float camShakeAmount = 0.025f;
    public float camShakeLength = 0.1f;
    private CameraShake _camShake;

    public LayerMask whatToHit;

    protected void Start() {
        //Call parent abstract Start()
        base.Start();
        _camShake = GameMaster.instance.GetComponent<CameraShake>();
        if(_camShake == null) {
            Debug.LogError("Weapon.cs: No CameraShake found on game master");
            throw new MissingComponentException();
        }
    }

    private void Update() {
        if (fireRate == 0) {//single fire
            if (Input.GetButtonDown("Fire1")) {
                Shoot();
            }
        } else {//auto
            if(Input.GetButton("Fire1") && Time.time > _timeToFire) {
                //update time to fire
                _timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }
    }

    //fire a projectile
    private void Shoot() {
        //store mouse position (B)
        Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        //store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        //collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, mousePosition - firePointPosition, 100, whatToHit);

        //generate bullet effect
        if (Time.time >= timeToSpawnEffect) {
            //bullet effect position data
            Vector3 hitPosition;
            Vector3 hitNormal;

            //precalculate so if we aren't shooting at anything at least the normal is correct - i think....
                hitPosition = (mousePosition - firePointPosition) * 100; //arbitrarily laarge number so the bullet trail flys off the camera
            if (shot.collider != null) {
                hitNormal = shot.normal;//if we most likely hit something store the normal so the particles make sense when they shoot out
                hitPosition = shot.point;
            } else {
                hitNormal = new Vector3(999, 999, 999); //rediculously huge so we can use it as a sanity check for the effect
            }

            //actually instantiate the effect
            GenerateEffect(hitPosition, hitNormal, whatToHit);
            GenerateCamShake();
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

    private void GenerateCamShake() {
        //Generate camera shake
        _camShake.Shake(camShakeAmount, camShakeLength);

        //TODO: generate audio
    }

}
