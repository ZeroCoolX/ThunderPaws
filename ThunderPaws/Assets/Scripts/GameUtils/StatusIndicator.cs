using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour {

    [SerializeField]
    private RectTransform healthBarRect;
    [SerializeField]
    private Text healthText;

    private void Start() {
        if(healthBarRect == null) {
            Debug.LogError("No HealthBarRect found");
            throw new UnassignedReferenceException();
        }
        if (healthText == null) {
            Debug.LogError("No HealthText found");
            throw new UnassignedReferenceException();
        }
    }

    public void SetHealth(int _cur, int _max) {
        //calculate percentage of max health
        float value = (float)_cur / _max;
        //TODO: change color of bar over time
        healthBarRect.localScale = new Vector3(value, healthBarRect.localScale.y, healthBarRect.localScale.z);
        healthText.text = (_cur + "/" + _max + " HP");
    }
}
