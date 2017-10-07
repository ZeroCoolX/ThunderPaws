using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    /// <summary>
    /// How fast the bullet travels
    /// </summary>
    public float MoveSpeed = 10f;

    /// <summary>
    /// How much damage this bullet does
    /// </summary>
    public int Damage = 5;

    /// <summary>
    /// Precalculated values necessary for determining how to spray the particles, where we THINK the collision will take place
    /// </summary>
    private Vector3 _targetPos;
    private Vector3 _targetNormal;
    /// <summary>
    /// Specifies what direction the bullet should move
    /// </summary>
    private Vector3 _targetDirection;
    /// <summary>
    /// Prefab referense for hit particles
    /// </summary>
    public Transform hitPrefab;
    /// <summary>
    /// LayerMask indicating what to hit
    /// </summary>
    [SerializeField]
    private LayerMask WhatToHit;

    private void Start() {
        //Validate the hit prefab is set
        if(hitPrefab == null) {
            Debug.LogError("No HitPrefab was found on bullet");
            throw new UnassignedReferenceException();
        }
    }

    void Update () {
        //Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, _targetPos - transform.position);
        if (possibleHit.collider != null){
            //Mini raycast to check handle ellusive targets
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, _targetPos - transform.position, 0.2f, WhatToHit);
            if (distCheck.collider != null) {
                HitTarget(transform.position, distCheck.collider);
                return;
            }

            //Last check is simplest check
            Vector3 dir = _targetPos - transform.position;
            float distanceThisFrame = MoveSpeed * Time.deltaTime;
            //Length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                //Make sure the player didn't dodge out of the way
                distCheck = Physics2D.Raycast(transform.position, _targetPos - transform.position, 0.2f, WhatToHit);
                if (distCheck.collider != null) {
                    HitTarget(transform.position, distCheck.collider);
                    return;
                }
            }
        }
        //Move as a constant speed
        transform.Translate(_targetDirection.normalized * MoveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Once the bullet leaves the Cameras viewport destroy it
    /// </summary>
    void OnBecameInvisible() {
        Destroy(gameObject);
    }

    /// <summary>
    /// Set the layermask
    /// </summary>
    /// <param name="parentLayerMask"></param>
    public void SetLayerMask(LayerMask parentLayerMask) {
        WhatToHit = parentLayerMask;
    }

    /// <summary>
    /// Tell the update statement wwhere to move the bullet
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="targetNormal"></param>
    public void Fire(Vector3 targetPos, Vector3 targetNormal) {
        _targetPos = targetPos;
        _targetNormal = targetNormal;
        _targetDirection = _targetPos - transform.position;
    }

    /// <summary>
    /// Destroy and generate effects
    /// </summary>
    /// <param name="hitPos"></param>
    /// <param name="hitObject"></param>
    public void HitTarget(Vector3 hitPos, Collider2D hitObject) {
        //Damage whoever we hit
        switch (hitObject.gameObject.tag) {
            case "Player":
                Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
                Player player = hitObject.GetComponent<Player>();
                if(player != null) {
                    player.DamageHealth(Damage);
                }
                break;
            case "BADDIE":
                Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
                Baddie baddie = hitObject.GetComponent<Baddie>();
                if (baddie != null) {
                    baddie.DamageHealth(Damage);
                }
                break;
        }

        //Mask it so when we hit something the particles shoot OUT from it.
        Transform hitParticles = Instantiate(hitPrefab, hitPos, Quaternion.FromToRotation(Vector3.up, _targetNormal)) as Transform;
        //Destroy hit particles
        Destroy(hitParticles.gameObject, 1f);
        Destroy(gameObject);
    }
}
