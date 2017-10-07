using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour {
    /// <summary>
    /// Rectangle that indicates health level
    /// </summary>
    [SerializeField]
    private RectTransform healthBarRect;
    /// <summary>
    /// Text within rectangle indicating health level
    /// </summary>
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

    /// <summary>
    /// Update health references and visual indicator
    /// </summary>
    /// <param name="_cur"></param>
    /// <param name="_max"></param>
    public void SetHealth(int _cur, int _max) {
        //Calculate percentage of max health
        float value = (float)_cur / _max;
        //TODO: Change color of bar over time
        healthBarRect.localScale = new Vector3(value, healthBarRect.localScale.y, healthBarRect.localScale.z);
        healthText.text = (_cur + "/" + _max + " HP");
    }
}
