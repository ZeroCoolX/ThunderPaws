using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelComplete : MonoBehaviour {

    /// <summary>
    /// How we indicate the level is complete is if the Target collides with us
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Layermask to indicate what we can hit
    /// Right now its hardcoded to Player
    /// </summary>
    public LayerMask WhatToHit;
    /// <summary>
    /// The name of the tag of the object we want to check collisions with
    /// </summary>
    public string TargetName;

    private bool _completedLevel = false;

    private void Awake() {
        //Placeholder right now till I figure out how I really want to do pickups
        if (Target == null) {
            FindTarget(TargetName);
        }
    }

    private void Update() {
        if (!_completedLevel) {
            if (Target == null) {
                FindTarget(TargetName);
            }
            //Raycast to check if we could potentially the target
            RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, Target.position - transform.position);
            if (possibleHit.collider != null) {
                //TODO: change the distance of the ray we draw to be relative to the pickup size
                RaycastHit2D distCheck = Physics2D.Raycast(transform.position, Target.position - transform.position, 2f, WhatToHit);
                if (distCheck.collider != null) {
                    GameMaster.Instance.LevelComplete(Target.GetComponent<Player>());
                    _completedLevel = true;
                    return;
                }
            }
        }
    }

    protected void FindTarget(string target) {
        GameObject searchResult = GameObject.FindGameObjectWithTag(target);
        if (searchResult != null) {
            Target = searchResult.transform;
        }
    }
}
