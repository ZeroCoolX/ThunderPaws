using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
    private PlayerStats _stats;

    public Transform deathParticles;
    //for death camera shake
    public float shakeAmount = 0.05f;
    public float shakeLength = 0.1f;

    [SerializeField]
    private StatusIndicator statusIndicator;

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

        if(statusIndicator == null) {
            Debug.LogError("No status indicator found");
        }
        statusIndicator.SetHealth(_stats.curHealth, _stats.maxHealth);

        if(deathParticles == null) {
            Debug.LogError("No player death particles found");
        }

        //select the default weapon
        SelectWeapon(GameMaster.instance.weaponChoice);
        //Add the weapon switch method onto the weaponSwitch delegate
        GameMaster.instance.onWeaponSwitch += SelectWeapon;

        //Regenerate health over time
        InvokeRepeating("RegenHealth", 1f / _stats.healthRegenRate, 1f / _stats.healthRegenRate);
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
        if(statusIndicator != null) {
            statusIndicator.SetHealth(_stats.curHealth, _stats.maxHealth);
        }
        LifeCheck();
    } 

    private void RegenHealth() {
        _stats.curHealth += _stats.healthRegenValue;
        //update visual health bad
        statusIndicator.SetHealth(_stats.curHealth, _stats.maxHealth);
    }

}
