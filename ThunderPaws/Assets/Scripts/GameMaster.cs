using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
    //singleton for other scripts to access
    public static GameMaster instance;
    //Camera instance
    public CameraShake camShake;

    //weapon choice 1, 2,...etc default 1 (pistol)
    [SerializeField]
    private int _weaponChoice = 0;
    public int weaponChoice { get { return _weaponChoice; } set { _weaponChoice = value; } }

    //Delegate for switching weapons
    public delegate void WeaponSwitchCallback(int choice);
    public WeaponSwitchCallback onWeaponSwitch;

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

    private void Update() {
        //if the user is switching weapons, change the selection, then update the delegate so the player knows to switch
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            //switch to pistol
            weaponChoice = 1;
            onWeaponSwitch.Invoke(weaponChoice);
        }else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            //switch to machine gun
            weaponChoice = 2;
            onWeaponSwitch.Invoke(weaponChoice);
        }
    }

}
