using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerController interactor);

    bool CanInteract(PlayerController interactor);

    string GetUIMessage(PlayerController interactor);    
}

