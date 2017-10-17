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
	
	void Update () {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Player.SetDirectionalInput(directionalInput);
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    player.OnJumpInputDown();
        //}
        //if (Input.GetKeyUp(KeyCode.Space)) {
        //    player.OnJumpInputUp();
        //}
    }
}
