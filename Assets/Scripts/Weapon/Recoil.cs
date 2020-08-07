﻿using System;
using UnityEngine;

//TODO: Depth
[Serializable]
public class Recoil
{
    public enum RecoilState { Idle, Recoiling, Recovering, DelayBeforeRecovery }
    public RecoilState State;

    public float RecoilSpeed;
    public float DelayBeforeRecovery;
    public float RecoveryTime;
    public float DepthAmount;

    public RecoilPattern Pattern;
    private PlayerController _target;

    private Timer _recoverDelayTimer;
    private Timer _applyRecoilTimer;
    private Timer _recoveryTimer;


    //The euler before the first recoil is applied
    private Vector3 _startEuler;
    
    //The current euler after all of the recoil has been applied.
    private Vector3 _appliedEuler;


    //The current amount we are modifiying our rotation for recoil by.
    private Vector3 _eulerModifer;

    //The amount of recoil total added
    private Vector3 _addedRecoil;

    //The final euler used to recover from recoil.
    private Vector3 _endEuler;

    private Vector3 _changedRecoveryEuler;

    private bool _firingFinished = false;

    public void Initialize(Weapon weapon)
    {
        weapon.Shot += OnWeaponFired;
        weapon.FinishedShooting += OnWeaponFinishedFiring;


        _recoverDelayTimer = new Timer(DelayBeforeRecovery, OnDelayBeforeRecoveryFinished );
        _applyRecoilTimer = new Timer(RecoilSpeed, () => { } );
        _recoveryTimer = new Timer(RecoveryTime, () => { } );

    }

    public void SetRecoilTarget(PlayerController target)
    {
        _target = target;
    }

    public void ResetRecoilTarget()
    {
        _target = null;
    }

    public void Update(float deltaTime)
    {
        DebugOverlay.Instance.ChangeStringValue("State",State.ToString());

        _recoverDelayTimer.Update(deltaTime);
        _applyRecoilTimer.Update(deltaTime);
        _recoveryTimer.Update(deltaTime);


        if (State == RecoilState.Recoiling)
        {
            ApplyRecoil(deltaTime);   
        }
        else if (State == RecoilState.Recovering)
        {
            RecoverFromRecoil();
        }

    }

    private void ApplyRecoil(float deltaTime)
    {

        float pX = _target.GetRotation().eulerAngles.x;


        //TODO: Calculate max a
        if (Util.CompareAngles(pX,75) < Util.CompareAngles(_endEuler.x,75) )
        {
            _endEuler.x = pX;
        }

        if (_applyRecoilTimer.IsFinished == true && _firingFinished == true)
        {
            _appliedEuler = _target.GetRotation().eulerAngles;

            _changedRecoveryEuler = _target.GetRotation().eulerAngles;

            State = RecoilState.DelayBeforeRecovery;
            _applyRecoilTimer.Stop();
            _recoverDelayTimer.Start();
            return;
        }
        _eulerModifer = Vector3.Lerp(Vector3.zero, new Vector3(Pattern.CurrentPattern.XAmount, Pattern.CurrentPattern.YAmount, 0) , _applyRecoilTimer.Elapsed / _applyRecoilTimer.Goal);

        //Flip the x value only 
        _addedRecoil += new Vector3(-_eulerModifer.x,_eulerModifer.y,_eulerModifer.z) * deltaTime;

        //Apply rotation already scales by delta time 
        _target.ApplyPlayerRotation(_eulerModifer);
    }

    private void RecoverFromRecoil()
    {
        if (_recoveryTimer.IsFinished == true || _changedRecoveryEuler != _target.GetRotation().eulerAngles)
        {
            ResetToIdle();
            return;
        }

        _eulerModifer = Quaternion.Lerp(Quaternion.Euler(_appliedEuler), Quaternion.Euler(_endEuler), _recoveryTimer.Elapsed / _recoveryTimer.Goal).eulerAngles;
        _eulerModifer.x = -_eulerModifer.x;
        
        
        _target.SetPlayerRotation(_eulerModifer);
        _changedRecoveryEuler = _target.GetRotation().eulerAngles;

    }

    private void OnWeaponFired()
    {
        Pattern.ShotFired();  
        if (State == RecoilState.Idle || State == RecoilState.Recovering )
        {
            //If we are idle or recovering then this shot is a new sequence of recoil.
            //And make sure all values have been reset in case recovery was never finished.

            ResetToIdle();
            _endEuler.x = _target.GetRotation().eulerAngles.x;
            _endEuler.y = _target.GetRotation().eulerAngles.y;
            _startEuler = _target.GetRotation().eulerAngles;
        }
        else
        {
            //reset timers
            _recoverDelayTimer.Stop();
            _applyRecoilTimer.Stop();
            _recoveryTimer.Stop();
        }
        //Start to apply recoil
        _firingFinished = false;
        State = RecoilState.Recoiling;

        _applyRecoilTimer.Start();
    }

    private void OnWeaponFinishedFiring()
    {
        _firingFinished = true;
        Pattern.Reset();
    }
    
    private void OnDelayBeforeRecoveryFinished()
    {


        //'subtract' the amount of recoil added to the rotation
        //This would be the point the player started shoooing from if they never changed the rotation.
        Quaternion deducedStart = _target.GetRotation() * Quaternion.Euler(-_addedRecoil);

        //Find the difference between when we started shooting and now. 
        //This is so we know if the user changed the rotation.
        //So this is the amount the player changed
        Quaternion userDif = Quaternion.Euler(_startEuler) * Quaternion.Inverse(deducedStart);
        
        //This is our start rotation minus the amount the user added.
        Quaternion newRot = Quaternion.Euler(_startEuler) * Quaternion.Euler(_addedRecoil);



        float roundX = Mathf.Round(userDif.eulerAngles.x);
        float roundY = Mathf.Round(userDif.eulerAngles.y);

        DebugOverlay.Instance.ChangeVectorValue("Rounded Difference", new Vector3(roundX,roundY,0) );

        if (roundX > 0 && roundX <= 90)
        {
            _endEuler.x = newRot.eulerAngles.x;
        }

        if (roundY > 0 && roundY <= 90 || roundY > 90 && roundY < 360)
        {
            //Looking left/right
            _endEuler.y = _target.GetRotation().eulerAngles.y;

        }

		State = RecoilState.Recovering;
        _recoveryTimer.Start(); 

    }

    private void ResetToIdle()
    {
         //Finished recovering
         //Reset all timers
         _recoverDelayTimer.Stop();
        _applyRecoilTimer.Stop();
        _recoveryTimer.Stop();

        //Set all values back to zero
        _startEuler = Vector3.zero;
        _appliedEuler = Vector3.zero;
        _eulerModifer = Vector3.zero;
        _addedRecoil = Vector3.zero;
        _endEuler = Vector3.zero;
        _changedRecoveryEuler = Vector3.zero;


        State = RecoilState.Idle;
    }

}

[Serializable]
public class RecoilData
{
    public float XAmount;
    public float YAmount;

    public int BulletsTillFinish;

    public bool IsFinished { get { return _currentBullet == BulletsTillFinish; } }


    private int _currentBullet;

    public void Reset()
    {
        _currentBullet = 0;
    }

    public void Apply()
    {
        _currentBullet++;
    }
}

[Serializable]
public class RecoilPattern
{
    public RecoilData CurrentPattern { get { return Patterns[_currentIndex]; }}

    public RecoilData[] Patterns;

    
    private int _currentIndex;

    public void ShotFired()
    {
        CurrentPattern.Apply();

        if (CurrentPattern.IsFinished == true)
        {
            CurrentPattern.Reset();
            _currentIndex = _currentIndex == Patterns.Length - 1 ? 0 : _currentIndex + 1;
        }
    }

    public void Reset()
    {
        CurrentPattern.Reset();
        _currentIndex = 0;
    }
}