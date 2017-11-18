using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTurretAIController : MonoBehaviour {
    /// <summary>
    /// Target transform if needed
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Tag to search for if searching
    /// </summary>
    protected string _targetTag = "Player";
    /// <summary>
    /// Reference to the weapon
    /// </summary>
    private BaddieWeapon _baddieWeapon;

    // Use this for initialization
    void Start () {
        _baddieWeapon = GetComponent<BaddieWeapon>();
        if (_baddieWeapon == null) {
            throw new MissingReferenceException();
        }
        //Specify that we want to wait 2 seconds till the homing missile starts...homing
        _baddieWeapon.FreeFlyTime = 1;

    }
	
	// Update is called once per frame
	void Update () {
        if (Target == null) {
            StartCoroutine(SearchForPlayer());
            return;
        } else {
            //Super simple - change later
            _baddieWeapon.ShouldShoot = (Vector2.Distance(Target.position, transform.position) <= 10);
        }
    }

    private IEnumerator SearchForPlayer() {
        //Search for the player
        GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
        if (searchResult == null) {//search only twice a second until found
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        } else {
            //Found the player - set it as the target and stop searching and reinvoke the state updating
            Target = searchResult.transform;
            _baddieWeapon.AttackTarget = Target;
           // _searchingForPlayer = false;
            yield break;
        }
    }
}
