using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieStats : LifeformStats {
    /// <summary>
    /// Constructor with optional parameters. Default values are used if not supplied
    /// </summary>
    /// <param name="healthRegenRate"></param>
    /// <param name="healthRegenValue"></param>
    /// <param name="maxHealth"></param>
    public void Initialize(float? healthRegenRate = 2f, int? healthRegenValue = 1, int? maxHealth = 100) {
        //Set all the stat values
        InitializeStats(2f, 1, 100);
    }
}
