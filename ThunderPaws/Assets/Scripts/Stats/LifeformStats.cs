using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeformStats : MonoBehaviour {

    /// <summary>
    /// How many times a second regeneration occurs
    /// </summary>
    public float HealthRegenRate;
    /// <summary>
    /// How much by value regeneration produces
    /// </summary>
    public int HealthRegenValue;
    /// <summary>
    /// Maximum health
    /// </summary>
    public int MaxHealth;
    /// <summary>
    /// Current health at any given time
    /// </summary>
    private int _curHealth;
    public int CurHealth {
        get { return _curHealth; }
        set { _curHealth = Mathf.Clamp(value, 0, MaxHealth); }
    }

    /// <summary>
    /// Set all the stat values
    /// </summary>
    /// <param name="healthRegenRate"></param>
    /// <param name="healthRegenValue"></param>
    /// <param name="maxHealth"></param>
    protected void InitializeStats(float healthRegenRate, int healthRegenValue, int maxHealth) {
        HealthRegenRate = healthRegenRate;
        HealthRegenValue = healthRegenValue;
        CurHealth = MaxHealth = maxHealth;
    }
}
