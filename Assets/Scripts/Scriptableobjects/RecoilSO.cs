using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Weapon Recoil")]
public class RecoilSO : ScriptableObject
{
    public RecoilData[] RecoilPattern;
}
