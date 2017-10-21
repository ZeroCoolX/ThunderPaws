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
        InitializePhysicsValues(6f, 4f, 4f, 0.4f, 0.2f, 0.1f);

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
        HealthDrop.GetComponent<SpriteRenderer>().sprite = PickupableSpriteMap.Sprites[PickupableEnum.HEALTH];
        HealthDrop.GetComponent<Pickupable>().Pickup = PickupableEnum.HEALTH;

        _graphics = transform.FindChild("Graphics");
        if(_graphics == null) {
            throw new MissingMemberException();
        }else {
            _wanderGraphics = _graphics.FindChild("BaddieWanderArm");
            if(_wanderGraphics == null) {
                throw new MissingMemberException();
            }
        }
        _armGraphics = transform.FindChild("arm");
        if(_armGraphics == null) {
            throw new MissingMemberException();
        }
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
        if(State != MentalStateEnum.NEUTRAL && _wanderGraphics.gameObject.activeSelf) {
            _wanderGraphics.gameObject.SetActive(false);
            _armGraphics.gameObject.SetActive(true);
        } else if(State == MentalStateEnum.NEUTRAL && !_wanderGraphics.gameObject.activeSelf) {
            _wanderGraphics.gameObject.SetActive(true);
            _armGraphics.gameObject.SetActive(false);
        }
        if (State == MentalStateEnum.NEUTRAL) {
            CalculateWanderVelocity();
        } else if (State == MentalStateEnum.NOTICE) {
            CalculateNoticeVelocity();
        } else if (State == MentalStateEnum.ATTACK) {
            CalculateAttackVelocity();
        }else if (State == MentalStateEnum.PURSUE_ATTACK) {
            CalculatePursueAttackVelocity();
        }else if(State == MentalStateEnum.PERSONAL_SPACE) {
            CalculateCreateSpaceVelocity();
        }
        if(Target != null && State != MentalStateEnum.NOTICE && State != MentalStateEnum.NEUTRAL) {
            //Raycast to check if there is something in our way
            float distance = Vector2.Distance(transform.position, Target.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Velocity, 1.5f, Controller.CollisionMask);
            //This indicates the target is within range, however we hit something and now we need to jump over it
            if (hit) {
                CalculateJumpVelocity();
            }
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
        //Check ledges before anything else
        if (Controller.Collisions.NearLedge) {
            _wanderStart = transform.position;
            if (_previousInput == Vector2.right) {
                _previousInput = Vector2.left;
            } else {
                _previousInput = Vector2.right;
            }
        }

        //Do not check for maxdistance threshold if we're recalculating
        if (!_recalculatingWander) {
            //Oscilate back and forth betwen +- maxWanderDist
            if (dist >= _maxWanderDistance || (Controller.Collisions.FromLeft || Controller.Collisions.FromRight)) {
                _wanderStart = transform.position;
                if (_previousInput == Vector2.right) {
                    //this indicates that we collided with something before we wandered far enough 
                    if (Controller.Collisions.FromRight) {
                        RecalculateWanderStart(true);
                    }
                    _previousInput = Vector2.left;
                } else {
                    //this indicates that we collided with something before we wandered far enough 
                    if (Controller.Collisions.FromLeft) {
                        RecalculateWanderStart(false);
                    }
                    _previousInput = Vector2.right;
                }
            }
        }else {
            //We recalculated the wanderStart to be twice as far away, so we need to get back below the maxWanderThreshold
            //Once that happens we are done recalculating and the normal wander calculations can begin again
            if(dist < _maxWanderDistance) {
                _recalculatingWander = false;
            }
        }
        float targetVelocityX = _previousInput.x * _wanderMoveSpeed;
         Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
         FlipGraphics(_previousInput);
    }

    /// <summary>
    /// Face the way we are wandering
    /// </summary>
    private void FlipGraphics(Vector2 wanderingDirection) {
        //rotate left or right to face target
        bool graphicsFacingLeft = _graphics.rotation.eulerAngles.y == 180.0f;
        if(wanderingDirection == Vector2.left && !graphicsFacingLeft) {
            _graphics.rotation = Quaternion.Euler(0f,-180f, 0f);
        } else if(wanderingDirection == Vector2.right && graphicsFacingLeft) {
            _graphics.rotation = Quaternion.Euler(0f, 360f, 0f);
        }
    }

    private void RecalculateWanderStart(bool wanderLeft) {
        _recalculatingWander = true;
        //Double (+-) where we started wandering from in the opposite direction
        print("_wanderStart = " + _wanderStart);
        _wanderStart.x += (_maxWanderDistance * (wanderLeft ? -1 : 1));
        print("Now _wanderStart = " + _wanderStart);
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
    private void CalculateJumpVelocity() {
        //Velocity.x = 0;
        //Need to filter out slopes, because that is not technically a collision
        if (Controller.Collisions.FromBelow && !(Controller.Collisions.ClimbingSlope || Controller.Collisions.DescendingSlope)) {
            Velocity.y = MaxJumpVelocity;
        }
    }

    /// <summary>
    /// Calculate how the object should move during a fight.
    /// Strafeing and jumping at random intervals.
    /// Random bullet checks cause a jump
    /// </summary>
    public void CalculateAttackVelocity() {
        //Calculate random strafing
        //Based off random value - change to go right or left 
        //Only strafe on an interval instead of every frame. Makes it look more realistic
        if (Time.time > _timeToStrafe) {
            var rand = UnityEngine.Random.Range(-10, 10f) * Time.deltaTime;

            Vector2 strafeVelocity = Vector2.right * Mathf.Sign(rand);
            _timeToStrafe = Time.time + 3 / StrafeRate;

            _currentStrafeVelocity = strafeVelocity.x * MoveSpeed;
            Velocity.x = Mathf.SmoothDamp(Velocity.x, _currentStrafeVelocity, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        }else {
            Velocity.x = Mathf.SmoothDamp(Velocity.x, _currentStrafeVelocity, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        }

        //Check if any nearby bullets
        //chance to jump out of the way
        if (BulletWithinRange() && Time.time > _timeToDodge) {
            //If can jump out of the way via time do it
            _timeToDodge = Time.time + 5;
            CalculateJumpVelocity();
        }
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
    /// Very basic "seek the target"
    /// Move in the direction of the target to pursue
    /// </summary>
    private void CalculatePursueAttackVelocity() {
        //If there is an obstacle in the way of the pursuit jump over it 
        //Actually this is where some more intelligent pathfinding could be used for checking where the jump should actually be
        //if target Y is greater than out Y then jump but till move in the direction of the X


        ////Raycast to check if there is something in our way
        //float distance = Vector2.Distance(transform.position, Target.position);
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, Target.position - transform.position, 3f, Controller.CollisionMask);
        ////This indicates the target is within range, however we hit something and now we need to jump over it
        //if (hit) {
        //    CalculateJumpVelocity();
        //}

        float targetVelocityX = _pursueMovespeed * (TargetOnLeft ? -1 : 1);
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
