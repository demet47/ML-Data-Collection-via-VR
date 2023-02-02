using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSwitchMaintainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("HandModels");

        if(objs.Length > 1){
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

}
