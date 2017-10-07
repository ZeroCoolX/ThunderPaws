using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {

    //singleton so other scripts can access 
    public static PlayerStats instance;

    public float healthRegenRate = 2f;//how many times per second regeneration occurs
    public int healthRegenValue = 1;
    public int maxHealth = 100;
    private int _curHealth;
    public int curHealth {
        get { return _curHealth; }
        set { _curHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

    private void Awake() {
        if(instance == null) {
            instance = this;
        }
        curHealth = maxHealth;
    }
}
