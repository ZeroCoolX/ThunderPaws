using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls  the backgorund scrolling
public class ParallaxController : MonoBehaviour {

    /// <summary>
    /// Collection of backgrounds to be parallaxed
    /// </summary>
    public List<Transform> Backgrounds;
    /// <summary>
    /// Proportion of the camera movement to move the backgrounds by
    /// </summary>
    private List<float> _parallaxScales;
    /// <summary>
    /// How smooth the parallax is going to be. Must be > 0
    /// </summary>
    public float ParallaxSmoothing = 1f;

    /// <summary>
    /// Main camera transform ref
    /// </summary>
    private Transform _cam;
    /// <summary>
    /// Store position of the camera in the previous frame - used for parallax calculation
    /// </summary>
    private Vector3 _previousCamPosition;

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
        for(int i = 0; i < Backgrounds.Count; ++i) {
            //Parallax value is the opposite of the cameras movement * scale
            float parallax = (_previousCamPosition.x - _cam.position.x) * _parallaxScales[i];
            //Set a target x position which is the current postition + parallax
            float backgroundTargetPosX = Backgrounds[i].position.x + parallax;
            //Create target position which is the backgrounds current position with its target x pos
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, Backgrounds[i].position.y, Backgrounds[i].position.z);
            //Fade between current target position
            Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, backgroundTargetPos, ParallaxSmoothing * Time.deltaTime);
        }

        //store the previous cam position
        _previousCamPosition = _cam.position;
	}

    //TODO: Add /// documentation
    private void UpdateParallaxScales() {
        for (int i = 0; i < Backgrounds.Count; ++i) {
            if(_parallaxScales.Count <= i) {
                _parallaxScales.Add(Backgrounds[i].position.z * -1);
            }else {
                _parallaxScales[i] = Backgrounds[i].position.z * -1;
            }
        }
    }

    //TODO: Add /// documentation
    public void AddParallax(Transform newParallax) {
        Backgrounds.Add(newParallax);
        UpdateParallaxScales();
    }

    //TODO: Add /// documentation
    public void RemoveParallax(Transform parallaxToRemove) {
        Backgrounds.Remove(parallaxToRemove);
        UpdateParallaxScales();
    }
}
