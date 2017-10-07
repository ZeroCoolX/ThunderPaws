using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : LifeformStats {
    /// <summary>
    /// Singleton so other scripts can reference this one persistent object
    /// </summary>
    public static PlayerStats Instance;

    private void Awake() {
        //Set all the stat values
        InitializeStats(2f, 1, 100);
        //Create singleton
        if(Instance == null) {
            Instance = this;
        }
    }
}
