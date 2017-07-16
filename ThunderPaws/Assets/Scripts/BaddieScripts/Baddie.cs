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

    private void Start() {
        //initialize stats
        stats.Init();
    }

    private void LifeCheck() {
        //Kill the baddie
        if (stats.curHealth <= 0) {
            GameMaster.KillBaddie(this);
        } else {
            //Not dead just hurt
            //TODO: audio
        }
    }
    
    public void DamageHealth(int dmg) {
        //Damage baddie and check vitals
        stats.curHealth -= dmg;
        LifeCheck();
    }
}
