using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Data : MonoBehaviour
{
    private static int saveRate = 100;

    public int gameObjectCode = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameObjectCode = DataCollector.addNewObject(this.gameObject);
        DataCollector.printData();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % saveRate == 0)
        {
            DataCollector.positions[gameObjectCode] = this.gameObject.transform.position;
            DataCollector.rotations[gameObjectCode] = this.gameObject.transform.rotation;
            DataCollector.printData();
        }
    }
}
