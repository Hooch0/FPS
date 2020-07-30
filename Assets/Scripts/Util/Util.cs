using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static float AngleClamp(float angle, float min, float max) 
    {

        if((angle %= 360) < 0)
        {
            angle += 360;
        }

        return Mathf.Clamp(angle, min, max);
    }

    public static float AddToAngleClamp(float value, float angle, float min, float max)
    {
        float x = value;


        float newAngle = angle - 180;

        if (newAngle < 0)
        {
            newAngle += 360;
        }

        if (x > 0 && newAngle > min || x < 0 && newAngle < max)
        {
            x = value;
        }
        else
        {
            x = 0;
        }

        return x;
    }

    public static float CompareAngles(float angle1, float angle2)
    {
        return 180 - Mathf.Abs(Mathf.Abs(angle1 - angle2) - 180); 
    }

}
