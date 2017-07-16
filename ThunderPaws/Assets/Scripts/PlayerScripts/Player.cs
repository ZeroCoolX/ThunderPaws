using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
    private PlayerStats _stats;

    [Header("Weapons")]
    //weapon refeerences to enable/disable based on user input
    [SerializeField]
    private GameObject _machineGun; //2
    [SerializeField]
    private GameObject _pistol; //1

    private void Start() {
        //set the player stats
        _stats = PlayerStats.instance;
        _stats.curHealth = _stats.maxHealth;

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

    private void LifeCheck() {
        if(_stats.curHealth <= 0) {
            //The player has died
            GameMaster.KillPlayer(this);
        }else {
            //not dead just hurt
            //TODO: audio
        }
    }

    public void DamageHealth(int dmg) {
        //damage health and check for signs of life
        _stats.curHealth -= dmg;
        LifeCheck();
    } 

}
