using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
    //singleton for other scripts to access
    public static GameMaster instance;
    //Camera instance
    public CameraShake camShake;

    private void Awake() {
        if(instance == null) {
            instance = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        }
    }

    private void Start() {
        if(camShake == null) {
            Debug.LogError("GameMaster.cs: No CameraShake found");
        }
    }
}
