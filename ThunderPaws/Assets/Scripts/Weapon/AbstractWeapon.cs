using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class that has functionality all weapons share
/// </summary>
public abstract class AbstractWeapon : MonoBehaviour {

    /// <summary>
    /// How fast the weapon can shoot per second in addition to the first click
    /// </summary>
    [Header("Abstract: Attributes")]
    public float FireRate = 0f;
    /// <summary>
    /// How much damage it does
    /// </summary>
    public int Damage = 10;

    /// <summary>
    /// Bullet graphics
    /// </summary>
    [Header("Abstract: Effects")]
    public Transform BulletTrailPrefab;
    public Transform HitPrefab;
    public Transform MuzzleFlashPrefab;
    /// <summary>
    /// Position where the bullet will spawn
    /// </summary>
    public Transform FirePoint;

    /// <summary>
    /// Graphics spawning: delay from spawning
    /// </summary>
    [Header("Abstract: TimeAttributes")]
    public float TimeToSpawnEffect = 0f;
    /// <summary>
    /// Rate at which the effect should spawn
    /// </summary>
    public float EffectSpawnRate = 10f;
    /// <summary>
    /// Delay between firing
    /// </summary>
    public float _timeToFire = 0f;

    protected void Start () {
        if (FirePoint == null) {
            Debug.LogError("AbstractWeapon.cs: No firePoint found");
            throw new UnassignedReferenceException();
        }
    }

    /// <summary>
    /// Generate particle effect, spawn bullet, then destroy after allotted time
    /// </summary>
    /// <param name="shotPos"></param>
    /// <param name="shotNormal"></param>
    /// <param name="whatToHit"></param>
    public virtual void GenerateEffect(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit) {
        //Fire the projectile - this will travel either out of the frame or hit a target - below should instantiate and destroy immediately
        Transform trail = Instantiate(BulletTrailPrefab, FirePoint.position, FirePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        Bullet bullet = trail.GetComponent<Bullet>();
        //Set layermask of parent (either player or baddie)
        bullet.SetLayerMask(whatToHit);
        //Fire at the point clicked
        bullet.Fire(shotPos, shotNormal);

        //Generate muzzleFlash
        Transform muzzleFlash = Instantiate(MuzzleFlashPrefab, FirePoint.position, FirePoint.rotation) as Transform;
        //Parent to firepoint
        muzzleFlash.parent = FirePoint;
        //Randomize its size a bit
        float size = Random.Range(0.2f, 0.5f);
        muzzleFlash.localScale = new Vector3(size, size, size);
        //Destroy muzzle flash
        Destroy(muzzleFlash.gameObject, 0.035f);
    }

}
