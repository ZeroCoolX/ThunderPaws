using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionBase : MonoBehaviour {
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

    public bool Idle = true;

    void Start () {
        Origin = transform.position;
        SetExistenceBounds();
    }

    /// <summary>
    /// Calculate the 4 planes for which the companion can never exceed
    /// </summary>
    private void SetExistenceBounds() {
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
        Debug.DrawRay(BottomLeft, Vector2.up * Vector2.Distance(BottomLeft, BottomRight), Color.green);
        Debug.DrawRay(TopLeft, Vector2.right * Vector2.Distance(TopLeft, TopRight), Color.green);
        Debug.DrawRay(TopRight, Vector2.down * Vector2.Distance(TopRight, BottomRight), Color.green);
        Debug.DrawRay(BottomRight, Vector2.left * Vector2.Distance(BottomRight, BottomLeft), Color.green);
        VerticalBoundsCheck();
        //Only move if we should idle. Otherwise the player is moving and we need to catch up with them
        if (Idle) {
            Move();
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
    /// Add the gravity constant to .y component of velocity
    /// Do not accumulate gravity if colliding with anything vertically
    /// </summary>
    private void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }

    /// <summary>
    /// Stores 4 faces for which the companion cannot leave while idle
    /// </summary>
    public struct ExistenceBounds {
        public float Top, Bottom;  
        public float Left, Right;
    }
}
