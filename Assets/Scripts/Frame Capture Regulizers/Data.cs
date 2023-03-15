using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Data : MonoBehaviour
{
    public int gameObjectCode = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameObjectCode = DataCollector.addNewObject(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        DataCollector.positions[gameObjectCode] = this.gameObject.transform.position;
        DataCollector.rotations[gameObjectCode] = this.gameObject.transform.rotation;
    }
}
