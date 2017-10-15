using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LivesCounterUi : MonoBehaviour {
    private Text _livesText;

	void Awake () {
        if (_livesText == null) {
            _livesText = GetComponent<Text>();
        }
	}
	
	void Update () {
        _livesText.text = "Lives: " + GameMaster.Instance.RemainingLives;
	}
}
