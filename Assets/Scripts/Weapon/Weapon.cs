using System;
using System.Collections.Generic;
using UnityEngine;

/*TODO
    -Reload and drop causes error. weapon does not know it was dropped and timers are not reset
    -Switching weapons. Weapon does not know it was switched, kickback is stuck

*/

public class Weapon : MonoBehaviour, IInteractable
{
    public const int RAY_MAX_RANGE = 50000;
    public Action ShotCallback { get; set; }
    public Action FinishedShootingCallback { get; set; }
    public Action ReloadCallback { get; set; }
    public bool IsReloading { get; private set; }
    public bool IsShooting { get; private set; }

    public bool CanShoot { get { return IsReloading == false && CurrentAmmo > 0 ; } }
    public bool CanReload { get { return _character.InventorySystem.Ammo.GetAmmo(Data.AmmoType).CurrentReserveAmmo > 0; } }

    public int CurrentAmmo;

    public WeaponDataSO Data;
    public Recoil WeaponRecoil;

    public GameObject InteractableGO;
    public GameObject GraphicsLayer;
    public Collider[] WeaponColliders;
    public Rigidbody WeaponRigidbody;

    private ICharacter _character;

    private Timer _shootDelay;
    private Timer _reloadDelay;

    private Timer _kickTimer;
    private Timer _recoilResetDelay;

    private float _kickZ;
    private enum KickBackState { Idle, Kickback, Reset}
    private KickBackState _kickbackState = KickBackState.Idle;

    private void Awake()
    {
        float shotDelay = Data.RPMToInterval;
        if (Data.RPMToInterval == -1)
        {
            shotDelay = 0;
        }

        _shootDelay = new Timer(shotDelay, () => { IsShooting = false; _shootDelay.Stop(); _recoilResetDelay.Start(); } );
        _reloadDelay = new Timer(Data.ReloadTime, () => {  ReloadWeapon(); _reloadDelay.Stop(); } );
        _kickTimer = new Timer(Data.KickTime, null);
        _recoilResetDelay = new Timer(Data.RecoilRetentionTime, () => { _recoilResetDelay.Stop(); ShootFinished(); } );
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
        return character.InventorySystem.HasEmptySlot == true ? "Press [F] to equip " + Data.ReferenceName : "Press [F] to swap " + character.InventorySystem.CurrentWeapon.Data.ReferenceName + " for " + name;
    }

    public bool CanInteract(ICharacter character)
    {
        return true;
    }

    public void Interact(ICharacter character)
    {
        _character = character;


        _character.InventorySystem.EquipWeapon(this);

        ChangeGraphicsLayerMask(0x8);
        
        transform.parent = _character.ArmsInformation.ArmsHolder.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;


        foreach(Collider collider in WeaponColliders)
        {
            collider.enabled = false;
        }
        WeaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        WeaponRigidbody.isKinematic = true;
        WeaponRigidbody.useGravity = false;

        InteractableGO.layer = 0x0;
        WeaponRecoil.SetRecoilTarget(_character);

    }

    public void Drop()
    {

        ChangeGraphicsLayerMask(0x1);
        transform.parent = null;

        InteractableGO.layer = 0xA;
        ResetKickback();

        foreach(Collider collider in WeaponColliders)
        {
            collider.enabled = true;
        }
        WeaponRigidbody.isKinematic = false;
        WeaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        WeaponRigidbody.useGravity = true;
       
       
        WeaponRecoil.ResetRecoilTarget();
        _reloadDelay.Stop();
        IsReloading = false;

        _character = null;


    }

    public void Shoot()
    {
        if (CanShoot == true && IsShooting == false)
        {
            if (_character.IsAimingDownSight == true)
            {
                ShotCallback?.Invoke();
            }
            _recoilResetDelay.Stop();
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
    
    public void OnWeaponActive()
    {
        ResetKickback();
    }

    public void OnWeaponInactive()
    {
        WeaponRecoil.ResetToIdle();
        _reloadDelay.Stop();
        IsReloading = false;
    }

    private void KickbackUpdate()
    {
        if (_kickbackState == KickBackState.Idle)
        {
            return;
        }

        Transform arms = _character.ArmsInformation.ArmsHolder;

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

    private void ResetKickback()
    {
        _character.ArmsInformation.ArmsHolder.localPosition = new Vector3(0,0,0);
        _kickTimer.Stop();
        _kickbackState = KickBackState.Idle;
    }

    private void ReloadWeapon()
    {

        if (IsShooting == false && IsReloading == true)
        {
            //Play animation
            int needed =  Data.AmmoSize - CurrentAmmo;

            ReserveAmmo ammo = _character.InventorySystem.Ammo.GetAmmo(Data.AmmoType);

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
        if (Physics.Raycast(ray, out hit, RAY_MAX_RANGE,0x1,QueryTriggerInteraction.Ignore))
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
