using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IInteractable
{
    public const int RAY_MAX_RANGE = 50000;

    public bool IsReloading { get; private set; }
    public bool IsShooting { get; private set; }

    public bool CanShoot { get { return IsReloading == false && CurrentAmmo > 0 && IsShooting == false; } }
    public bool CanReload { get { return _player.inventory.Ammo.GetAmmo(Data.AmmoType).CurrentReserveAmmo > 0; } }

    public int CurrentAmmo;

    public WeaponDataSO Data;

    public GameObject InteractableGO;
    public BoxCollider WeaponCollider;
    public Rigidbody WeaponRigidbody;

    private PlayerController _player;

    private Timer _shootDelay;
    private Timer _reloadDelay;

    private void Awake()
    {
        _shootDelay = new Timer(Data.RPMToInterval, () => { IsShooting = false; _shootDelay.Stop(); } );
        _reloadDelay = new Timer(Data.ReloadTime, () => { IsReloading = false; _reloadDelay.Stop(); } );
    }

    private void Update()
    {
        _shootDelay.Update(Time.deltaTime);
        _reloadDelay.Update(Time.deltaTime);
    }

    public string GetUIMessage(PlayerController interactor)
    {
        //return Localization.EquipWeapon(name);
        return interactor.inventory.HasEmptySlot == true ? "Press [F] to equip " + name : "Press [F] to swap " + interactor.inventory.CurrentWeapon + " for " + name;
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

    }

    public void Shoot()
    {
        if (CanShoot == true)
        {

            CurrentAmmo -= 1;
            HitScan();
            IsShooting = true;
            _shootDelay.Start();
        }
        else if (CanReload == true)
        {
            Reload();
        }
    }

    public void Reload()
    {
        if (IsShooting == false && IsReloading == false)
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
