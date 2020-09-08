using System;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IInteractable
{
    public const int RAY_MAX_RANGE = 50000;

    public Action ShotCallback { get; set; }
    public Action FinishedShootingCallback { get; set; }
    public Action ReloadCallback { get; set; }
    public bool IsReloading { get; private set; }
    public bool IsShooting { get; private set; }

    public bool CanShoot { get { return IsReloading == false && CurrentAmmo > 0 ; } }
    public bool CanReload { get { return _player.inventory.Ammo.GetAmmo(Data.AmmoType).CurrentReserveAmmo > 0; } }

    public int CurrentAmmo;

    public WeaponDataSO Data;
    public Recoil WeaponRecoil;

    public GameObject InteractableGO;
    public BoxCollider WeaponCollider;
    public Rigidbody WeaponRigidbody;

    private PlayerController _player;

    private Timer _shootDelay;
    private Timer _reloadDelay;

    private void Awake()
    {
        _shootDelay = new Timer(Data.RPMToInterval, () => { IsShooting = false; _shootDelay.Stop(); } );
        _reloadDelay = new Timer(Data.ReloadTime, () => {  ReloadWeapon(); _reloadDelay.Stop(); } );
        WeaponRecoil.Initialize(this);

    }

    private void Update()
    {
        _shootDelay.Update(Time.deltaTime);
        _reloadDelay.Update(Time.deltaTime);

        WeaponRecoil.Update(Time.deltaTime);
    }

    public string GetUIMessage(PlayerController interactor)
    {
        //return Localization.EquipWeapon(name);
        return interactor.inventory.HasEmptySlot == true ? "Press [F] to equip " + Data.ReferenceName : "Press [F] to swap " + interactor.inventory.CurrentWeapon.Data.ReferenceName + " for " + name;
    }

    public bool CanInteract(PlayerController interactor)
    {
        return true;
    }

    public void Interact(PlayerController interactor)
    {
        _player = interactor;


        _player.inventory.EquipWeapon(this);
        
        transform.parent = _player.HipPosition.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;


        WeaponCollider.enabled = false;
        WeaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        WeaponRigidbody.isKinematic = true;
        WeaponRigidbody.useGravity = false;

        InteractableGO.layer = 0x0;
        WeaponRecoil.SetRecoilTarget(_player);

    }

    public void Drop()
    {

        transform.parent = null;

        InteractableGO.layer = 0xA;

        WeaponCollider.enabled = true;
        WeaponRigidbody.isKinematic = false;
        WeaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        WeaponRigidbody.useGravity = true;
        _player = null;
        WeaponRecoil.ResetRecoilTarget();

    }

    public void Shoot()
    {
        if (CanShoot == true && IsShooting == false)
        {
            if (_player.IsAimingDownSight == true)
            {
                ShotCallback?.Invoke();
            }
            CurrentAmmo -= 1;
            HitScan();
            IsShooting = true;
            _shootDelay.Start();
        }
        else if (CanShoot == false && CanReload == true)
        {
            
            Reload();
        }
    }

    public void ShootFinished()
    {
        FinishedShootingCallback?.Invoke();
    }

    public void Reload()
    {
        ReloadCallback?.Invoke();
        IsReloading = true;
        _reloadDelay.Start();
    }

    private void ReloadWeapon()
    {

        if (IsShooting == false && IsReloading == true)
        {
            //Play animation
            int needed =  Data.AmmoSize - CurrentAmmo;

            ReserveAmmo ammo = _player.inventory.Ammo.GetAmmo(Data.AmmoType);

            if (ammo.CurrentReserveAmmo < needed)
            {
                needed = ammo.CurrentReserveAmmo;
            }

            ammo.CurrentReserveAmmo -= needed;
            CurrentAmmo += needed;

        }

        IsReloading = false;
    }

    private void HitScan()
    {
        Ray ray = _player.GetHitScanRay();
        RaycastHit hit;

        //we could do raycast all and filter out the current user.
        //or we could just use raycast for now and change later if needed.
        if (Physics.Raycast(ray, out hit, RAY_MAX_RANGE))
        {
            //Bullet holes. nuff said
            BulletHoleManager.Instance.PlaceBulletHole(hit.transform.parent, hit.point + hit.normal * 0.01f, Quaternion.LookRotation( -hit.normal));


            IDamageable target = hit.transform.GetComponent<IDamageable>();

            if (target?.Equals(null) == false)
            {
                target.TakeDamage(Data.Damage);
            }
        }


    }

}
