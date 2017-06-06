using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float moveSpeed = 10f;
    private bool shouldFire = false;
    private Vector3 _targetPos;
    private Vector3 _targetNormal;

    public Transform hitPrefab;

    private void Start() {
        if(hitPrefab == null) {
            Debug.LogError("No HitPrefab was found on bullet");
        }
    }

    // Update is called once per frame
    void Update () {
        if (shouldFire) {
            Vector3 dir = _targetPos - transform.position;
            float distanceThisFrame =  moveSpeed * Time.deltaTime;
            Debug.Log("target : " + _targetPos + " and dir.mag : " + dir.magnitude + " and distanceThisFrame : " + distanceThisFrame);
            //length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                HitTarget();
                return;
            }

            //move as a constant speed
            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        }
    }
    
    //tell the update statement to move the bullet
    public void Fire(Vector3 targetPos, Vector3 targetNormal) {
        shouldFire = true;
        _targetPos = targetPos;
        _targetNormal = targetNormal;
    }

    public void HitTarget() {
        //mask it so when we hit something the particles shoot OUT from it.
        Transform hitParticles = Instantiate(hitPrefab, _targetPos, Quaternion.FromToRotation(Vector3.up, _targetNormal)) as Transform;
        //Destroy hit particles
        Destroy(hitParticles.gameObject, 1f);
        Destroy(gameObject);
    }
}
