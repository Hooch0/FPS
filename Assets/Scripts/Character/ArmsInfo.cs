using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmsInfo
{
    public Transform Arms;
    public Transform ArmsHolder;

    public Transform LeftUpperArm { get; private set; }
    public Transform LeftForearm { get; private set; }
    public Transform LeftHand { get; private set; }
    
    public Transform RightUpperArm { get; private set; }
    public Transform RightForearm { get; private set; }
    public Transform RightHand { get; private set; }

    public void Initialize()
    {
        Transform left = null;
        Transform right = null;

        foreach(Transform trans in ArmsHolder)
        {
            if (trans.CompareTag("LeftArm"))
            {
                left = trans;
            }
            if (trans.CompareTag("RightArm"))
            {
                right = trans;
            }
        }

        foreach(Transform trans in left)
        {
            if (trans.CompareTag("UpperArm"))
            {
                LeftUpperArm = trans;
            }
            if (trans.CompareTag("Forearm"))
            {
                LeftForearm = trans;
            }
            if (trans.CompareTag("Hand"))
            {
                LeftHand = trans;
            }
        }

        foreach(Transform trans in right)
        {
            if (trans.CompareTag("UpperArm"))
            {
                RightUpperArm = trans;
            }
            if (trans.CompareTag("Forearm"))
            {
                RightForearm = trans;
            }
            if (trans.CompareTag("Hand"))
            {
                RightHand = trans;
            }
        }
    }
   
}
