using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    //how fast the bullet travels
    public float moveSpeed = 10f;

    //how much damage this bullet does
    public int damage = 5;

    //precalculated values necessary for determining how to spray the particles, where we THINK the collision will take place, and in what direction to move the bullet
    private Vector3 _targetPos;
    private Vector3 _targetNormal;
    private Vector3 _targetDirection;

    public Transform hitPrefab;
    [SerializeField]
    private LayerMask whatToHit;

    //Make sure we have a bullet visually
    private void Start() {
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
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, _targetPos - transform.position, 0.2f, whatToHit);
            if (distCheck.collider != null) {
                HitTarget(transform.position, distCheck.collider);
                return;
            }

            //Last check is simplest check
            Vector3 dir = _targetPos - transform.position;
            float distanceThisFrame = moveSpeed * Time.deltaTime;
            //length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                //Make sure the player didn't dodge out of the way
                distCheck = Physics2D.Raycast(transform.position, _targetPos - transform.position, 0.2f, whatToHit);
                if (distCheck.collider != null) {
                    HitTarget(transform.position, distCheck.collider);
                    return;
                }
            }
        }
        //move as a constant speed
        transform.Translate(_targetDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }

    //Once the bullet leaves the Cameras viewport destroy it
    void OnBecameInvisible() {
        Destroy(gameObject);
    }

    public void SetLayerMask(LayerMask parentLayerMask) {
        whatToHit = parentLayerMask;
    }

    //Tell the update statement wwhere to move the bullet
    public void Fire(Vector3 targetPos, Vector3 targetNormal) {
        _targetPos = targetPos;
        _targetNormal = targetNormal;
        _targetDirection = _targetPos - transform.position;
    }

    //Destroy and generate effects
    public void HitTarget(Vector3 hitPos, Collider2D hitObject) {
        //Damage whoever we hit
        switch (hitObject.gameObject.tag) {
            case "Player":
                Debug.Log("We hit " + hitObject.name + " and did " + damage + " damage");
                Player player = hitObject.GetComponent<Player>();
                if(player != null) {
                    player.DamageHealth(damage);
                }
                break;
            case "BADDIE":
                Debug.Log("We hit " + hitObject.name + " and did " + damage + " damage");
                Baddie baddie = hitObject.GetComponent<Baddie>();
                if (baddie != null) {
                    baddie.DamageHealth(damage);
                }
                break;
        }

        //mask it so when we hit something the particles shoot OUT from it.
        Transform hitParticles = Instantiate(hitPrefab, hitPos, Quaternion.FromToRotation(Vector3.up, _targetNormal)) as Transform;
        //Destroy hit particles
        Destroy(hitParticles.gameObject, 1f);
        Destroy(gameObject);
    }
}
