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
    /// Much much slower when we're wandering
    /// </summary>
    private float _idleMoveSpeed = 1f;


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
    private Vector2 _currentStrafeVelocity;

    /// <summary>
    /// Set by the AI controller if we get into the PERSONAL_SPACE zone, the AI will tell us which way to create space
    /// </summary>
    public bool TargetOnLeft;

    public Transform Target { get; set; }

    /// <summary>
    /// Indicates where the "center" of the baddie boss's movement constraints are similar to that of the companion.
    /// Allows us to draw an invisible box around the origin for which will be the area of movement for the boss.
    /// This object can move x amount away from the origin.
    /// </summary>
    private Transform _baddieBossOrigin;
    /// <summary>
    /// Position representation of where the origin of the baddie boss is
    /// </summary>
    private Vector3 _origin;
    /// <summary>
    /// How far away from the origin can the companion float
    /// </summary>
    private Vector2 _floatReach = new Vector2(15f, 6f);
    /// <summary>
    /// Just used as a reference for the Mathf.SmoothDamp function
    /// </summary>
    protected float VelocityYSmoothing;
    /// <summary>
    /// Struct storing plane locations
    /// </summary>
    public ExistenceBounds Bounds;
    /// <summary>
    /// Indicates a 4 quadrant representation of the aarea to which the boss badd can navigate
    /// </summary>
    public Quadrants Quads;
    /// <summary>
    /// A small delay between hitting the bounds, inverting the direction, and choosing a new random direction.
    /// If we hit the bounds, invert the direction so we start coming back into the bounds, and wait half a second before finding another value so that we don't get stuck on the outer rim
    /// </summary>
    private bool _boundsRebounding = false;

    //Just used for debugging
    public Vector2 BottomLeft;
    public Vector2 TopLeft;
    public Vector2 TopRight;
    public Vector2 BottomRight;

    private Vector2 _targetPosition;

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

        _baddieBossOrigin = GameObject.FindGameObjectWithTag("BADDIEBOSSORIGIN").transform;
        if(_baddieBossOrigin == null) {
            Debug.LogError("Could not find a BaddieBossOrigin game object within the scene");
            //In order for the boss to know where/how to move it NEEDS a point of origin
            throw new NullReferenceException();
        }
        SetExistenceBounds();
        SetQuadrants();
        //Just for now center the baddie on the origin - then hard code them to the top bounds so they begin idle movement
        //transform.position = _baddieBossOrigin.position;
         //transform.position = new Vector3(_baddieBossOrigin.position.x, Bounds.Top, _baddieBossOrigin.position.z);

    }

    /// <summary>
    /// Calculate the 4 planes for which the baddie can never exceed
    /// The origin is passed in though so the same method and bounds logic can be used for its idle floaty animation as well as its movement
    /// </summary>
    private void SetExistenceBounds() {
        _origin = _baddieBossOrigin.position;
        Bounds.Top = _origin.y + _floatReach.y;
        Bounds.Bottom = _origin.y - _floatReach.y;
        Bounds.Left = _origin.x - _floatReach.x;
        Bounds.Right = _origin.x + _floatReach.x;
    }

    /// <summary>
    /// Calculate the 4 planes for which the baddie can never exceed
    /// The origin is passed in though so the same method and bounds logic can be used for its idle floaty animation as well as its movement
    /// </summary>
    private void SetQuadrants() {
        Quads.ResetQuadrants();
        Quads.NW = new Vector2[] { new Vector2(Bounds.Left, _origin.y), new Vector2(_origin.x, Bounds.Top) };
        Quads.NE = new Vector2[] { new Vector2(_origin.x, _origin.y), new Vector2(Bounds.Right, Bounds.Top) };
        Quads.SW = new Vector2[] { new Vector2(Bounds.Left, Bounds.Bottom), new Vector2(_origin.x, _origin.y) };
        Quads.SE = new Vector2[] { new Vector2(_origin.x, Bounds.Bottom), new Vector2(Bounds.Right, _origin.y) };
    }

    /// <summary>
    /// Calculate the 4 corners for drawing debug square
    /// </summary>
    private void CalculateCorners() {
        BottomLeft = new Vector2(Bounds.Left, Bounds.Bottom);
        TopLeft = new Vector2(Bounds.Left, Bounds.Top);
        TopRight = new Vector2(Bounds.Right, Bounds.Top);
        BottomRight = new Vector2(Bounds.Right, Bounds.Bottom);
    }

    void Update() {
        CalculateCorners();
        Debug.DrawRay(BottomLeft, Vector2.up * Vector2.Distance(BottomLeft, BottomRight), Color.green);
        Debug.DrawRay(TopLeft, Vector2.right * Vector2.Distance(TopLeft, TopRight), Color.green);
        Debug.DrawRay(TopRight, Vector2.down * Vector2.Distance(TopRight, BottomRight), Color.green);
        Debug.DrawRay(BottomRight, Vector2.left * Vector2.Distance(BottomRight, BottomLeft), Color.green);
        CalculateVelocity();
        var step = MoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);
        if (Quads.CURRENT_INT != -1) {
            UpdateQuadCurrentlyIn();
        }
    }


    private void UpdateQuadCurrentlyIn() {
        if (transform.position.x <= _origin.x) {//Must be NW, SW
            if (transform.position.y <= _origin.y) {//SW
                Quads.CURRENT_INT = Quads.SW_INT;
            } else {//NW
                Quads.CURRENT_INT = Quads.NW_INT;
            }
        } else {//Must be NE, SE
            if (transform.position.y <= _origin.y) {//SE
                Quads.CURRENT_INT = Quads.SE_INT;
            } else {//NE
                Quads.CURRENT_INT = Quads.NE_INT;
            }
        }
        print("Updating what quad we are currently in to: " + Quads.CURRENT_INT);
    }

    private Vector2 CalculateNewPosition() {
        //If no quad is set then recalculate which one we're in
        if (Quads.CURRENT_INT == -1) {
            UpdateQuadCurrentlyIn();
        }
        Vector2 randBoundsMin = Vector2.zero;
        Vector2 randBoundsMax = Vector2.zero;

        //0 - NE, 1 - NW, 2 - SW, 3 - SE

        var index = -1;
        do{
            index = UnityEngine.Random.Range(0, 4);
        }while (index == Quads.CURRENT_INT);
        print("New index = " + index + " and currentINT = " +  Quads.CURRENT_INT);
        if (index == Quads.NE_INT) {
            randBoundsMin = Quads.NE[0];
            randBoundsMax = Quads.NE[1];
        } else if (index == Quads.NW_INT) {
            randBoundsMin = Quads.NW[0];
            randBoundsMax = Quads.NW[1];
            print("Min = " + randBoundsMin + "   and max = " + randBoundsMax);
        } else if (index == Quads.SW_INT) {
            randBoundsMin = Quads.SW[0];
            randBoundsMax = Quads.SW[1];
        } else if (index == Quads.SE_INT) {
            randBoundsMin = Quads.SE[0];
            randBoundsMax = Quads.SE[1];
        } else {
            throw new Exception();
        }
        Quads.NEXT_INT = index;
        var randX = UnityEngine.Random.Range(randBoundsMin.x, randBoundsMax.x);
        var randY = UnityEngine.Random.Range(randBoundsMin.y, randBoundsMax.y);
        print("RandX = " + randX + "   and randY = " + randY);
        return new Vector2(randX, randY);
    }

    /// <summary>
    /// Calculate how the boss should be moving
    /// </summary>
    private void CalculateVelocity() {
        //Calculate random strafing
        //Based off random value - change to go right or left 
        //Only strafe on an interval instead of every frame. Makes it look more realistic
        //Only look for a new position if the time to strafe has been exceeded AND we have arrived in the desired quadrant
        if (Time.time > _timeToStrafe && Quads.CURRENT_INT == Quads.NEXT_INT) {
                print("Get new position");
                _targetPosition = CalculateNewPosition();
                _timeToStrafe = Time.time + 1.5f;
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

    /// <summary>
    /// Stores 4 faces for which the companion cannot leave while idle
    /// </summary>
    public struct ExistenceBounds {//TODO: refactor this because both the companion and baddie boss uses this
        public float Top, Bottom;
        public float Left, Right;
    }

    public struct Quadrants {
        public int NW_INT, NE_INT, SW_INT, SE_INT;
        public int CURRENT_INT, NEXT_INT;
        //Index 0 being the minX, minY
        //Index 1 being the maxX, maxY
        public Vector2[] NW, NE, SW, SE;

        public void ResetQuadrants() {
            CURRENT_INT = -1; NEXT_INT = -1;  NW_INT = 0;  NE_INT = 1; SW_INT = 2; SE_INT = 3;
            NW = new Vector2[]{ Vector2.zero, Vector2.zero};
            NE = new Vector2[] { Vector2.zero, Vector2.zero };
            SW = new Vector2[] { Vector2.zero, Vector2.zero };
            SE = new Vector2[] { Vector2.zero, Vector2.zero };
        }
    }

    /// <summary>
    /// Move the object in an Oscilating motion to simulate floating
    /// </summary>
    private void Idle() {
        float targetVelocityY = Gravity * _idleMoveSpeed;
        Velocity.y = Mathf.SmoothDamp(Velocity.y, targetVelocityY, ref VelocityYSmoothing, 0.2f);
        transform.Translate(Velocity * Time.deltaTime);
        VerticalBoundsCheck();
    }

    /// <summary>
    /// Determine if the object should be floating up or down
    /// </summary>
    private void VerticalBoundsCheck() {
        if (transform.position.y <= Bounds.Bottom) {
            Gravity = 2f;
        } else if (transform.position.y >= Bounds.Top) {
            Gravity = -2f;
        }
    }
}
