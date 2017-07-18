using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaddieAI))]
public class Baddie : MonoBehaviour {//TODO: baddieStats and PlayerStats are like identical...maybe abstract just like Weapon

    //Each baddie type will have their own stats so for now inner class works
    [System.Serializable]
    public class BaddieStats {
        public int maxHealth = 100;
        private int _curHealth;
        public int curHealth {
            get { return _curHealth; }
            set { _curHealth = Mathf.Clamp(value, 0, maxHealth); }
        }

        public int damage = 5;

        public void Init() {
            curHealth = maxHealth;
        }
    }

    //Reference to inner class
    BaddieStats stats = new BaddieStats();

    public Transform deathParticles;
    public Transform healthDrop;
    public int healthDropAmount = 10;

    [Header("Optional: ")]
    [SerializeField]
    private StatusIndicator statusIndicator;

    //for death camera shake
    public float shakeAmount = 0.05f;
    public float shakeLength = 0.1f;

    private void Start() {
        //initialize stats
        stats.Init();
        //set baddie health
        if(statusIndicator != null) {
            statusIndicator.SetHealth(stats.curHealth, stats.maxHealth);
        }

        if(deathParticles == null) {
            Debug.LogError("No death particles found");
        }
        //TODO: add user interface
    }

    private void LifeCheck() {
        //Kill the baddie
        if (stats.curHealth <= 0) {
            GameMaster.KillBaddie(this);
        } else {
            //Not dead just hurt
            //TODO: audio and maybe color flash or something to visually indicate
        }
    }
    
    public void DamageHealth(int dmg) {
        //Damage baddie and check vitals
        stats.curHealth -= dmg;
        if(statusIndicator != null) {
            statusIndicator.SetHealth(stats.curHealth, stats.maxHealth);
        }
        LifeCheck();
    }
}
