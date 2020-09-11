using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(ICharacter character);

    bool CanInteract(ICharacter character);

    string GetUIMessage(ICharacter character);    
}

