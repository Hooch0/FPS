using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{

    public float RPMToInterval {get { return 60 / RoundsPerMinute; } }


    public string ReferenceName;
    public int Damage;

    public float HipSpread;

    public float RecoilRetentionTime = 0.5f;
    public float Kick;
    public float KickTime;
    public float ReloadTime;
    public float RoundsPerMinute;
    public FireType WeaponFireType;
    public int AmmoSize;

    public string AmmoType;

    public ArmWeaponInfoSO ArmData;

}

public enum FireType { AUTO = 0, SEMI = 1, SINGLE = 1 }
