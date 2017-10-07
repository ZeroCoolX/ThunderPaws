﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(CollisionController2D))]
public class Player : LifeformBase {
    /// <summary>
    /// Statistics object containing health, currency...etc
    /// </summary>
    private PlayerStats _stats;
    /// <summary>
    /// Status bar above object inficates health and other stats
    /// </summary>
    [SerializeField]
    private StatusIndicator _statusIndicator;

    /// <summary>
    /// Amount to shake the camera by
    /// </summary>
    public float ShakeAmount = 0.05f;
    /// <summary>
    /// How long to shake the camera
    /// </summary>
    public float ShakeLength = 0.1f;

    /// <summary>
    /// Weapons indicated by ints 1, 2...etc
    /// </summary>
    [Header("Weapons")]
    [SerializeField]
    private GameObject _machineGun;
    [SerializeField]
    private GameObject _pistol;

    /// <summary>
    /// Particles to emit upon death
    /// </summary>
    public Transform DeathParticles;
    /// <summary>
    /// Graphics of actual sprite on the player
    /// </summary>
    public Transform PlayerGraphics;
    /// <summary>
    /// Seperate object for the arm to allow full 360 degree motion and indicates which way the body should be facing
    /// </summary>
    public Transform PlayerArm;
    /// <summary>
    /// Indicates player is facing right
    /// </summary>
    private bool _facingRight = true;
    /// <summary>
    /// Indicates the arm sprite is right side up
    /// </summary>
    private bool _rightSideUp = true;

    /// <summary>
    /// Input supplied from user input
    /// </summary>
    public Vector2 DirectionalInput;

    /// <summary>
    /// Setup Player object.
    /// Initialize physics values, stats, particles, weapons, and invoke constant health regeneration
    /// </summary>
    void Start() {
        //Validate graphics sprite and Arm are set
        PlayerGraphics = transform.FindChild("Graphics");
        if (PlayerGraphics == null) {
            Debug.LogError("No Graphics on player found");
        }
        PlayerArm = transform.FindChild("arm");
        if (PlayerArm == null) {
            Debug.LogError("No Player Arm on player found");
        }

        //Set all physics values
        InitializePhysicsValues(6f, 4f, 0.4f, 0.2f, 0.1f);

        //Set the PlayerStats singleton and initialize
        _stats = PlayerStats.instance;
        _stats.curHealth = _stats.maxHealth;

        //Validate StatusIndicator
        if(_statusIndicator == null) {
            Debug.LogError("No status indicator found");
        }
        _statusIndicator.SetHealth(_stats.curHealth, _stats.maxHealth);

        //Validate DeathParticles
        if(DeathParticles == null) {
            Debug.LogError("No player death particles found");
        }

        //Set default weapon (Pistol) and Add weapon switching logic to GameMaster delegate
        SelectWeapon(GameMaster.instance.weaponChoice);
        //Add the weapon switch method onto the weaponSwitch delegate
        GameMaster.instance.OnWeaponSwitch += SelectWeapon;

        //Regenerate health over time
        //TODO: Only invoke regen if health < max
        InvokeRepeating("RegenHealth", 1f / _stats.healthRegenRate, 1f / _stats.healthRegenRate);
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        CalculateVelocityOffInput();
        ApplyGravity();
        Controller.Move(Velocity * Time.deltaTime);
        CalculatePlayerFacing();
    }

    public void SetDirectionalInput(Vector2 input) {
        DirectionalInput = input;
    }

    /// <summary>
    /// Get the input from either the user 
    /// </summary>
    private void CalculateVelocityOffInput() {
        //check if user - or NPC - is trying to jump and is standing on the ground
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && Controller.Collisions.FromBelow) {
            Velocity.y = JumpVelocity;
        }
        float targetVelocityX = DirectionalInput.x * MoveSpeed;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Toggle active status of weapon objects on the player based off choice
    /// </summary>
    /// <param name="choice"></param>
    private void SelectWeapon(int choice) {
        //TODO: only allow weapon switch if they have the weapon
        if(_machineGun != null && _pistol != null) {
            _machineGun.SetActive(false);
            _pistol.SetActive(false);
            switch (choice) {
                case 1:
                    _pistol.SetActive(true);
                    break;
                case 2:
                    _machineGun.SetActive(true);
                    break;
                default:
                    _pistol.SetActive(true);
                    break;
            }
        }
    }

    /// <summary>
    /// Check health and kill if necessary
    /// </summary>
    private void LifeCheck() {
        if(_stats.curHealth <= 0) {
            GameMaster.KillPlayer(this);
        }else {
            //TODO: audio
        }
    }

    /// <summary>
    /// Decrease player health by Damage amount
    /// Check for life after damage taken
    /// </summary>
    /// <param name="dmg"></param>
    public void DamageHealth(int dmg) {
        _stats.curHealth -= dmg;
        if(_statusIndicator != null) {
            _statusIndicator.SetHealth(_stats.curHealth, _stats.maxHealth);
        }
        LifeCheck();
    } 

    /// <summary>
    /// Increment health by a small amount and update visual healthbar
    /// </summary>
    private void RegenHealth() {
        _stats.curHealth += _stats.healthRegenValue;
        _statusIndicator.SetHealth(_stats.curHealth, _stats.maxHealth);
    }

    /// <summary>
    /// Calculate which way the player should be facing based off where the arm is pointing.
    /// Since the arm can rotate 360 degrees, once it passes the middle 90degree north or south threshold the sprite should be facing the opposite direction
    /// </summary>
    private void CalculatePlayerFacing() {
        //Difference between the mouse position and the player arm
        Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - PlayerArm.position;
        //Normalize the vector so we only have to check 90degrees as the threshold
        diff.Normalize();
        //Find the angle in degrees
        float rotZ = Mathf.Abs(Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
        if ((rotZ <= 90f) && !_facingRight) {
            //Face right
            Flip();
            if (!_rightSideUp) {
                invertArm();
            }
        } else if ((rotZ > 90f) && _facingRight) {
            //Face left
            Flip();
            if (_rightSideUp) {
                invertArm();
            }
        }
        //Multiply by input so animation plays only when input is supplied instead of all the time because its a moving platform
        //animator.SetFloat("Speed", Mathf.Max(Mathf.Abs(moveAmount.x), Mathf.Abs(moveAmount.y)) * (input.Equals(Vector2.zero) ? 0 : 1));
    }

    /// <summary>
    /// Mirror the player graphics by inverting the .x local scale value
    /// </summary>
    private void Flip() {
        // Switch the way the player is labelled as facing.
        _facingRight = !_facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = PlayerGraphics.localScale;
        theScale.x *= -1;
        PlayerGraphics.localScale = theScale;
    }

    /// <summary>
    /// Invert the arm graphics so its always right side up no matter what angle its facing
    /// </summary>
    private void invertArm() {
        //Switch the way the arm is labeled as facing
        _rightSideUp = !_rightSideUp;

        // Multiply the player's y local scale by -1.
        Vector3 theScale = PlayerArm.localScale;
        theScale.y *= -1;
        PlayerArm.localScale = theScale;

        //Also deal with the arm rotation axis offset since the graphics, arm, and colliders are all seperate.
        //This 0.3 offset is because the pivot point on the graphics is dead center, but the arm is at the shoulder for a natural arm movement.
        //The offset allows the arm to stay in place when left or right. Otherwise it jutts out when facing left because its flipping scale based on the rotational axis
        if (theScale.y < 0f) {
            theScale = PlayerArm.transform.localPosition;
            theScale.x += 0.3f;
        } else {
            theScale = PlayerArm.transform.localPosition;
            theScale.x -= 0.3f;
        }
        PlayerArm.transform.localPosition = theScale;
    }

}