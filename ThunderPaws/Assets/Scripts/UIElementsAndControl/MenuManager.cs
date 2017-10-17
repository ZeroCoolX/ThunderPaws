using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public string sceneToLoadOnPlay = "Level1";

    public void StartGame() {
        SceneManager.LoadScene(sceneToLoadOnPlay);
    }

    public void QuitGame() {
        print("Quit game");
        Application.Quit();
    }
}
