using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionBase : MonoBehaviour {
    /// <summary>
    /// Layermask indicating what to query for closest targets
    /// </summary>
    public LayerMask TargetLayerMask;

    /// <summary>
    /// Just used as a reference for the Mathf.SmoothDamp function
    /// </summary>
    protected float VelocityYSmoothing;

    /// <summary>
    /// How far away from the origin can the companion float
    /// </summary>
    private float _floatReach = 0.25f;
    /// <summary>
    /// Middle of its existence 
    /// </summary>
    public Vector3 Origin;
    /// <summary>
    /// Struct storing plane locations
    /// </summary>
    public ExistenceBounds Bounds;

    public Vector2 BottomLeft;
    public Vector2 TopLeft;
    public Vector2 TopRight;
    public Vector2 BottomRight;

    /// <summary>
    /// Companions velocity
    /// </summary>
    public Vector2 Velocity;
    /// <summary>
    /// Gravity constant for companion
    /// </summary>
    public float Gravity = -1f;
    /// <summary>
    /// how fast the companion moves within its floating existence
    /// </summary>
    public float MoveSpeed = 0.3f;
    
    /// <summary>
    /// Reference to the weapon on the companion so we can set who is the target and when to shoot
    /// </summary>
    private CompanionWeapon _companionWeapon;
    /// <summary>
    /// This is where the companion gets its bounds from because it moves relative to where the player is
    /// </summary>
    private Transform _companionOrigin;
    /// <summary>
    /// Who this object is a companion TOO
    /// </summary>
    public Transform Leader;

    /// <summary>
    /// Max distance the companion searches for a target
    /// </summary>
    private float _maxTargetLocateRadius = 10f;

    /// <summary>
    /// Set from the CompanionFollow script to indicate the player isn't moving
    /// </summary>
    public bool Idle = true;

    void Start () {
        var weaponTransform = transform.Find("Weapon");
        if(weaponTransform != null) {
            _companionWeapon = weaponTransform.GetComponent<CompanionWeapon>();
        }

        _companionOrigin = Leader.Find("CompanionOrigin");
        if (_companionOrigin == null) {
            //We have a problem 
            Debug.LogError("Could not find CompanionOrigin on: " + Leader.name);
            throw new MissingReferenceException();
        }

        SetExistenceBounds();

        //Calculate the closest target once a second.
        //We handle if we need to shoot or not, but regardless calculate it
        InvokeRepeating("HandleShooting", 0f, 1f);
    }

    /// <summary>
    /// Calculate the 4 planes for which the companion can never exceed
    /// </summary>
    private void SetExistenceBounds() {
        Origin = _companionOrigin.position;
        Bounds.Top = Origin.y + _floatReach;
        Bounds.Bottom = Origin.y - _floatReach;
        Bounds.Left = Origin.x - _floatReach;
        Bounds.Right = Origin.x + _floatReach;
        CalculateCorners();
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

    void Update () {
        if (_companionOrigin != null) {
            SetExistenceBounds();
            Debug.DrawRay(BottomLeft, Vector2.up * Vector2.Distance(BottomLeft, BottomRight), Color.green);
            Debug.DrawRay(TopLeft, Vector2.right * Vector2.Distance(TopLeft, TopRight), Color.green);
            Debug.DrawRay(TopRight, Vector2.down * Vector2.Distance(TopRight, BottomRight), Color.green);
            Debug.DrawRay(BottomRight, Vector2.left * Vector2.Distance(BottomRight, BottomLeft), Color.green);
            VerticalBoundsCheck();
            //Only move if we should idle. Otherwise the player is moving and we need to catch up with them
            if (Idle) {
                Move();
            }

            if (_companionWeapon.Target != null) {
                _companionWeapon.Shoot();
            }
        }
    }

    /// <summary>
    /// Move the object in an Oscilating motion to simulate floating
    /// </summary>
    private void Move() {
        float targetVelocityY = Gravity * MoveSpeed;
        Velocity.y = Mathf.SmoothDamp(Velocity.y, targetVelocityY, ref VelocityYSmoothing, 0.2f);
        transform.Translate(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// Determine if the object should be floating up or down
    /// </summary>
    private void VerticalBoundsCheck() {
        if(transform.position.y <= Bounds.Bottom) {
            Gravity = 1f;
        }else if(transform.position.y >= Bounds.Top) {
            Gravity = -1f;
        }
    }

    /// <summary>
    /// Stores 4 faces for which the companion cannot leave while idle
    /// </summary>
    public struct ExistenceBounds {
        public float Top, Bottom;  
        public float Left, Right;
    }

    /// <summary>
    /// Handles setting the companion weapons target
    /// </summary>
    private void HandleShooting() {
        _companionWeapon.Target = GetClosestTarget();
    }

    /// <summary>
    /// Given a set radius, draw a circle and collect any collider of the LayerMask supplied (that way we only query for desired objects instad of everything) that hits it
    /// Traverse through the collection and store the closes or null if none found
    /// </summary>
    /// <returns></returns>
    private Transform GetClosestTarget() {
        var hitColliders = Physics2D.OverlapCircleAll(transform.position, _maxTargetLocateRadius, TargetLayerMask);
        float currentClosest = Mathf.Infinity;
        Transform closestTarget = null;
        foreach(var hit in hitColliders) {
            if(Vector2.Distance(transform.position, hit.transform.position) < currentClosest) {
                closestTarget = hit.transform;
            }
        }
        return closestTarget;
    }
}
