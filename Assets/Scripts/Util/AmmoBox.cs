using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteractable
{
    public void Interact(ICharacter character)
    {
        if (character.InventorySystem.HasWeapon == true)
        {
            character.InventorySystem.Ammo.AddAmmo(character.InventorySystem.CurrentWeapon.Data.AmmoType ,character.InventorySystem.Ammo.GetAmmo(character.InventorySystem.CurrentWeapon.Data.AmmoType).MaxAmmo);
        }
    }

    public bool CanInteract(ICharacter character)
    {
        return true;
    }

    public string GetUIMessage(ICharacter character)
    {
        return "Press [F] to Refil Ammo";
    }
}
