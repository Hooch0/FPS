using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AmmoScriptableObject : ScriptableObject
{
    public List<AmmoDataSO> AmmoTypes = new List<AmmoDataSO>();
}
