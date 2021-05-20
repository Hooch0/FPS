using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{

    //Adds a given value to an angle while clamping within a min max range.
    public static float AddAngleClamp(float value, float angle, float min, float max)
    {
        float val = value;


        float newAngle = angle - 180;

        float nMin = min + 180;
        float nMax = max + 180;

        if (newAngle < 0)
        {
            newAngle += 360;
        }
        
        if (val > 0 && newAngle > nMin || val < 0 && newAngle < nMax)
        {
            val = value;
        }
        else
        {
            val = 0;
        }

        return val;
    }

    //Compares the difference between 2 angles
    public static float CompareAngles(float angle1, float angle2)
    {
        return 180 - Mathf.Abs(Mathf.Abs(angle1 - angle2) - 180); 
    }

}
