using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls  the backgorund scrolling
public class Parallaxing : MonoBehaviour {

    //Collection of backgrounds to be parallaxed
    public List<Transform> backgrounds;
    //proportion of the camera movement to move the backgrounds by
    private List<float> _parallaxScales;
    //How smooth the parallax is going to be. Must be > 0
    public float parallaxSmoothing = 1f;

    //Main camera transform ref
    private Transform _cam;
    //Store position of the camera in the previous frame - used for parallax calculation
    private Vector3 _previousCamPosition;

    //Set refs
    private void Awake() {
        _cam = Camera.main.transform;
    }

    
    void Start () {
        //Store the previous frame at the current frames camera position
        _previousCamPosition = _cam.position;

        //for each background assign that background position to the corresponding parallax scale
        _parallaxScales = new List<float>();//new float[backgrounds.Length];
        UpdateParallaxScales();
    }

    void Update () {
		//do for each background
        for(int i = 0; i < backgrounds.Count; ++i) {
            //Parallax value is the opposite of the cameras movement * scale
            float parallax = (_previousCamPosition.x - _cam.position.x) * _parallaxScales[i];
            //Set a target x position which is the current postition + parallax
            float backgroundTargetPosX = backgrounds[i].position.x + parallax;
            //Create target position which is the backgrounds current position with its target x pos
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgrounds[i].position.y, backgrounds[i].position.z);
            //fade between current target position
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPos, parallaxSmoothing * Time.deltaTime);
        }

        //store the previous cam position
        _previousCamPosition = _cam.position;
	}

    private void UpdateParallaxScales() {
        for (int i = 0; i < backgrounds.Count; ++i) {
            if(_parallaxScales.Count <= i) {
                _parallaxScales.Add(backgrounds[i].position.z * -1);
            }else {
                _parallaxScales[i] = backgrounds[i].position.z * -1;//courtesy of Brackeys
            }
        }
    }

    public void AddParallax(Transform newParallax) {
        backgrounds.Add(newParallax);
        UpdateParallaxScales();
    }

    
    public void RemoveParallax(Transform parallaxToRemove) {
        backgrounds.Remove(parallaxToRemove);
        UpdateParallaxScales();
    }
}
