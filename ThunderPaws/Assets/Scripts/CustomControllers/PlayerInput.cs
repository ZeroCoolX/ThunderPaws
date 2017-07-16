using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public class PlayerInput : MonoBehaviour {
    PlayerController2D player;

	
	void Start () {
        player = GetComponent<PlayerController2D>();
	}
	
	
	void Update () {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        if (Input.GetKeyDown(KeyCode.Space)) {
            player.OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            player.OnJumpInputUp();
        }
    }
}
