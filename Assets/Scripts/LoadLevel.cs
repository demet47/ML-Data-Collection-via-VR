using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public bool loadLevel;
    public bool initialScene = true;
    public static int sceneCount = 1;
    void Start()
    {
        if (initialScene)
        {
            StartCoroutine(LoadLevelAsync());
        }
    }
    private IEnumerator LoadLevelAsync()
    {
        var progress = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while (!progress.isDone)
        {
            initialScene = false;
            yield return null;
        }

        Debug.Log("Level Loaded: 1");
    }


}
