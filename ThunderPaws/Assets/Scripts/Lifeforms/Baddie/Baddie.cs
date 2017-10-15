using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaddieAIController))]
public class Baddie : LifeformBase {
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
    /// Visual status indicator displaying health
    /// </summary>
    [SerializeField]
    private StatusIndicator _statusIndicator;

    /// <summary>
    /// Animator reference for sprite animations
    /// </summary>
    private Animator Animator;

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
    private float _maxWanderdistance = 3f;
    /// <summary>
    /// Everytime we start wandering in a direction we want to store where we started wandering from so we know when we've wandered far enough
    /// </summary>
    private Vector2 _wanderStart;

    /// <summary>
    /// Set by the AI controller if we get into the PERSONAL_SPACE zone, the AI will tell us which way to create space
    /// </summary>
    public bool TargetOnLeft;

    /// <summary>
    /// State of the object.
    /// Determines how we move.
    /// </summary>
    public MentalStateEnum State;

    private void Start() {
        //Set all physics values
        InitializePhysicsValues(6f, 4f, 0.4f, 0.2f, 0.1f);

        //Confirm stats component and initialize
        _stats = transform.GetComponent<BaddieStats>();
        if (_stats == null) {
            Debug.LogError("No BaddieStats found on Baddie");
            throw new MissingComponentException();
        }
        _stats.Initialize();


        //Get the animator
        Animator = GetComponent<Animator>();
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
        HealthDrop.GetComponent<SpriteRenderer>().sprite = PickupableSprites.Sprites[PickupableEnum.HEALTH];
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        CalculateVelocityOffState();
        ApplyGravity();
        Animate();
        Controller.Move(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// Based on the state of the entity calculate what the movement should be
    /// </summary>
    private void CalculateVelocityOffState() {
        if (State == MentalStateEnum.NEUTRAL) {
            CalculateWanderVelocity();
        } else if (State == MentalStateEnum.NOTICE) {
            CalculateNoticeVelocity();
        } else if (State == MentalStateEnum.ATTACK) {
            CalculateJumpContinuouslyVelocity();
        }else if (State == MentalStateEnum.PURSUE_ATTACK) {
            CalculatePursueAttackVelocity();
        }else if(State == MentalStateEnum.PERSONAL_SPACE) {
            CalculateCreateSpaceVelocity();
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
            GameMaster.KillBaddie(this);
        } else {
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

    protected override void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }

    /// <summary>
    /// Given a hardcoded max distance to wander from any direction change or begin origin just move in that direction till the threshold is met
    /// </summary>
    private void CalculateWanderVelocity() {
        //Indicates this is the initial wandering so choose an arbitrary direction  (right)
        if(_wanderStart == null) {
            _wanderStart = transform.position;
            _previousInput = Vector2.right;
        }
        var dist = Vector2.Distance(transform.position, _wanderStart);
        //Oscilate back and forth betwen +- maxWanderDist
        if (dist >= _maxWanderdistance) {
            _wanderStart = transform.position;
            if(_previousInput == Vector2.right) {
                _previousInput = Vector2.left;
            }else {
                _previousInput = Vector2.right;
            }
        }
        float targetVelocityX = _previousInput.x * MoveSpeed;
         Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Right now stop moving and track the target
    /// </summary>
    private void CalculateNoticeVelocity() {
        //This happens if we're currently not pursuing the target
        Velocity.x = 0;
    } 

    /// <summary>
    /// Helper method at the moment to force a jump just used to add a little diversity in when attacking
    /// </summary>
    private void CalculateJumpContinuouslyVelocity() {
        Velocity.x = 0;
        //Basic randomization
        Vector2 inputJump = new Vector2(0f, UnityEngine.Random.Range(-1f, 1f));
        if (inputJump.y > 0 && Controller.Collisions.FromBelow) {
            Velocity.y = JumpVelocity;
        }
    }

    /// <summary>
    /// Very basic "seek the target"
    /// Move in the direction of the target to pursue
    /// </summary>
    private void CalculatePursueAttackVelocity() {
        float targetVelocityX = MoveSpeed * (TargetOnLeft ? -1 : 1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Space is created by moving in the opposite direction from where we're facing
    /// </summary>
    private void CalculateCreateSpaceVelocity() {
        float targetVelocityX = MoveSpeed * (TargetOnLeft ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Animate the sprite
    /// </summary>
    private void Animate() {
        //Multiply by input so animation plays only when input is supplied instead of all the time because its a moving platform
        Animator.SetFloat("Speed", Mathf.Abs(Velocity.x));
    }

    public override void ApplyPickup(PickupableEnum pickupType) {
        throw new NotImplementedException();
    }


}
