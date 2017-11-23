using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Indicates when the player has made it to a checkpoint and updates which spawn point the player should respawn to
/// </summary>
public class CheckpointTrigger : MonoBehaviour {

    /// <summary>
    /// Who we want to detect colliding with the checkpoint
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Name of the target we should look for
    /// </summary>
    private string _targetName = "Player";

    /// <summary>
    /// What layer should we check for collisions on
    /// </summary>
    public LayerMask WhatToHit;

	// Use this for initialization
	void Start () {
        //Placeholder right now till I figure out how I really want to do pickups
        if (Target == null) {
            FindTarget(_targetName);
        }
    }

    private void Update() {
        if (Target == null) {
            FindTarget(_targetName);
        } else {
            //Raycast to check if we could potentially the target
            RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, Target.position - transform.position);
            if (possibleHit.collider != null) {
                RaycastHit2D distCheck = Physics2D.Raycast(transform.position, Target.position - transform.position, 3f, WhatToHit);
                if (distCheck.collider != null) {
                    ApplyCheckpoint(distCheck.collider);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Update the spawn index on the game master
    /// </summary>
    /// <param name="hitObject"></param>
    public void ApplyCheckpoint(Collider2D hitObject) {
        switch (hitObject.gameObject.tag) {
            case "Player":
                Player player = hitObject.GetComponent<Player>();
                if (player != null) {
                    print("apply checkpoint");
                    GameMaster.Instance.IncrementSpawnPoint();
                    Destroy(gameObject);
                }
                break;
        }
    }

    protected void FindTarget(string target) {
        GameObject searchResult = GameObject.FindGameObjectWithTag(target);
        if (searchResult != null) {
            Target = searchResult.transform;
        }
    }
}
