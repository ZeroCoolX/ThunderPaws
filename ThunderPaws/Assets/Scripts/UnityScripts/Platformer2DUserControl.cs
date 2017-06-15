using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D m_Character;
        private bool m_Jump;
        private bool m_Slide;

        private float slideInterval = 0f;//Can only slide once a second for right now - TODO: Probbably change so can only slide when going a certain speed AND time interval
        private float slideCooler = 0.5f; //Half a second before reset (button needs ot be pressed twice in half a second)
        private bool previousButtonLeft;//indicates that the previous button we pressed was left
        private int slideCharge = 0;

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
        }


        private void Update()
        {
            if (!m_Jump)
            {
                // Read the jump input in Update so button presses aren't missed.
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            //User double presses either right or left resulting in a slide
            //Cannot be done whilst jumping, Must wait 1 second in between slides, and must either be pressing right or left movement keys
            if (!m_Jump && slideInterval <= 0f && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))) {
                //for now only worrying about keyboard
                //MUST be within 0.5 seconds, MUST be the second press, MUST be the same key as the first press within the 0.5 second window
                if (slideCooler > 0f && slideCharge == 1 && (previousButtonLeft == Input.GetKeyDown(KeyCode.A))) {
                    //Start slide interval so we can't infintely slide
                    slideInterval = 1f;
                    m_Slide = true;
                } else {
                    //Slide key was pressed once
                    slideCooler = 0.5f;
                    previousButtonLeft = Input.GetKeyDown(KeyCode.A);
                    ++slideCharge;
                }
            }
            //Decrement slide cooler
            if(slideCooler > 0f) {
                slideCooler -= (1 * Time.deltaTime);
            }else {
                //slideCooler time is up - reset
                slideCharge = 0;
                m_Slide = false;
            }
            //Decrement slide interval if we need to (thanks clamp)
            slideInterval = Mathf.Clamp((slideInterval -(1 * Time.deltaTime)), 0f, 1f);

        }


        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            m_Character.Move(h, crouch, m_Jump, m_Slide, previousButtonLeft);
            m_Jump = false;
            m_Slide = false;
        }
    }
}
