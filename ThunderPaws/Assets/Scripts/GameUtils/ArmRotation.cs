using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows the arm to rotate freely following the mouse movement 
public class ArmRotation : MonoBehaviour {

    void Update() {
            Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            //Normalize the vector x + y + z = 1
            diff.Normalize();

            //find the angle in degrees
            float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            //apply the rotation
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ);//degrees not radians
    }
}
