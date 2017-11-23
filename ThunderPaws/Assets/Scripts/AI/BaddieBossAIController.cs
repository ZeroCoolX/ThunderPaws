using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBossAIController : BaddieBaseAIController {
    /// <summary>
    /// Reference to the correct type of this AI
    /// </summary>
    public BaddieBoss BaddieBoss;

    // Use this for initialization
    void Start () {
        InitializeAIValues("Bazooka_Baddie");
        if (Lifeform.GetType() == typeof(BaddieBoss)) {
            BaddieBoss = Lifeform as BaddieBoss;
        } else {
            throw new InvalidCastException("Expected typeof BaddieBoss but found typeof " + Lifeform.GetType());
        }
        //Search for the target in game if there isn't one set - meaning he died and is respawning
        if (Target == null) {
            //Player might be dead so search, but only if we should search
            if (!_searchingForPlayer) {
                _searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }
        if (Vector2.Distance(Target.transform.position, transform.position) <= 50) {
            _baddieWeapon.ShouldShoot = true;
        }
    }

    // Update is called once per frame
    void Update () {
        if (Target == null) {
            //Player might be dead so search
            if (!_searchingForPlayer) {//there is no target so search
                _searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }
        LockOnTarget();
        if (Vector2.Distance(Target.transform.position, transform.position) <= 50) {
            _baddieWeapon.ShouldShoot = true;
        }
    }


    /// <summary>
    /// Locate the player in the world
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator SearchForPlayer() {
        //Search for the player
        GameObject searchResult = GameObject.FindGameObjectWithTag(_targetTag);
        if (searchResult == null) {//search only twice a second until found
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        } else {
            //Found the player - set it as the target and stop searching and reinvoke the state updating
            Target = searchResult.transform;
            _baddieWeapon.AttackTarget = Target;
            _searchingForPlayer = false;
            yield break;
        }
    }

}
