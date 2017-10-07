using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaddieAI))]
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
    /// Amount to shake camera by
    /// </summary>
    public float ShakeAmount = 0.05f;
    /// <summary>
    /// How long to shake camera for
    /// </summary>
    public float ShakeLength = 0.1f;

    /// <summary>
    ///  USED FOR TESTING ONLY.
    ///  Indicates the type of movement to be acted. 
    /// </summary>
    public bool Jump = false;
    /// <summary>
    ///  USED FOR TESTING ONLY.
    ///  Stores the generated input until it changes. 
    /// </summary>
    private Vector2 _previousInput;

    private void Start() {
        //Set all physics values
        InitializePhysicsValues(6f, 4f, 0.4f, 0.2f, 0.1f);

        //Confirm stats component and initialize
        _stats = transform.GetComponent<BaddieStats>();
        if(_stats == null) {
            Debug.LogError("No BaddieStats found on Baddie");
            throw new MissingComponentException();
        }
        _stats.Initialize();

        //Validate Status indicator
        if (_statusIndicator != null) {
            _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
        }
        //Validate Death particles are set
        if(DeathParticles == null) {
            Debug.LogError("No death particles found");
            throw new UnassignedReferenceException();
        }
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        CalculateVelocityOffInput();
        ApplyGravity();
        Controller.Move(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// Contents are temporary for testing. AI movement script not written yet.
    /// Change direction either vertical or horizontal an arbitrary amount and some interval
    /// </summary>
    private void CalculateVelocityOffInput() {

        //TODO: Movement should be based off AI Movement scripting
        if(Controller.Collisions.FromLeft || Velocity == Vector3.zero) {
            _previousInput = Vector2.right;
        }else if(Controller.Collisions.FromRight) {
            _previousInput = Vector2.left;
        }
        Vector2 inputJump = new Vector2(0f, Random.Range(-1f, 1f));
        if (Jump) {
            if (inputJump.y > 0 && Controller.Collisions.FromBelow) {
                Velocity.y = JumpVelocity;
            }
        } else {
            float targetVelocityX = _previousInput.x * MoveSpeed;
            print("target x = " + targetVelocityX);
            Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        }
    }

    /// <summary>
    /// Check the stats to see if we need to destroy the object
    /// </summary>
    private void LifeCheck() {
        //Kill the baddie
        if (_stats.CurHealth <= 0) {
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
        if(_statusIndicator != null) {
            _statusIndicator.SetHealth(_stats.CurHealth, _stats.MaxHealth);
        }
        LifeCheck();
    }
}
