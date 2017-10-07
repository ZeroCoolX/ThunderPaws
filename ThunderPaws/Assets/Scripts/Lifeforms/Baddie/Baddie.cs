using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaddieAI))]
public class Baddie : LifeformBase {
    /// <summary>
    /// Baddie Stats reference
    /// </summary>
    BaddieStats Stats = new BaddieStats();

    public Transform DeathParticles;
    public Transform HealthDrop;
    public int HealthDropAmount = 10;

    [Header("Optional: ")]
    [SerializeField]
    private StatusIndicator _statusIndicator;

    //for death camera shake
    public float ShakeAmount = 0.05f;
    public float ShakeLength = 0.1f;

    public bool Jump = false;
    private Vector2 _previousInput;

    private void Start() {
        //initialize all collisiondetection  values
        InitializePhysicsValues(6f, 4f, 0.4f, 0.2f, 0.1f);

        //initialize stats
        Stats.Initialize();
        //set baddie health
        if(_statusIndicator != null) {
            _statusIndicator.SetHealth(Stats.CurHealth, Stats.MaxHealth);
        }

        if(DeathParticles == null) {
            Debug.LogError("No death particles found");
        }
        //TODO: add user interface
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
    /// Change direction either vertical or horizontal an arbitrary amount and some interval
    /// </summary>
    private void CalculateVelocityOffInput() {

        //TODO: this is suuuper basic so fix later
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

    private void LifeCheck() {
        //Kill the baddie
        if (Stats.CurHealth <= 0) {
            GameMaster.KillBaddie(this);
        } else {
            //Not dead just hurt
            //TODO: audio and maybe color flash or something to visually indicate
        }
    }
    
    public void DamageHealth(int dmg) {
        //Damage baddie and check vitals
        Stats.CurHealth -= dmg;
        if(_statusIndicator != null) {
            _statusIndicator.SetHealth(Stats.CurHealth, Stats.MaxHealth);
        }
        LifeCheck();
    }

    /// <summary>
    /// Basic stats class
    /// </summary>
    [System.Serializable]
    public class BaddieStats {
        public int MaxHealth = 100;
        private int _curHealth;
        public int CurHealth {
            get { return _curHealth; }
            set { _curHealth = Mathf.Clamp(value, 0, MaxHealth); }
        }

        public int Damage = 5;

        public void Initialize() {
            CurHealth = MaxHealth;
        }
    }
}
