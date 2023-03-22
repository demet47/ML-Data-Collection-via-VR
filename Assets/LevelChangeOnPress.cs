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
    
    protected override void OnTriggerEnter(Collider collider)
    {
        if (Time.time > availableAt) {
            if ((SceneManager.sceneCountInBuildSettings > globalCurrentSceneTracker+1))
            {
                if(globalCurrentSceneTracker+1 != 1) StartCoroutine(RemoveSceneAsync(globalCurrentSceneTracker));
                StartCoroutine(Wait());
                StartCoroutine(LoadSceneAsync(globalCurrentSceneTracker+1));
                StartCoroutine(Wait());
            }
            else Debug.Log("No more sample scenes left.");
        }
        availableAt = Time.time + 3;
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

    private IEnumerator Wait(){
        yield return new WaitForSeconds(5);
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

}

//-316.668    -244.95     1.026