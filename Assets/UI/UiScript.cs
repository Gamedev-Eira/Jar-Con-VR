using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//this is a really basic class
//when a UI button is pressed, it will run toScene(), and it will recieve the scene it is going to as a string argument
//that scene is then loaded

public class UiScript : MonoBehaviour
{
    public void toScene(string sceneName) {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }//end toScene
}//end class
