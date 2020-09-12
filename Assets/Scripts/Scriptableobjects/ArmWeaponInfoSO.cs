using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Arm Weapon Info")]
public class ArmWeaponInfoSO : ScriptableObject
{
    public ArmSOInfo LeftArm;
    public ArmSOInfo RightArm;
}
