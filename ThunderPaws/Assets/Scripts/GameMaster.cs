using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
    //singleton for other scripts to access
    public static GameMaster instance;

    [Header("Scripts")]
    //Camera instance
    public CameraShake camShake;

    //weapon choice 1, 2,...etc default 1 (pistol)
    [Header("Weapon Data")]
    [SerializeField]
    private int _weaponChoice = 0;
    public int weaponChoice { get { return _weaponChoice; } set { _weaponChoice = value; } }

    [Header("Health Data")]
    [SerializeField]
    private int _maxLives = 3;
    [SerializeField]
    private static int _remainingLives;
    public static int remainingLives { get { return _remainingLives; } set { _remainingLives = value; } }

    //Delegate for switching weapons
    public delegate void WeaponSwitchCallback(int choice);
    public WeaponSwitchCallback onWeaponSwitch;

    //TODO: currency
    //TODO: audio
    public Transform player;
    public Transform spawnPoint;
    public int spawnDelay = 2;
    public GameObject spawnPrefab;

    private void Awake() {
        if(instance == null) {
            instance = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        }
    }

    private void Start() {
        if(camShake == null) {
            Debug.LogError("GameMaster.cs: No CameraShake found");
        }
        //set lives
        _remainingLives = _maxLives;
    }

    private void Update() {
        //if the user is switching weapons, change the selection, then update the delegate so the player knows to switch
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            //switch to pistol
            weaponChoice = 1;
            onWeaponSwitch.Invoke(weaponChoice);
        }else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            //switch to machine gun
            weaponChoice = 2;
            onWeaponSwitch.Invoke(weaponChoice);
        }
    }

    public static void KillBaddie(Baddie baddie) {
        //TODO: sound
        //Generate death particles
        Transform clone = Instantiate(baddie.deathParticles, baddie.transform.position, Quaternion.identity) as Transform;
        Destroy(clone.gameObject, 3f);

        //Generate camera shake
        instance.camShake.Shake(baddie.shakeAmount, baddie.shakeLength);

        //Generate health drop
       // Instantiate(baddie.healthDrop, baddie.transform.position, Quaternion.identity);

        //Actually kill it finally
        instance.KillDashNine(baddie.gameObject, false);
    }

    public static void KillPlayer(Player player) {
        //decrement lives
        --remainingLives;

        //Generate death particles
        Transform clone = Instantiate(player.deathParticles, player.transform.position, Quaternion.identity) as Transform;
        Destroy(clone.gameObject, 3f);

        //Generate camera shake
        instance.camShake.Shake(player.shakeAmount, player.shakeLength);

        //kill the player
        instance.KillDashNine(player.gameObject, true);
    }

    //Actual destruction of optional respawn
    private void KillDashNine(GameObject obj, bool respawn) {
        Destroy(obj);
        if (respawn) { 
            instance.StartCoroutine(instance.RespawnPlayer());
        }
    }

    private IEnumerator RespawnPlayer() {//TODO: spawn sound
        yield return new WaitForSeconds(spawnDelay);
        Instantiate(player, spawnPoint.position, spawnPoint.rotation);
        GameObject clone = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        Destroy(clone, 3f);
    }

}
