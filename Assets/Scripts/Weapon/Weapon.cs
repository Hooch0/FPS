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
    public bool CanReload { get { return _character.GetInventory().Ammo.GetAmmo(Data.AmmoType).CurrentReserveAmmo > 0; } }

    public int CurrentAmmo;

    public WeaponDataSO Data;
    public Recoil WeaponRecoil;

    public GameObject InteractableGO;
    public GameObject GraphicsLayer;
    public BoxCollider WeaponCollider;
    public Rigidbody WeaponRigidbody;

    private ICharacter _character;

    private Timer _shootDelay;
    private Timer _reloadDelay;

    private Timer _kickTimer;

    private float _kickZ;
    private enum KickBackState { Idle, Kickback, Reset}
    private KickBackState _kickbackState = KickBackState.Idle;

    private void Awake()
    {
        _shootDelay = new Timer(Data.RPMToInterval, () => { IsShooting = false; _shootDelay.Stop(); } );
        _reloadDelay = new Timer(Data.ReloadTime, () => {  ReloadWeapon(); _reloadDelay.Stop(); } );
        _kickTimer = new Timer(Data.KickTime, null);
        WeaponRecoil.Initialize(this);

    }

    private void Update()
    {
        _shootDelay.Update(Time.deltaTime);
        _reloadDelay.Update(Time.deltaTime);
        _kickTimer.Update(Time.deltaTime);
        WeaponRecoil.Update(Time.deltaTime);

        KickbackUpdate();
    }

    public string GetUIMessage(ICharacter character)
    {
        //return Localization.EquipWeapon(name);
        return character.GetInventory().HasEmptySlot == true ? "Press [F] to equip " + Data.ReferenceName : "Press [F] to swap " + character.GetInventory().CurrentWeapon.Data.ReferenceName + " for " + name;
    }

    public bool CanInteract(ICharacter character)
    {
        return true;
    }

    public void Interact(ICharacter character)
    {
        _character = character;


        _character.GetInventory().EquipWeapon(this);

        ChangeGraphicsLayerMask(8);
        
        transform.parent = _character.GetArmsHolder().transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;


        WeaponCollider.enabled = false;
        WeaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        WeaponRigidbody.isKinematic = true;
        WeaponRigidbody.useGravity = false;

        InteractableGO.layer = 0x0;
        WeaponRecoil.SetRecoilTarget(_character);

    }

    public void Drop()
    {

        ChangeGraphicsLayerMask(0);
        transform.parent = null;

        InteractableGO.layer = 0xA;

        WeaponCollider.enabled = true;
        WeaponRigidbody.isKinematic = false;
        WeaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        WeaponRigidbody.useGravity = true;
        _character = null;
        WeaponRecoil.ResetRecoilTarget();

    }

    public void Shoot()
    {
        if (CanShoot == true && IsShooting == false)
        {
            if (_character.IsAimingDownSight == true)
            {
                ShotCallback?.Invoke();
            }
            CurrentAmmo -= 1;
            HitScan();
            IsShooting = true;
            _shootDelay.Start();
            _kickbackState = KickBackState.Kickback;
            _kickTimer.Restart();
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

    private void KickbackUpdate()
    {
        if (_kickbackState == KickBackState.Idle)
        {
            return;
        }

        Transform arms = _character.GetArmsHolder();

        if (_kickbackState == KickBackState.Kickback)
        {
            _kickZ = Mathf.Lerp(0,Data.Kick,_kickTimer.Elapsed / _kickTimer.Goal);
            if (_kickTimer.IsFinished == true)
            {
                _kickbackState = KickBackState.Reset;
                _kickTimer.Restart();
            }
        }
        else if (_kickbackState == KickBackState.Reset)
        {
            _kickZ = Mathf.Lerp(Data.Kick,0 ,_kickTimer.Elapsed / _kickTimer.Goal);
            if (_kickTimer.IsFinished == true)
            {
                _kickbackState = KickBackState.Idle;
                _kickTimer.Stop();
            }
        }

        arms.localPosition = new Vector3(0,0,-_kickZ);

    }

    private void ReloadWeapon()
    {

        if (IsShooting == false && IsReloading == true)
        {
            //Play animation
            int needed =  Data.AmmoSize - CurrentAmmo;

            ReserveAmmo ammo = _character.GetInventory().Ammo.GetAmmo(Data.AmmoType);

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
        Ray ray = _character.GetHitScanRay();
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

    private void ChangeGraphicsLayerMask(int layer)
    {
       ChangeLayers(GraphicsLayer.transform, layer);
    }

    private void ChangeLayers(Transform root, int layer)
    {
         root.gameObject.layer = layer;

        foreach(Transform trs in root)
        {
            trs.gameObject.layer = layer;
            if (trs.childCount > 0)
            {
                ChangeLayers(trs,layer);
            }
        }
    }

}
