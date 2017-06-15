using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    //TODO: Fix the horizontal collisions - the player gets stuck on walls since technically hes colliding...which makes him like spiderman just sticking and "jumping" up walls which is not intended
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField]
        private float m_SlideMultiplier = 1.5f;
        private float slideDuration = 0f;
        [SerializeField]
        private float m_JumpForce = 400f;// Amount of force added when the player jumps.      
        [SerializeField]
        private float m_DoubleJumpImpulse = 15f; //Impulse to add for the mid air double jump
        [SerializeField]
        private bool m_DoubleJump;
        [Range(0, 1)]
        [SerializeField]
        private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField]
        private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField]
        private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
        private bool m_RightSideUp = true;  //  for determining if the arm should be inverted for left vs right direction

        private Transform playerGraphics;   //Reference to the player graphics so we can change direction ourself

        public Transform playerArm;        //ref to the arm used for roataion determined facing - free from the body graphics

        private void Awake() {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            playerGraphics = transform.FindChild("Graphics");
            if (playerGraphics == null) {
                //couldn't find the graphics object
                Debug.LogError("PlatformCharacter2D.cs - No player graphics detected as a child of the player. This is bad");
            }
        }


        private void FixedUpdate() {
            bool wasGrounded = m_Grounded;

            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);//TODO: do not collide with walls in regards to sliding down them unless wall climbing or grabbing
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].gameObject != gameObject) {
                    m_Grounded = true;
                    m_DoubleJump = false;
                }
            }
            m_Anim.SetBool("Ground", m_Grounded);

            // Set the vertical animation
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);

            //If we're still in the air, and the jump button is pressed perform another jump
            if (!m_Grounded) {
                if (!m_DoubleJump) {
                    // Read the jump input in Update so button presses aren't missed.
                    m_DoubleJump = CrossPlatformInputManager.GetButtonDown("Jump");
                    if (m_DoubleJump) {
                        Jump(true);//perform double jump
                    }
                }
            }
            //If we are actively sliding decrement the duration
            //this ensures we dont reset the velocity while sliding until the slide is complete
            if(slideDuration > 0f) {
                Debug.Log("slide duration = " + slideDuration);
                slideDuration -= (1 * Time.deltaTime);
            }else {
                slideDuration = 0f;
            }
        }


        public void Move(float move, bool crouch, bool jump, bool slide, bool slideLeft) {
            //TODO: Change slide so you probably have to be at least moving first - but for now allow sliding from a stand still
            //Apply impulse in the direction they indicated
            if (m_Grounded && slide && m_Rigidbody2D.velocity.x != 0f) {//Only slide if we're moving either left or right
                m_Rigidbody2D.AddForce(new Vector2(m_MaxSpeed * m_SlideMultiplier * (m_Rigidbody2D.velocity.x > 0 ? 1 : -1), m_Rigidbody2D.velocity.y), ForceMode2D.Impulse);
                slideDuration = 0.25f;
            }

            // If crouching, check to see if the character can stand up
            if (!crouch && m_Anim.GetBool("Crouch")) {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround)) {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl) {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move * m_CrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character - but only if we're not actively sliding
                if (slideDuration <= 0f) {
                    m_Rigidbody2D.velocity = new Vector2(move * m_MaxSpeed, m_Rigidbody2D.velocity.y);
                }

                //Rotate the player based on direction pointing - its more natural
                Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerArm.position;
                //Normalize the vector x + y + z = 1
                diff.Normalize();
                //find the angle in degrees
                float rotZ = Mathf.Abs(Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
                if ((rotZ <= 90f) && !m_FacingRight) {
                    //face right
                    Flip();
                    if (!m_RightSideUp) {
                        invertArm();
                    }
                } else if ((rotZ > 90f) && m_FacingRight) {
                    //face left
                    Flip();
                    if (m_RightSideUp) {
                        invertArm();
                    }
                }

            }

            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("Ground")) {
                Jump(false);
            }
        }

        private void Jump(bool doubleJump) {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Anim.SetBool("Ground", false);
            if (doubleJump) {//if we're going upwards the velocity.y > 0 so subtract that from the impulse, otherwise we're going down < 0 so add to it - all about a consistent double jump
                m_Rigidbody2D.AddForce(new Vector2(0f, m_DoubleJumpImpulse + (m_Rigidbody2D.velocity.y * -1f)), ForceMode2D.Impulse);
            } else {
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce), ForceMode2D.Force);
            }
        }


        private void Flip() {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = playerGraphics.localScale;
            theScale.x *= -1;
            playerGraphics.localScale = theScale;
        }

        private void invertArm() {
            //switch the way the arm is labeled as facing
            m_RightSideUp = !m_RightSideUp;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = playerArm.localScale;
            theScale.y *= -1;
            playerArm.localScale = theScale;

            //Also deal with the arm rotation axis offset since the graphics, arm, and colliders are all seperate.
            //This 0.3 offset is because the pivot point on the graphics is dead center, but the arm is at the shoulder for a natural arm movement.
            //The offset allows the arm to stay in place when left or right. Otherwise it jutts out when facing left because its flipping scale based on the rotational axis
            if (theScale.y < 0f) {
                theScale = playerArm.transform.localPosition;
                theScale.x += 0.3f;
            } else {
                theScale = playerArm.transform.localPosition;
                theScale.x -= 0.3f;
            }
            playerArm.transform.localPosition = theScale;
        }
    }
}
