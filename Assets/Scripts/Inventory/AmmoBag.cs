using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class AmmoBag
{

    public AmmoScriptableObject AmmoDatabase;
    private Dictionary<string,ReserveAmmo> _ammoMap = new Dictionary<string, ReserveAmmo>();

    public void Initialize()
    {
        foreach(AmmoDataSO type in AmmoDatabase.AmmoTypes)
        {
            _ammoMap.Add(type.ReferenceType, new ReserveAmmo(type.ReferenceType,0,type.MaxAmmo));
        }
    }

    public ReserveAmmo GetAmmo(string type)
    {
        return _ammoMap[type];
    }

    public bool IsFull(string type)
    {
        return _ammoMap[type].CurrentReserveAmmo == _ammoMap[type].MaxAmmo;
    }

    public bool IsEmpty(string type)
    {
        return _ammoMap[type].CurrentReserveAmmo <= 0;
    }

    public void AddAmmo(string type, int amount)
    {

        if (IsFull(type) == false)
        {
            if (amount + _ammoMap[type].CurrentReserveAmmo > _ammoMap[type].MaxAmmo)
            {
                _ammoMap[type].CurrentReserveAmmo = _ammoMap[type].MaxAmmo;
                return;
            }

            _ammoMap[type].CurrentReserveAmmo += amount;
        }

        
    }

    public void RemoveAmmo(string type, int amount)
    {

        if (IsEmpty(type) == false)
        {
            if (_ammoMap[type].CurrentReserveAmmo - amount < 0)
            {
                _ammoMap[type].CurrentReserveAmmo = 0;
                return;
            }
            _ammoMap[type].CurrentReserveAmmo -= amount;

        }
    }

    public void SetStartingAmmo(List<ReserveAmmo> reserveAmmo)
    {
        foreach(ReserveAmmo ammo in reserveAmmo)
        {
            
            if (ammo.CurrentReserveAmmo > _ammoMap[ammo.ReferenceType].MaxAmmo)
            {
                _ammoMap[ammo.ReferenceType].CurrentReserveAmmo = _ammoMap[ammo.ReferenceType].MaxAmmo;

                continue;
            }
            _ammoMap[ammo.ReferenceType].CurrentReserveAmmo = ammo.CurrentReserveAmmo;
        }
    }

}