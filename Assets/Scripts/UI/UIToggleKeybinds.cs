using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggleKeybinds : MonoBehaviour
{
    public GameObject UIKeybinds;
    private bool _enabled = false;
    public void ToggleKeybinds()
    {
        _enabled = !_enabled;
        UIKeybinds.SetActive(_enabled);

    }
}
