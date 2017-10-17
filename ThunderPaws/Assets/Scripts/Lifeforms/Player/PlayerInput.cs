using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {
    /// <summary>
    /// Player reference
    /// </summary>
    Player Player;

	
	void Start () {
        Player = GetComponent<Player>();
	}
	/// <summary>
    /// Get the player input and store it on the Player object
    /// </summary>
	void Update () {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Player.SetDirectionalInput(directionalInput);
        //Used to apply variable jump height
        if (Input.GetKeyUp(KeyCode.Space)) {
            Player.OnJumpInputUp();
        }
    }
}
