using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBossAIController : MonoBehaviour {

    /// <summary>
    /// Reference to the object the AI is effecting
    /// </summary>
    public BaddieBoss BaddieBoss;
    /// <summary>
    /// Target transform if needed
    /// </summary>
    public Transform Target;
    /// <summary>
    /// Tag to search for if searching
    /// </summary>
    private string _targetTag = "Player";
    /// <summary>
    /// Reference to graphics for rotatipn/flipping
    /// </summary>
    public Transform BaddieGraphics;
    /// <summary>
    /// Axis around which we rotate the arm
    /// </summary>
    public Transform ArmRotationAxis;
    /// <summary>
    /// Indicates the arm is facing left
    /// </summary>
    private bool _armIsLeft = false;

    /// <summary>
    /// AI needs a reference to its weapon so it can control when its firing
    /// </summary>
    private BaddieWeapon _baddieWeapon;

    //TODO: Find a beter way to do this
    /// <summary>
    /// Determines if we should search for the player or not
    /// </summary>
    private bool _searchingForPlayer = false;

    // Use this for initialization
    void Start () {
        BaddieGraphics = transform.FindChild("Graphics");
        if (BaddieGraphics == null) {
            //couldn't find player graphics 
            Debug.LogError("Cannot find Graphics on baddie");
            throw new MissingReferenceException();
        }
        //Need a reference to the weapon attached to the object so we can tell it when and what to attack
        var weaponTransform = ArmRotationAxis.FindChild("Bazooka_Baddie");
        if (weaponTransform != null) {
            _baddieWeapon = weaponTransform.GetComponent<BaddieWeapon>();
            _baddieWeapon.AttackTarget = Target;
        }else {
            throw new MissingComponentException();
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
        _baddieWeapon.ShouldShoot = true;
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
       // _baddieWeapon.ShouldShoot = true;
    }


    /// <summary>
    /// Locate the player in the world
    /// </summary>
    /// <returns></returns>
    IEnumerator SearchForPlayer() {
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

    /// <summary>
    /// Face the target and track with where the arm is pointing
    /// </summary>
    private void LockOnTarget() {
        //rotate left or right to face target
        BaddieGraphics.rotation = Quaternion.Euler(0f, IsFacingLeft() ? -180f : 360f, 0f);

        Vector3 diff = Target.position - ArmRotationAxis.position;
        //Normalize the vector x + y + z = 1
        diff.Normalize();
        //find the angle in degrees
        float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //apply the rotation
        ArmRotationAxis.rotation = Quaternion.Euler(0f, 0f, rotZ);//degrees not radians

        //invert the arm only iff necessary
        if (IsFacingLeft() && !_armIsLeft) {
            InvertArm();
        } else if (_armIsLeft && !IsFacingLeft()) {
            InvertArm();
        }
    }

    /// <summary>
    /// Invert the arm when necessary so its never upside down
    /// Allows 360 degree motion
    /// </summary>
    private void InvertArm() {
        //switch the way the arm is labeled as facing
        _armIsLeft = !_armIsLeft;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = ArmRotationAxis.localScale;
        theScale.y *= -1;
        ArmRotationAxis.localScale = theScale;

        //Also deal with the arm rotation axis offset since the graphics, arm, and colliders are all seperate.
        //This 0.3 offset is because the pivot point on the graphics is dead center, but the arm is at the shoulder for a natural arm movement.
        //The offset allows the arm to stay in place when left or right. Otherwise it jutts out when facing left because its flipping scale based on the rotational axis
        if (theScale.y < 0f) {
            theScale = ArmRotationAxis.transform.localPosition;
            theScale.x += 0.3f;
        } else {
            theScale = ArmRotationAxis.transform.localPosition;
            theScale.x -= 0.3f;
        }
        ArmRotationAxis.transform.localPosition = theScale;
    }

    /// <summary>
    /// Helper function to tell us what direction we're facing relative to the target
    /// result  less than or equal to 0 indicates facing LEFT
    /// </summary>
    private bool IsFacingLeft() {
        return (Target.position.x - transform.position.x) <= 0;
    }
}
