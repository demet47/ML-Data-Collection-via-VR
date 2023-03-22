 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 using UnityEngine.SceneManagement;

public class SwitchBetweenScenes : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(SceneManager.sceneCount != SceneManager.GetActiveScene().buildIndex) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else Debug.Log("No more sample scenes left.");
    }
}
