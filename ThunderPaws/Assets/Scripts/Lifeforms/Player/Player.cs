using System;
using System.Collections;
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
    /// Animator reference for sprite animations
    /// </summary>
    private Animator Animator;

    /// <summary>
    /// Amount to shake the camera by
    /// </summary>
    public float ShakeAmount = 0.05f;
    /// <summary>
    /// How long to shake the camera
    /// </summary>
    public float ShakeLength = 0.1f;
    /// <summary>
    /// List of owned weapons that have either been picked up, purchased, or default
    /// </summary>
    private List<WeaponEnum> _ownedWeapons;

    /// <summary>
    /// Weapons indicated by ints 1, 2...etc
    /// </summary>
    [Header("Weapons")]
    [SerializeField]
    private GameObject _machineGun;
    [SerializeField]
    private GameObject _pistol;
    [SerializeField]
    private GameObject _bazooka;

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
    /// Reference to the companion anchor.
    /// When the sprite flips direction so does the position of where thecompanion floats
    /// </summary>
    public Transform CompanionAnchor;
    /// <summary>
    /// Value used to move the companion anchor when the player flips its body.
    /// </summary>
    private float _companionFlipOffset;
    /// <summary>
    /// Companion reference 
    /// </summary>
    public Transform Companion;
    /// <summary>
    /// Indicates player is facing right
    /// </summary>
    private bool _facingRight = true;
    /// <summary>
    /// Indicates the arm sprite is right side up
    /// </summary>
    private bool _rightSideUp = true;

    /// <summary>
    /// How far the world extends downwards until we kill and respawn the player
    /// </summary>
    public float WorldEdge = -110f;

    /// <summary>
    /// Input supplied from user input
    /// </summary>
    public Vector2 DirectionalInput;
    /// <summary>
    /// Indicates the player shot down and hit the trigger and if they jump within 0.25seconds of hitting the trigger they will receive a massive jump boost
    /// </summary>
    private bool _rocketJumpInitiated = false;
    /// <summary>
    /// Adds to jump velocity
    /// </summary>
    private float _rocketJumpBoost = 5f;

    /// <summary>
    /// Setup Player object.
    /// Initialize physics values, stats, particles, weapons, and invoke constant health regeneration
    /// </summary>
    void Start() {
        //Validate graphics sprite and Arm are set
        PlayerGraphics = transform.FindChild("Graphics");
        if (PlayerGraphics == null) {
            Debug.LogError("No Graphics on player found");
            throw new UnassignedReferenceException();
        }
        PlayerArm = transform.FindChild("arm");
        if (PlayerArm == null) {
            Debug.LogError("No Player Arm on player found");
            throw new UnassignedReferenceException();
        }
        Animator = GetComponent<Animator>();
        if(Animator == null) {
            Debug.LogError("No Animator on player found");
            throw new MissingComponentException();
        }
        CompanionAnchor = transform.FindChild("CompanionOrigin");
        if(CompanionAnchor == null) {
            //This might be okay because they won't always have a companion
            Debug.Log("CompanionOrigin was null");
        }else {
            CalculateCompanionFlipOffset();
        }

        //Set all physics values
        InitializePhysicsValues(8f, 4f, 1f, 0.4f, 0.2f, 0.1f);

        //Set the PlayerStats singleton and initialize
        _stats = PlayerStats.Instance;
        _stats.CurHealth = _stats.MaxHealth;

        //Validate StatusIndicator
        if(_statusIndicator == null) {
            Debug.LogError("No status indicator found");
            throw new UnassignedReferenceException();
        }
        _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);

        //Validate DeathParticles
        if(DeathParticles == null) {
            Debug.LogError("No player death particles found");
            throw new UnassignedReferenceException();
        }

        //Add default starting weapon to the collection of owned weapons
        _ownedWeapons = new List<WeaponEnum>();
        _ownedWeapons.Add(WeaponEnum.PISTOL);
        //Set default weapon (Pistol) and Add weapon switching logic to GameMaster delegate
        SelectWeapon(GameMaster.Instance.WeaponChoice);
        //Add the weapon switch method onto the weaponSwitch delegate
        GameMaster.Instance.OnWeaponSwitch += SelectWeapon;

        //Regenerate health over time
        //TODO: Only invoke regen if health < max
        InvokeRepeating("RegenHealth", 1f / _stats.HealthRegenRate, 1f / _stats.HealthRegenRate);
    }

    /// <summary>
    /// Check for falling off the map.
    /// Perhaps should be extracted to the parent LifeFormBase
    /// </summary>
    private void FixedUpdate() {
        FallOffMapCheck();
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        CalculateVelocityOffInput();
        ApplyGravity();
        Controller.Move(Velocity * Time.deltaTime, DirectionalInput);
        CalculatePlayerFacing();
    }

    /// <summary>
    /// Tells the companion to flip origin or not
    /// </summary>
    private void CalculateCompanionFlipOffset() {
        _companionFlipOffset = Vector3.Distance(CompanionAnchor.position, transform.position);
    }

    /// <summary>
    /// Store the player input 
    /// </summary>
    /// <param name="input"></param>
    public void SetDirectionalInput(Vector2 input) {
        DirectionalInput = input;
    }

    /// <summary>
    /// Get the input from either the user 
    /// </summary>
    private void CalculateVelocityOffInput() {
        //check if user - or NPC - is trying to jump and is standing on the ground
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && Controller.Collisions.FromBelow) {
            //If they're trying to rocket jump...LET THEM
            Velocity.y = MaxJumpVelocity + (_rocketJumpInitiated ? _rocketJumpBoost : 0f);
        }
        float targetVelocityX = DirectionalInput.x * MoveSpeed;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Toggle active status of weapon objects on the player based off choice
    /// </summary>
    /// <param name="choice"></param>
    private void SelectWeapon(WeaponEnum choice) {
        //Only switch to the desired weapon if the player owns it
        if (_ownedWeapons.Contains(choice)) {
            if (_machineGun != null && _pistol != null && _bazooka != null) {
                _machineGun.SetActive(false);
                _pistol.SetActive(false);
                _bazooka.SetActive(false);
                switch (choice) {
                    case WeaponEnum.PISTOL:
                        _pistol.SetActive(true);
                        break;
                    case WeaponEnum.MACHINE_GUN:
                        _machineGun.SetActive(true);
                        break;
                    case WeaponEnum.BAZOOKA:
                        _bazooka.SetActive(true);
                        break;
                    default:
                        _pistol.SetActive(true);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Check health and kill if necessary
    /// </summary>
    private void LifeCheck() {
        if(_stats.CurHealth <= 0) {
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
        _stats.CurHealth -= dmg;
        if(_statusIndicator != null) {
            _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
        }
        LifeCheck();
    } 

    public void MakeWalletTransaction(int amount) {
        _stats.NipAmount += amount;
        GameMaster.Instance.NipAccumulated = _stats.NipAmount;
    }

    /// <summary>
    /// Increment health by a small amount and update visual healthbar
    /// </summary>
    private void RegenHealth() {
        _stats.CurHealth += _stats.HealthRegenValue;
        _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
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
                InvertArm();
            }
        } else if ((rotZ > 90f) && _facingRight) {
            //Face left
            Flip();
            if (_rightSideUp) {
                InvertArm();
            }
        }
        //Multiply by input so animation plays only when input is supplied instead of all the time because its a moving platform
        Animator.SetFloat("Speed", Mathf.Max(Mathf.Abs(Velocity.x), Mathf.Abs(Velocity.y)) * (DirectionalInput.Equals(Vector2.zero) ? 0 : 1));
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
        FlipCompanionAnchorDelayed();
    }

    /// <summary>
    /// Ensures the companion anchor is always behind the player
    /// </summary>
    private void FlipCompanionAnchorDelayed() {
        Vector3 newPos = CompanionAnchor.position;
        float newX = newPos.x + (2 * _companionFlipOffset * (_facingRight ? -1f : 1f));
        newPos.x = newX;
        CompanionAnchor.position = newPos;
    }

    /// <summary>
    /// Invert the arm graphics so its always right side up no matter what angle its facing
    /// </summary>
    private void InvertArm() {
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

    /// <summary>
    /// Overriden method to apply gravity ourselves
    /// </summary>
    protected override void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }

    /// <summary>
    /// Based off the pickup type: do the expected action
    /// </summary>
    /// <param name="pickupType"></param>
    public override void ApplyPickup(PickupableEnum pickupType) {
        switch (pickupType) {
            case PickupableEnum.HEALTH:
                _stats.CurHealth = _stats.MaxHealth;
                _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
                break;
            case PickupableEnum.CURRENCY:
                MakeWalletTransaction(50);
                break;
            case PickupableEnum.MACHINE_GUN:
                _ownedWeapons.Add(WeaponEnum.MACHINE_GUN);
                break;
            case PickupableEnum.BAZOOKA:
                _ownedWeapons.Add(WeaponEnum.BAZOOKA);
                break;
            case PickupableEnum.COMPANION:
                CreateCompanion();
                break;
        }
    }

    /// <summary>
    /// Create a Companion and set all the necessary properties
    /// </summary>
    private void CreateCompanion() {
        var companionOrigin = transform.FindChild("CompanionOrigin").transform;
        var companion = Instantiate(CompanionMap.Companions[CompanionEnum.BASE], companionOrigin.position, transform.rotation);
        companion.GetComponent<CompanionBase>().Leader = transform;
        companion.GetComponent<CompanionFollow>().Target = companionOrigin;
    }

    /// <summary>
    /// Helper method that handles variable jump height
    /// </summary>
    public void OnJumpInputUp() {
        if (Velocity.y > MinJumpVelocity) {
            Velocity.y = MinJumpVelocity + (_rocketJumpInitiated ? _rocketJumpBoost : 0f);
        }
    }

    /// <summary>
    /// Rocket jumping shouldnt be easy
    /// Only 1/4 of a second interval to rocket jump
    /// </summary>
    public void AllowRocketJump() {
        _rocketJumpInitiated = true;
        Invoke("ResetRocketJump", 0.25f);
    }

    /// <summary>
    /// Reset after rocket jumped
    /// </summary>
    public void ResetRocketJump() {
        _rocketJumpInitiated = false;
    }

    /// <summary>
    /// Once the player leaves the Cameras viewport it most likely (I haven't hit a scenario where this wasn't the case)
    /// because they fell off the map. So kill them and take a life
    /// </summary>
    public void FallOffMapCheck() {
        if (transform.position.y <= WorldEdge) {
            print("kill the player because he fell off");
            DamageHealth(_stats.MaxHealth);
        }
    }
}
