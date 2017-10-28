using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBoss : LifeformBase {
    /// <summary>
    /// Baddie Stats reference
    /// </summary>
    private BaddieStats _stats;
    /// <summary>
    /// Particle reference to play on destruct
    /// </summary>
    public Transform DeathParticles;
    /// <summary>
    /// Health object that drops when baddie dies
    /// </summary>
    public Transform HealthDrop;
    /// <summary>
    /// Amount of health to drop on death
    /// </summary>
    public int HealthDropAmount = 10;

    /// <summary>
    /// Layermask to indicate what object this needs to dodge
    /// </summary>
    public LayerMask DodgeLayerMask;
    /// <summary>
    /// Maximum distance object this needs to dodge can be away before we dodge
    /// </summary>
    private float _maxDodgeRadius = 5f;
    /// <summary>
    /// How often to check if we can dodge or not
    /// </summary>
    private float _timeToDodge;

    /// <summary>
    /// Visual status indicator displaying health
    /// </summary>
    [SerializeField]
    private StatusIndicator _statusIndicator;

    /// <summary>
    /// Animator reference for sprite animations
    /// </summary>
    private Animator Animator;

    /// <summary>
    /// Reference to the graphics component so we can rotate it when wandering.
    /// When attacking and all other states the AI controller handles that but no when wandering
    /// </summary>
    private Transform _graphics;
    /// <summary>
    /// Need a reference because I set this active or not based off wandering
    /// </summary>
    private Transform _armGraphics;
    /// <summary>
    /// Don't even ask
    /// Its an uphill batle rotating that arm whihc isn't apart of the graphics when wandering so this does it for me
    /// </summary>
    private Transform _wanderGraphics;

    /// <summary>
    /// Amount to shake camera by
    /// </summary>
    public float ShakeAmount = 0.05f;
    /// <summary>
    /// How long to shake camera for
    /// </summary>
    public float ShakeLength = 0.1f;

    /// <summary>
    ///  Stores the generated input until it changes allowing us to know which way we came from so go the opposite direction. 
    /// </summary>
    private Vector2 _previousInput;
    /// <summary>
    /// Farthest this object can wander.
    /// Its updated randomly with a random value within a range
    /// </summary>
    private float _maxWanderDistance = 10f;
    /// <summary>
    /// Everytime we start wandering in a direction we want to store where we started wandering from so we know when we've wandered far enough
    /// </summary>
    private Vector2 _wanderStart;
    /// <summary>
    /// Used when we collided with something before our wander threshold
    /// Must reset where we are wandering so the object doesn't get stuck in a corner forever
    /// </summary>
    private bool _recalculatingWander = false;

    /// <summary>
    /// Much much slower when we're wandering
    /// </summary>
    private float _wanderMoveSpeed = 3f;
    /// <summary>
    /// Pursue faster than normal speed by still a little slower than player
    /// </summary>
    private float _pursueMovespeed = 7f;

    /// <summary>
    /// Allows for attack strafing to be done at an interval instead of every frame so it looks more natural
    /// </summary>
    private float _timeToStrafe = 0f;
    /// <summary>
    /// The rate at which we should strafe
    /// </summary>
    public float StrafeRate;
    /// <summary>
    /// Need to keep a reference in case we're currently moving in a strafe direction we have to update the speed based off the last known strafe velocity
    /// </summary>
    private float _currentStrafeVelocity;

    /// <summary>
    /// Set by the AI controller if we get into the PERSONAL_SPACE zone, the AI will tell us which way to create space
    /// </summary>
    public bool TargetOnLeft;

    public Transform Target { get; set; }

    /// <summary>
    /// State of the object.
    /// Determines how we move.
    /// </summary>
    public MentalStateEnum State;

    private void Start() {
        //Set all physics values
        InitializePhysicsValues(6f, 4f, 4f, 0.4f, 0.2f, 0.1f, 0f);

        //Confirm stats component and initialize
        _stats = transform.GetComponent<BaddieStats>();
        if (_stats == null) {
            Debug.LogError("No BaddieStats found on Baddie");
            throw new MissingComponentException();
        }
        _stats.Initialize();


        //Get the animator
        Animator = GetComponent<Animator>();//TODO: don't need this at the moment, but we will in the future when I make a specific animator for the boss
        if (Animator == null) {
            Debug.LogError("No Animator on player found");
            throw new MissingComponentException();
        }

        //Validate Status indicator
        if (_statusIndicator != null) {
            _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
        }
        //Validate Death particles are set
        if (DeathParticles == null) {
            Debug.LogError("No death particles found");
            throw new UnassignedReferenceException();
        }
        //Set the sprite renderer we need for our health drop because it is not set at compile time
        HealthDrop.GetComponent<SpriteRenderer>().sprite = PickupableSpriteMap.Sprites[PickupableEnum.HEALTH];
        HealthDrop.GetComponent<Pickupable>().Pickup = PickupableEnum.HEALTH;

        _graphics = transform.FindChild("Graphics");
        if (_graphics == null) {
            throw new MissingMemberException();
        } else {
            _wanderGraphics = _graphics.FindChild("BaddieWanderArm");
            if (_wanderGraphics == null) {
                throw new MissingMemberException();
            }
        }
        _armGraphics = transform.FindChild("arm");
        if (_armGraphics == null) {
            throw new MissingMemberException();
        }
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        //if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
        //    Velocity.y = 0;
        //}
        //CalculateVelocity();//This is going to be different than state since its a boss battle, the boss is never not going to be attacking...
       // Controller.Move(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// Calculate how the boss should be moving
    /// </summary>
    private void CalculateVelocity() {
        //has an "idle" while not currently moving
        //Moves to different points in the world
        //Shoots on some sort of interval - atm
        //Calculate random strafing
        //Based off random value - change to go right or left 
        //Only strafe on an interval instead of every frame. Makes it look more realistic
        if (Time.time > _timeToStrafe) {
            var rand = UnityEngine.Random.Range(-10, 10f) * Time.deltaTime;

            Vector2 strafeVelocity = Vector2.right * Mathf.Sign(rand);
            _timeToStrafe = Time.time + 3 / StrafeRate;

            _currentStrafeVelocity = strafeVelocity.x * MoveSpeed;
            Velocity.x = Mathf.SmoothDamp(Velocity.x, _currentStrafeVelocity, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        } else {
            Velocity.x = Mathf.SmoothDamp(Velocity.x, _currentStrafeVelocity, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        }

        //Check if any nearby bullets
        //chance to jump out of the way
        if (BulletWithinRange() && Time.time > _timeToDodge) {
            //If can jump out of the way via time do it
            _timeToDodge = Time.time + 5;
            //CalculateJumpVelocity();
        }
    }

    /// <summary>
    /// Check the stats to see if we need to destroy the object
    /// </summary>
    private void LifeCheck() {
        //Kill the baddie
        if (_stats.CurHealth <= 0) {
            //Drop Health
            var pickupable = Instantiate(HealthDrop, transform.position, transform.rotation) as Transform;
            pickupable.GetComponent<Pickupable>().TargetName = "Player";
            Destroy(gameObject);
            //GameMaster.KillBaddie(this);//TODO: should take in lifeform
            //TODO: Add audio
        }
    }

    /// <summary>
    /// Decrement health and check for life
    /// </summary>
    /// <param name="dmg"></param>
    public void DamageHealth(int dmg) {
        //Damage baddie and check vitals
        _stats.CurHealth -= dmg;
        if (_statusIndicator != null) {
            _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
        }
        LifeCheck();
    }

    /// <summary>
    /// BOSS does not need gravity.
    /// </summary>
    protected override void ApplyGravity() {
        throw new NotImplementedException();
      // Velocity.y += Gravity * Time.deltaTime;
    }

    private bool BulletWithinRange() {
        var hitColliders = Physics2D.OverlapCircleAll(transform.position, _maxDodgeRadius, DodgeLayerMask);
        float currentClosest = Mathf.Infinity;
        Transform closestTarget = null;
        foreach (var hit in hitColliders) {
            if (Vector2.Distance(transform.position, hit.transform.position) < currentClosest) {
                closestTarget = hit.transform;
            }
        }
        return closestTarget != null;
    }


    /// <summary>
    /// Space is created by moving in the opposite direction from where we're facing
    /// </summary>
    private void CalculateCreateSpaceVelocity() {
        float targetVelocityX = MoveSpeed * (TargetOnLeft ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Animate the sprite - not used atm
    /// </summary>
    private void Animate() {
        //Multiply by input so animation plays only when input is supplied instead of all the time because its a moving platform
        Animator.SetFloat("Speed", Mathf.Abs(Velocity.x));
    }

    /// <summary>
    /// Not used at the moment
    /// </summary>
    /// <param name="pickupType"></param>
    public override void ApplyPickup(PickupableEnum pickupType) {
        throw new NotImplementedException();
    }
}
