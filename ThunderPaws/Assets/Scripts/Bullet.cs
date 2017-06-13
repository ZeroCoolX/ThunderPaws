using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    //how fast the bullet travels
    public float moveSpeed = 10f;

    //precalculated values necessary for determining how to spray the particles, where we THINK the collision will take place, and in what direction to move the bullet
    private Vector3 _targetPos;
    private Vector3 _targetNormal;
    private Vector3 _targetDirection;

    public Transform hitPrefab;

    //Make sure we have a bullet visually
    private void Start() {
        if(hitPrefab == null) {
            Debug.LogError("No HitPrefab was found on bullet");
        }
    }

    void Update () {
        //move as a constant speed
        transform.Translate(_targetDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }

    //Once the bullet leaves the Cameras viewport destroy it
    void OnBecameInvisible() {
        Destroy(gameObject);
    }

    //Tell the update statement wwhere to move the bullet
    public void Fire(Vector3 targetPos, Vector3 targetNormal) {
        _targetPos = targetPos;
        _targetNormal = targetNormal;
        _targetDirection = _targetPos - transform.position;
    }

    //We've hit something! - destroy with particle effects and do the required damage
    private void OnTriggerEnter2D(Collider2D collision) {
        switch (collision.gameObject.tag) {
            case "Player":
                if (gameObject.tag != "Player") {//don't hit ourselves
                    HitTarget();
                    Debug.Log("DAMAGE PLAYER");
                }
                break;
            case "BADDIE":
                if(gameObject.tag != "BADDIE") {//no baddie friendly fire
                    HitTarget();
                    Debug.Log("DAMAGE BADDIE");
                }
                break;
            case "ENVIROMENT":
                HitTarget();
                Debug.Log("Hit enviroment");
                break;
            default:
                break;
        }
    }

    //Destroy and generate effects
    public void HitTarget() {
        //mask it so when we hit something the particles shoot OUT from it.
        Transform hitParticles = Instantiate(hitPrefab, _targetPos, Quaternion.FromToRotation(Vector3.up, _targetNormal)) as Transform;
        //Destroy hit particles
        Destroy(hitParticles.gameObject, 1f);
        Destroy(gameObject);
    }
}
