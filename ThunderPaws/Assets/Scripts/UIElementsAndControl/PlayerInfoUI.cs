using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class PlayerInfoUI : MonoBehaviour {
    private Text _inforUIText;

	void Awake () {
        if (_inforUIText == null) {
            _inforUIText = GetComponent<Text>();
        }
	}
	
	void Update () {
        _inforUIText.text = "Lives: " + GameMaster.Instance.RemainingLives + 
            "\nNip Amount: " + GameMaster.Instance.NipAccumulated;
	}
}
