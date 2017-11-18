using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour {
    /// <summary>
    /// Pickup type
    /// </summary>
    public PickupableEnum Pickup;
    /// <summary>
    /// This is the object we want to be able to pick us up
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Layermask to indicate what we can hit
    /// Right now its hardcoded to Player
    /// </summary>
    public LayerMask WhatToHit;
    /// <summary>
    /// The name of the tag of the object we want to be picked up by
    /// </summary>
    public string TargetName;
    /// <summary>
    /// Indicates that the pickupable has landed on some OBSTACLE surface
    /// </summary>
    private bool _hasLanded = false;
    /// <summary>
    /// Used for applying gravity in case it was spawned in the air
    /// </summary>
    private Vector2 _velocity = Vector2.zero;
    /// <summary>
    /// Gravity only used to make pickupoables drop to the ground
    /// </summary>
    private float _gravity = 50f;
    /// <summary>
    /// Just used as a reference for the smoothDamp function
    /// </summary>
    private float _currentVelocity;

    private void Start() {
        //Placeholder right now till I figure out how I really want to do pickups
        if(Target == null) {
            FindTarget(TargetName);
        }
    }

    private void Update() {
        if (Target == null) {
            FindTarget(TargetName);
        } else {
            //Raycast to check if we could potentially hit the target
            RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, Target.position - transform.position);
            if (possibleHit.collider != null) {
                //TODO: change the distance of the ray we draw to be relative to the pickup size
                var playerLayer = 1 << 8;
                RaycastHit2D distCheck = Physics2D.Raycast(transform.position, Target.position - transform.position, 0.75f, playerLayer);
                if (distCheck.collider != null) {
                    ApplyPickup(distCheck.collider);
                }
            }
        }
        if (!_hasLanded) {
            var newVelocity = ApplyGravity();
            var fallSpeed = Mathf.SmoothDamp(_velocity.y, newVelocity, ref _currentVelocity, 0.25f);
            transform.Translate(Vector2.down * fallSpeed);
            var obstacleLayer = 1 << 10;
            float rayLength = Mathf.Abs(newVelocity);
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, obstacleLayer);
            if (distCheck.collider != null) {
                _hasLanded = true;
                _velocity.y = 0;
                return;
            }
        }
    }

    private float ApplyGravity() {
        return (_velocity.y + _gravity) * Mathf.Min(Time.deltaTime, 0.02f);
    }

    public void ApplyPickup(Collider2D hitObject) {
        //Apply pickup to whoever we hit
        switch (hitObject.gameObject.tag) {
            case "Player":
                Player player = hitObject.GetComponent<Player>();
                if (player != null) {
                    player.ApplyPickup(Pickup);
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

    protected void FindTarget(string target) {
        GameObject searchResult = GameObject.FindGameObjectWithTag(target);
        if (searchResult != null) {
            Target = searchResult.transform;
        }
    }
}
