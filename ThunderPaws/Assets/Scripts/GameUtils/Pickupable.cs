using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour {

    private PickupableEnum _pickup = PickupableEnum.HEALTH;
    public Transform Target;
    public LayerMask WhatToHit;

    private void Start() {
        //Placeholder right now till I figure out how I really want to do pickups
        if(Target == null) {
            throw new MissingReferenceException();
        }
    }

    private void Update() {
        //Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, Target.position - transform.position);
        if (possibleHit.collider != null) {
            //TODO: change the distance of the ray we draw to be relative to the pickup size
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, Target.position - transform.position, 0.75f, WhatToHit);
            if (distCheck.collider != null) {
                ApplyPickup(distCheck.collider);
                return;
            }
        }
    }

    public void ApplyPickup(Collider2D hitObject) {
        //Apply pickup to whoever we hit
        switch (hitObject.gameObject.tag) {
            case "Player":
                Player player = hitObject.GetComponent<Player>();
                if (player != null) {
                    player.ApplyPickup(_pickup);
                }
                break;
        }

        //TODO: Add particles to pickup
        //Mask it so when we hit something the particles shoot OUT from it.
        //Transform hitParticles = Instantiate(HitPrefab, hitPos, Quaternion.FromToRotation(Vector3.up, _targetNormal)) as Transform;
        ////Destroy hit particles
        //Destroy(hitParticles.gameObject, 1f);
        Destroy(gameObject);
    }
}
