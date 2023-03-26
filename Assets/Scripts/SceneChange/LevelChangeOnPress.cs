using System.Collections;
using UnityEngine;
using Leap.Unity.Interaction;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelChangeOnPress : InteractionButton
{
    // Start is called before the first frame update

    public static int globalCurrentSceneTracker = 1;
    public static float availableAt = 0;
    private bool initialScene = true;


    protected override void Start()
    {
        if (initialScene)
        {
            StartCoroutine(LoadFirstSceneAsync());
        }
    }



    protected override void OnTriggerEnter(Collider collider)
    {
        if (Time.time > availableAt) {
            if ((SceneManager.sceneCountInBuildSettings > globalCurrentSceneTracker+1))
            {
                if(globalCurrentSceneTracker+1 != 1) StartCoroutine(RemoveSceneAsync(globalCurrentSceneTracker));
                Wait(5);
                StartCoroutine(LoadSceneAsync(globalCurrentSceneTracker+1));
                Wait(5);
            }
            else Debug.Log("No more sample scenes left.");
        }
        availableAt = Time.time + 3;
    }



    //BELOW ARE COROUTINES FOR SCENE LOADING-REMOVAL


    private IEnumerator LoadFirstSceneAsync()
    {
        var progress = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while (!progress.isDone)
        {
            initialScene = false;
            yield return null;
        }

        Debug.Log("Level Loaded: 1");
    }

    

    private IEnumerator LoadSceneAsync(int levelName)
    {
        //BEWARE below two lines may cause run time error
        var progress = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        GameObject.Find("Camera/Multi Line TextMesh Pro").GetComponent<TextMeshPro>().text = StaticTextData.directiveText[globalCurrentSceneTracker];

        Debug.Log("Scene Loaded: " + (globalCurrentSceneTracker+1));
        ++globalCurrentSceneTracker;
        while (!progress.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator RemoveSceneAsync(int scene)
    {
        var progress = SceneManager.UnloadSceneAsync(scene);
        Debug.Log("Scene Unloaded: "+ (scene));

        while (!progress.isDone)
        {
            yield return null;
        }
    }

    IEnumerator Wait(int seconds){
        yield return new WaitForSeconds(seconds);
    }

}