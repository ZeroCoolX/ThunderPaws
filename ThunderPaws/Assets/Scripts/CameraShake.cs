using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {

    public Camera mainCam;
    private float _shakeAmount = 0f;

    //caching
    private void Awake() {
        if(mainCam == null) {
            mainCam = Camera.main;
        }
    }

    //shake for the alloted length of time
    public void Shake(float amt, float length) {
        _shakeAmount = amt;
        InvokeRepeating("DoShake", 0, 0.01f);//repeat the method, the deplayed seconds, every second time
        Invoke("StopShake", length);
    }

    //apply shake
    private void DoShake() {
        if(_shakeAmount > 0f) {
            Vector3 camPos = mainCam.transform.position;

            //Get shake values
            float offsetX = Random.value * _shakeAmount * 2 - _shakeAmount;//Calculation found online courtesy of Brackeys
            float offsetY = Random.value * _shakeAmount * 2 - _shakeAmount;//Calculation found online courtesy of Brackeys

            //apply shake
            camPos.x += offsetX;
            camPos.y += offsetY;
            //move the main camera
            mainCam.transform.position = camPos;
        }
    }

    //stop
    private void StopShake() {
        CancelInvoke("DoShake");
        //zero out the main camera objects transform which will just set it to where the parent is - which is following the player like always
        mainCam.transform.localPosition = Vector3.zero;
    }
}
