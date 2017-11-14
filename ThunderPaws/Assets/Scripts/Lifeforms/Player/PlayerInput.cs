using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {
    /// <summary>
    /// Player reference
    /// </summary>
    Player Player;

    private InputDevice _gamepad;
    private bool _gamepadInUse = false;


    /// <summary>
    /// Get the controller currently active - multiple devices are accessed by array indicies
    /// </summary>
    private void Awake() {
        _gamepad = InputManager.ActiveDevice;
        _gamepadInUse = (_gamepad != InputDevice.Null);
    }

    void Start () {
        Player = GetComponent<Player>();
	}
	/// <summary>
    /// Get the player input and store it on the Player object
    /// </summary>
	void Update () {
        Vector2 directionalInput;
        if(_gamepadInUse) {
            print("gamepad In use");
            directionalInput = new Vector2(_gamepad.LeftStickX, _gamepad.LeftStickY);
            Player.SetDirectionalInput(directionalInput, _gamepad);
        } else {
            print("gamepad NOT IN use");
            directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Player.SetDirectionalInput(directionalInput);
        }

        //if (_gamepad.Action1) {
        //    print("Action1");
        //}else if (_gamepad.Action2) {
        //    print("Action2");
        //} else if (_gamepad.Action3) {
        //    print("Action3");
        //} else if (_gamepad.Action4) {
        //    print("Action4");
        //}

        //Used to apply variable jump height
        if (_gamepadInUse) {
            if (_gamepad.Action1.WasReleased) {
                Player.OnJumpInputUp();
            }
        } else {
            if (Input.GetKeyUp(KeyCode.Space)) {
                Player.OnJumpInputUp();
            }
        }
    }

}
