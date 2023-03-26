using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using LeapInternal;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine.UI;
using Leap.Unity;


public class HandDataCollect : MonoBehaviour
{
    // Start is called before the first frame update

    public LeapXRServiceProvider leapXRServiceProvider;
    public TMPro.TMP_Text testText;

    int id;
    void Start()
    {
        //Debug.Log(controller.GetInterpolatedHeadPose());
        //Debug.Log(controller.)
        printJSON();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 50 == 0)
        {
            printJSON();
        }
    }

    private void printJSON()
    {
       // _dataFile.AutoFlush = true;
        double time = Time.realtimeSinceStartup;
        time *= 1000000;
        var stringtime = time + "";
        if(stringtime.Contains('.'))
            stringtime = stringtime.Substring(0, stringtime.IndexOf('.'));

        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string currentPath = Directory.GetCurrentDirectory();
        string arrayDataPath = currentPath + "\\Assets\\Recordings\\HandData\\time_since_start-" + stringtime + ".json";

        using (var _dataFile = new StreamWriter(Path.Combine(docPath, arrayDataPath)))
        {
            _dataFile.WriteLine("[");
            if(leapXRServiceProvider == null){
                Debug.Log("No xrProvider available");
                _dataFile.WriteLine("]");
                return;
            }
            List<Hand> hands = leapXRServiceProvider.CurrentFrame.Hands;
            if (hands.Count > 0)
            {
                
                int handCount = hands.Count;
                foreach (Hand hand in hands)
                {
                    //enum for distinguishing which hand
                    _dataFile.WriteLine("\t{");
                    _dataFile.Write("\t\t\"hand_id\":");
                    int id = hand.Id;
                    _dataFile.WriteLine(id + ",");

                    //below 3 are The direction from the palm position toward the fingers. The direction vector they are extracted is
                    //a unit vector, but I am not sure what unit these three have, wanted to print it anyway for when it is meaningful
                    //They can be used to compute the pitch and yaw angles of the palm with respect to the horizontal plane
                    float pitch = hand.Direction.Pitch;
                    _dataFile.Write("\t\t\"pitch\":");
                    _dataFile.WriteLine(pitch + ",");

                    float yaw = hand.Direction.Yaw;
                    _dataFile.Write("\t\t\"yaw\":");
                    _dataFile.WriteLine(yaw + ",");

                    float roll = hand.PalmNormal.Roll;
                    _dataFile.Write("\t\t\"roll\":");
                    _dataFile.WriteLine(roll + ",");

                    //center position of the palm in global scene (in milimeters)
                    Vector handCenter = hand.PalmPosition;
                    _dataFile.Write("\t\t\"palm_center_position\":");
                    var handCenterJ = JsonUtility.ToJson(handCenter);
                    _dataFile.WriteLine(handCenterJ + ",");

                    //rate of change of the palm position (in milimeters/second)
                    Vector handSpeed = hand.PalmVelocity;
                    _dataFile.Write("\t\t\"palm_velocity\":");
                    var handSpeedJ = JsonUtility.ToJson(handSpeed);
                    _dataFile.WriteLine(handSpeedJ + ",");

                    //estimated width of the palm when the hand is in a flat position (milimeters)
                    float handWidth = hand.PalmWidth;
                    _dataFile.Write("\t\t\"palm_width\":");
                    _dataFile.WriteLine(handWidth + ",");

                    /*
                        Basis :-
                        The orientation of the hand as a basis matrix.
                        The basis is defined as follows:
                            - xAxis Positive in the direction of the pinky
                            - yAxis Positive above the hand
                            - zAxis Positive in the direction of the wrist
                    */
                    Vector translationOfTheHand = hand.Basis.translation;
                    _dataFile.Write("\t\t\"transform_of_hand_as_basis\":");
                    var translationOfTheHandJ = JsonUtility.ToJson(translationOfTheHand);
                    _dataFile.WriteLine(translationOfTheHandJ + ",");


                    LeapQuaternion rotationOfTheHand = hand.Basis.rotation;
                    _dataFile.Write("\t\t\"rotation_of_hand_as_basis\":");
                    var rotationOfTheHandJ = JsonUtility.ToJson(rotationOfTheHand);
                    _dataFile.WriteLine(rotationOfTheHandJ + ",");

                    //The angle between the fingers and the hand of a grab hand pose (in radians)
                    float grabAngle = hand.GrabAngle; 
                    _dataFile.Write("\t\t\"grab_angle\":");
                    _dataFile.WriteLine(grabAngle + ",");

                    // The distance between the thumb and index finger of a pinch hand pose (in milimeters)
                    float pinchDistance = hand.PinchDistance;
                    _dataFile.Write("\t\t\"pinch_distance\":");
                    _dataFile.WriteLine(pinchDistance + ",");

                    // The time-filtered position of the palm (in milimeters)
                    Vector stabilizedPalmPosition = hand.StabilizedPalmPosition; 
                    _dataFile.Write("\t\t\"time_filtered_palm_position\":");
                    var stabilizedPalmPositionJ = JsonUtility.ToJson(stabilizedPalmPosition);
                    _dataFile.WriteLine(stabilizedPalmPositionJ + ",");

                    _dataFile.WriteLine("\t\t\"fingers\": [");

                    int count = 5;
                    foreach (Finger finger in hand.Fingers)
                    {
                        _dataFile.WriteLine("\t\t\t{");

                        // finger type obeying an enumeration
                        _dataFile.Write("\t\t\t\t\"finger_enum\":");
                        Leap.Finger.FingerType type = finger.Type; 
                        var typeJ = JsonUtility.ToJson(type);
                        _dataFile.WriteLine(typeJ + ",");

                        // finger direction (in unit vector) pointing in the sane direction as the tip.
                        _dataFile.Write("\t\t\t\t\"direction\":");
                        Vector direction = finger.Direction;
                        var directionJ = JsonUtility.ToJson(direction);
                        _dataFile.WriteLine(directionJ + ",");

                        // the tip position (in millimeters) from the Leap Motion origin.
                        _dataFile.Write("\t\t\t\t\"stabilized_tip_position\":");
                        Vector stabilizedTipPosition = finger.TipPosition; 
                        var stabilizedTipPositionJ = JsonUtility.ToJson(stabilizedTipPosition);
                        _dataFile.WriteLine(stabilizedTipPositionJ + ",");

                        // width of finger (in milimeters)
                        _dataFile.Write("\t\t\t\t\"width_of_finger\":");
                        float width = finger.Width; 
                        _dataFile.WriteLine(width + ",");

                        // length of the finger (in milimeters)
                        _dataFile.Write("\t\t\t\t\"length_of_finger\":");
                        float length = finger.Length; 
                        _dataFile.WriteLine(length);
                        
                        _dataFile.Write("\t\t\t}");
                        if (--count != 0) _dataFile.WriteLine(",");
                        else _dataFile.WriteLine();

                        Debug.Log("finger type is " + finger.Type);

                        //if (finger.Type == Leap.Finger.FingerType.TYPE_THUMB)
                        //    testText.text = stabilizedTipPositionJ;
                        
                    }
                    
                    if (--handCount != 0) _dataFile.WriteLine("\t\t],");
                    else _dataFile.WriteLine("\t\t]");
                }
            }

            _dataFile.WriteLine("\t}");
            _dataFile.WriteLine("]");
            _dataFile.Close();
        }

    }
}
