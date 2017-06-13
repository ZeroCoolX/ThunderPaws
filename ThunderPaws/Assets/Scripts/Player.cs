using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Platformer2DUserControl))]
public class Player : MonoBehaviour {//TODO: also flip the players colliders, for some reason that is not flipping...the baddies is however...
    //TODO: health and player stats

    //weapon refeerences to enable/disable based on user input
    [SerializeField]
    private GameObject _machineGun; //2
    [SerializeField]
    private GameObject _pistol; //1

    private void Start() {
        //select the default weapon
        SelectWeapon(GameMaster.instance.weaponChoice);
        //Add the weapon switch method onto the weaponSwitch delegate
        GameMaster.instance.onWeaponSwitch += SelectWeapon;
    }

    private void SelectWeapon(int choice) {
        //TODO: only allow weapon switch if they have the weapon
        if(_machineGun != null && _pistol != null) {
            _machineGun.SetActive(false);
            _pistol.SetActive(false);
            switch (choice) {
                case 1:
                    _pistol.SetActive(true);
                    break;
                case 2:
                    _machineGun.SetActive(true);
                    break;
                default:
                    _pistol.SetActive(true);
                    break;
            }
        }
    }
}
