﻿using System;
using UnityEngine;

[Serializable]
public class Recoil
{
    public enum RecoilState { Idle, Recoiling, Recovering, DelayBeforeRecovery }
    public RecoilState State;

    public float RecoilSpeed;
    public float DelayBeforeRecovery = 0.125f;
    public float RecoveryTime = 0.125f;
    private float RecoveryErrorMargin = 3;
    public float DepthAmount;

    public RecoilPattern RecoilPatternSystem;
    private ICharacter _character;

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
        weapon.ShotCallback += OnWeaponFired;
        weapon.FinishedShootingCallback += OnWeaponFinishedFiring;
        weapon.ReloadCallback += OnWeaponReload;

        _recoverDelayTimer = new Timer(DelayBeforeRecovery, OnDelayBeforeRecoveryFinished );
        _applyRecoilTimer = new Timer(RecoilSpeed, () => { } );
        _recoveryTimer = new Timer(RecoveryTime, () => { } );

    }

    public void SetRecoilTarget(ICharacter character)
    {
        _character = character;
    }

    public void ResetRecoilTarget()
    {
        _character = null;
        ResetToIdle();
    }

    public void Update(float deltaTime)
    {
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

        float pX = _character.GetRotation().eulerAngles.x;


        if (Util.CompareAngles(pX,_character.MaxVerticalLook) < Util.CompareAngles(_endEuler.x,_character.MaxVerticalLook) )
        {
            _endEuler.x = pX;
        }

        if (_applyRecoilTimer.IsFinished == true)
        {
            _appliedEuler = _character.GetRotation().eulerAngles;

            _changedRecoveryEuler = _character.GetRotation().eulerAngles;

            State = RecoilState.DelayBeforeRecovery;
            _applyRecoilTimer.Stop();
            _recoverDelayTimer.Start();
            return;
        }
        _eulerModifer = Vector3.Lerp(Vector3.zero, new Vector3(RecoilPatternSystem.CurrentPattern.XAmount, RecoilPatternSystem.CurrentPattern.YAmount, 0) , _applyRecoilTimer.Elapsed / _applyRecoilTimer.Goal);

        //Flip the x value only 
        _addedRecoil += new Vector3(-_eulerModifer.x,_eulerModifer.y,_eulerModifer.z) * deltaTime;

        //Apply rotation already scales by delta time 
        _character.ApplyRotation(_eulerModifer);
    }

     private void OnDelayBeforeRecoveryFinished()
    {


        //'subtract' the amount of recoil added to the rotation
        //This would be the point the player started shoooing from if they never changed the rotation.
        Quaternion deducedStart = _character.GetRotation() * Quaternion.Euler(-_addedRecoil);

        //Find the difference between when we started shooting and now. 
        //This is so we know if the user changed the rotation.
        //So this is the amount the player changed
        Quaternion userDif = Quaternion.Euler(_startEuler) * Quaternion.Inverse(deducedStart);
        
        //This is our start rotation minus the amount the user added.
        Quaternion newRot = Quaternion.Euler(_startEuler) * Quaternion.Euler(_addedRecoil);



        float roundX = Mathf.Round(userDif.eulerAngles.x);
        float roundY = Mathf.Round(userDif.eulerAngles.y);

        //convert the x value to be in a range of 0 - 180.
        //xUp would be 0,179, so the right side of a circle, where 
        //xDown would be from 180 - 360, the left side of a circle
        float xUp = roundX  - 180 < 0 ? roundX  : 0;;
        float xDown = roundX  - 180 > 0 ? 180 - (roundX  - 180) : 0;;

        //This checks for user input in the x rotation, if there is any other then micro changes, then we change the end rotation x value.
        //if we looked up, we set the new end point to be where we are currently looking.
        //if we looked down, then we set the new end point to be the current users rotation
        if (xUp > 0)
        {
            _endEuler.x = newRot.eulerAngles.x;
        }
        else if (xDown > 0)
        {
            _endEuler.x = _character.GetRotation().eulerAngles.x;
        }

        if (roundY > 0 && roundY <= 90 || roundY > 90 && roundY < 360)
        {
            //Looking left/right
            _endEuler.y = _character.GetRotation().eulerAngles.y;
        }

		State = RecoilState.Recovering;
        _recoveryTimer.Start(); 

    }

    private void RecoverFromRecoil()
    {
        if (_recoveryTimer.IsFinished == true || _changedRecoveryEuler != _character.GetRotation().eulerAngles)
        {
            ResetToIdle();
            return;
        }

        _eulerModifer = Quaternion.Lerp(Quaternion.Euler(_appliedEuler), Quaternion.Euler(_endEuler), _recoveryTimer.Elapsed / _recoveryTimer.Goal).eulerAngles;
        _eulerModifer.x = -_eulerModifer.x;
        
        
        _character.SetRotation(_eulerModifer);
        _changedRecoveryEuler = _character.GetRotation().eulerAngles;

    }

    private void OnWeaponFired()
    {
        RecoilPatternSystem.ShotFired();  
        if (State == RecoilState.Idle || State == RecoilState.Recovering )
        {
            //If we are idle or recovering then this shot is a new sequence of recoil.
            //And make sure all values have been reset in case recovery was never finished.

            ResetToIdle();
            _endEuler.x = _character.GetRotation().eulerAngles.x;
            _endEuler.y = _character.GetRotation().eulerAngles.y;
            _startEuler = _character.GetRotation().eulerAngles;
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
        RecoilPatternSystem.Reset();
    }

    private void OnWeaponReload()
    {
       ResetToIdle();
    }

   

    public void ResetToIdle()
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
        RecoilPatternSystem.Reset();
    }

}