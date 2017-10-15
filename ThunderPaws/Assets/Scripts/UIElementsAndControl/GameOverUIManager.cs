using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour {
    public string RestartScene = "CollisionPlayground";
    public string MenuScene = "MainMenu";

    public void RestartGame() {
        SceneManager.LoadScene(RestartScene);
    }

    public void GoToMenu() {
        SceneManager.LoadScene(MenuScene);
    }

    public void Quit() {
        print("We quit the game from game over");
        Application.Quit();
    }
}
