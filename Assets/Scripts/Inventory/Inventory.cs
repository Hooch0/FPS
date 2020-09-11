using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    public Weapon CurrentWeapon {get { return _selection == 0 ? Primary : Secondary; } }
    public bool HasWeapon { get { return Primary != null || Secondary != null; } }
    public bool HasEmptySlot { get { return Primary == null || Secondary == null; } }

    public Action WeaponChanged { get; set;}

    public Weapon Primary;
    public Weapon Secondary;

    public AmmoBag Ammo;

    private PlayerController _player;

    [SerializeField]
    private int _selection = 0;

    public void Initialize(PlayerController player)
    {
        _player = player;
        Ammo.Initialize();
    }

    //Handles equiping to either an empty slot or the current weapon slot
    public void EquipWeapon(Weapon weapon)
    {
        //Check if we have an empty slot, if we do then set it to the first empty slot and switch to that slot.
        if (HasEmptySlot == true)
        {
            int select = GetEmptySlot();
            
            if (select == 0)
            {
                //Setup this weapon
                Primary = weapon;
            }
            else
            {
                Secondary = weapon;
            }
            //Switch to the new weapon selection
            SwitchToWeapon(select);
            return;
        }
        //other wise, drop the current weapon and set that slot to the new weapon
        if (_selection == 0)
        {
            //Setup this weapon
            DropWeapon(Primary);
            Primary = weapon;
        }
        else
        {
            DropWeapon(Secondary);
            Secondary = weapon;
        }
        SetActiveWeapon();

    }

    public int GetEmptySlot()
    {
        if (Primary == null)
        {
            return 0;
        }
        else if (Secondary == null)
        {
            return 1;
        }

        return -1;
    }

    public void DropCurrentWeapon()

    {
        if (_selection == 0)
        {
            DropWeapon(Primary);
            Primary = null;

            //If the secondary slot is not empty, then switch to it.
            if (Secondary != null)
            {
                SwitchToWeapon(1);
            }
        }
        else
        {
            DropWeapon(Secondary);
            Secondary = null;

            //Even if the primary is empty, switch to it.
            SwitchToWeapon(0);
        }

        if (CurrentWeapon == null)
        {
            WeaponChanged?.Invoke();
        }
    }
    
    public void AddAmmo(string type, int amount)
    {
        //Ammo.AddAmmo(type,amount);
    }

    public void RemoveAmmo(string type, int amount)
    {
       // Ammo.RemoveAmmo(type,amount);

    }

/* Utility and Item Functions
    public void AddUtility()
    {

    }

    public void RemoveUtility()
    {

    }

    public void AddItem()
    {

    }

    public void RemoveItem()
    {
        
    }
*/
    public void SwitchWeapon()
    {
        _selection = _selection == 0 ? 1 : 0;
        SetActiveWeapon();
    }

    public void SwitchToWeapon(int newSelection)
    {
        _selection = newSelection;
        SetActiveWeapon();
    }

    private void SetActiveWeapon()
    {
        if (_selection == 0)
        {
            Secondary?.gameObject.SetActive(false);
            Primary?.gameObject.SetActive(true);

        }
        else
        {
            Primary?.gameObject.SetActive(false);
            Secondary?.gameObject.SetActive(true);
        }
        WeaponChanged?.Invoke();
    }

    private void DropWeapon(Weapon weapon)
    {
        weapon.Drop();
    }

}