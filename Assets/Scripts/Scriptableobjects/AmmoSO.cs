using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AmmoSO : ScriptableObject
{
    public List<AmmoData> AmmoTypes = new List<AmmoData>();
}
