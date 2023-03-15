using UnityEngine;
using System;
using System.IO;

public static class DataCollector
{
    // Start is called before the first frame update

    public static int numOfPotentialObjects = 50; //the number of objects the system is going to support. this can be increased according to needs
    //the code allows now up to 50 objects
    public static Vector3[] positions = new Vector3[numOfPotentialObjects]; //stores in meters 
    public static Quaternion[] rotations = new Quaternion[numOfPotentialObjects]; //stores in radians
    public static GameObject[] allGameObjects = new GameObject[numOfPotentialObjects];
    public static int gameObjectCount = 0;


    //below, the array info can be collected. 
    //the storage of image info can be examplified from the imagesynthesis script
    public static int addNewObject(GameObject obj)
    {
        positions[gameObjectCount] = obj.transform.position;
        rotations[gameObjectCount] = obj.transform.rotation;
        allGameObjects[gameObjectCount] = obj;
        return gameObjectCount++;
    }
}
