using UnityEngine;
using System;
using System.IO;

public static class DataCollector
{
    // Start is called before the first frame update

    public static int numOfPotentialObjects = 50;
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
    private static string formatToPrint<T>(T[] array)
    {
        string s = "{";
        for (int i = 0; i < array.Length - 1; i++)
            s += array[i] + " , ";
        s += array[array.Length - 1] + "}";
        return s;
    }

    // Update is called once per frame

    public static void printData()
    {
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        float time = Time.realtimeSinceStartup;

        string currentPath = Directory.GetCurrentDirectory();
        string arrayDataPath = currentPath + "\\Assets\\Recordings\\ArrayData\\time_since_start-" + time + ".json";

        using (var writer = new StreamWriter(Path.Combine(docPath, arrayDataPath)))
        {
            writer.WriteLine("[");
            for (int i = 0; i < gameObjectCount; i++)
            {
                writer.WriteLine("\t{");
                writer.Write("\t\t\"object_name\": ");
                writer.WriteLine("\"" + allGameObjects[i] + "\",");
                writer.Write("\t\t\"position_vector\": ");
                var position = JsonUtility.ToJson(positions[i]*1000); //prints in milimeters
                writer.WriteLine(position + ",");
                writer.Write("\t\t\"rotation_vector\": ");
                var rotation = JsonUtility.ToJson(rotations[i]); //prints in radians
                writer.WriteLine(rotation);
                writer.Write("\t}");
                if (i != gameObjectCount - 1) writer.WriteLine(",");
                else writer.WriteLine();
            }
            writer.WriteLine("]");
            writer.Close();
        }



    }



    ///here, from the recommendations I sent from "Ben" group, we can take the moduler of Time.frameCount
    //and cause a less frequent mining of data
    //also here we should do the same data operations as in start method so it would be nice if we would 
    //do all these in a seperate private method and call the method on these both
}
