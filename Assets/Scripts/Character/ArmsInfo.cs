using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmsInfo
{
    public Transform Arms;
    public Transform ArmsHolder;

    [Header("Left Arms")]
    public Transform LeftUpperArm;
    public Transform LeftForearm;
    public Transform LeftHand;
    
    [Header("Right Arms")]
    public Transform RightUpperArm;
    public Transform RightForearm;
    public Transform RightHand;
   
}
