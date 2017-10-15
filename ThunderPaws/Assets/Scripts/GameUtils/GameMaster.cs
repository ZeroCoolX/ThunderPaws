using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
    /// <summary>
    /// List of possible pikcupable sprite references so we can fill a map with corresponding enums so whenever something picks up one, they know whaat to do with it
    /// Also this allows us to programatically set the SpriteRenderer of any generic Pickupable without having to create specific prefabs for each
    /// </summary>
    public Sprite[] Sprites = new Sprite[2];

    /// <summary>
    /// Singleton for other scripts to access
    /// </summary>
    public static GameMaster Instance;

    /// <summary>
    /// Camera instance
    /// </summary>
    [Header("Scripts")]
    public CameraShake CamShake;

    [Header("Weapon Data")]
    [SerializeField]
    private WeaponEnum _weaponChoice = WeaponEnum.PISTOL;
    /// <summary>
    /// Weapon choice 1, 2,...etc default 1 (pistol)
    /// </summary>
    public WeaponEnum WeaponChoice { get { return _weaponChoice; } set { _weaponChoice = value; } }

    /// <summary>
    /// Max lives per game
    /// </summary>
    [Header("Health Data")]
    [SerializeField]
    private int _maxLives = 3;

    /// <summary>
    /// Determines when we have ended the game
    /// </summary>
    [SerializeField]
    private static int _remainingLives;

    /// <summary>
    /// Allows us to spawn the UI at the end of the game
    /// </summary>
    public GameObject GameOverUI;

    /// <summary>
    /// Remaining lives counter must persist through player deaths
    /// </summary>
    public static int RemainingLives { get { return _remainingLives; } set { _remainingLives = value; } }

    /// <summary>
    /// Delegate for switching weapons
    /// </summary>
    /// <param name="choice"></param>
    public delegate void WeaponSwitchCallback(WeaponEnum choice);
    public WeaponSwitchCallback OnWeaponSwitch;

    //TODO: currency
    //TODO: audio
    /// <summary>
    /// Player reference for respawning
    /// </summary>
    public Transform Player;
    /// <summary>
    /// Where to respawn the player 
    /// </summary>
    public Transform SpawnPoint;
    /// <summary>
    /// How long to wait from player death to respawn
    /// </summary>
    public int SpawnDelay = 2;
    /// <summary>
    /// Spawn prefab reference
    /// </summary>
    public GameObject SpawnPrefab;

    private void Awake() {
        if(Instance == null) {
            Instance = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        }
        FillPickupableSprites();
    }

    private void Start() {
        ///Validate Camera Shake reference
        if(CamShake == null) {
            Debug.LogError("GameMaster.cs: No CameraShake found");
            throw new MissingComponentException();
        }
        //Set remaining lives
        _remainingLives = _maxLives;
    }

    private void Update() {
        //If the user is switching weapons, change the selection, then update the delegate so the player knows to switch
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            //Switch to pistol
            WeaponChoice = WeaponEnum.PISTOL;
            OnWeaponSwitch.Invoke(WeaponChoice);
        }else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            //Switch to machine gun
            WeaponChoice = WeaponEnum.MACHINE_GUN;
            OnWeaponSwitch.Invoke(WeaponChoice);
        }
    }

    /// <summary>
    /// Genreate death particles, shake camera, destroy baddie game object
    /// </summary>
    /// <param name="baddie"></param>
    public static void KillBaddie(Baddie baddie) {
        //TODO: sound
        //Generate death particles
        Transform clone = Instantiate(baddie.DeathParticles, baddie.transform.position, Quaternion.identity) as Transform;
        Destroy(clone.gameObject, 3f);

        //Generate camera shake
        Instance.CamShake.Shake(baddie.ShakeAmount, baddie.ShakeLength);

        //Generate health drop
       // Instantiate(baddie.healthDrop, baddie.transform.position, Quaternion.identity);

        //Actually kill it finally
        Instance.KillDashNine(baddie.gameObject, false);
    }

    /// <summary>
    /// Decrement lives, generate particles, shake camera and destroy current player reference
    /// </summary>
    /// <param name="player"></param>
    public static void KillPlayer(Player player) {
        //decrement lives
        --RemainingLives;

        //Generate death particles
        Transform clone = Instantiate(player.DeathParticles, player.transform.position, Quaternion.identity) as Transform;
        Destroy(clone.gameObject, 3f);

        //Generate camera shake
        Instance.CamShake.Shake(player.ShakeAmount, player.ShakeLength);

        //kill the player
        Instance.KillDashNine(player.gameObject, RemainingLives > 0);
    }

    /// <summary>
    /// Actual destruction of optional respawn
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="respawn"></param>
    private void KillDashNine(GameObject obj, bool respawn) {
        Destroy(obj);
        if (respawn) { 
            Instance.StartCoroutine(Instance.RespawnPlayer());
        }else {
            if (RemainingLives <= 0) {
                GameOverUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Respawn player
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnPlayer() {
        //TODO: spawn sound
        yield return new WaitForSeconds(SpawnDelay);
        Instantiate(Player, SpawnPoint.position, SpawnPoint.rotation);
        GameObject clone = Instantiate(SpawnPrefab, SpawnPoint.position, SpawnPoint.rotation) as GameObject;
        Destroy(clone, 3f);
    }

    //Fill the PickupableSprites map
    private void FillPickupableSprites() {
        PickupableSprites.Sprites.Add(PickupableEnum.HEALTH, Sprites[0]);
        PickupableSprites.Sprites.Add(PickupableEnum.MACHINE_GUN, Sprites[1]);
    }

}
