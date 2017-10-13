using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The script will chase the designated target until they collide
/// </summary>
public class SeekOutTarget : MonoBehaviour {

    /// <summary>
    /// What the holder chases
    /// </summary>
    public Transform Target;

    // Use this for initialization
    void Start() {
        if (Target == null) {
            Debug.LogError("Cannot find Target");
            throw new MissingReferenceException();
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
