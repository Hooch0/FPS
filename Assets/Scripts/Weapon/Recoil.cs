using System;
using UnityEngine;

/*Recoil System:
    * Should be a 1 class fits all type of system.
    * Does not use animations for any recoil movement. Fully code driven. 
    * Should be usable by either AI or Player without any changes in code. So generic.
    * Should have the option to be disabled.
    * Should have the ability to be modified during runtime.
    * After weapon has not been fired for X amount of seconds, rotation should attempt to reset back to when started
    * State machine to help make code more readable


    Notes:
        - After firing is complete, the weapon should recovery back to the lowest point the weapon was pointing at during shooting.
            e.g if shooting at x0.0 and weapon jumps to x-2.0, then weapon should recover to x0.0. 
                if shooting at x0.0 and weapon jumps to x-2.0, then weapon is forced to x1.5 but jumps more due to recoil, then weapon should recover to x1.5. 


        Recovery Point Rules:
        Keeps track of player moved rotation in a value called recalRot.
        1: recalRot set to the rotation before the first recoil amount is applied. -initial shot point
        2: recalRot euler x value is set to the lowest x value in 360 degree format (never surpasing 360, loops back to 0) 
        3: if user looks up, recalRot is either added the amount in the euler x value, or is set to the point.


        Keep value of player rot when first started shooting, at each addition to recal, keep track of how much in what direction WITH DELTATIME, then at the end
            take the new player rot, minus the addition and minus the old to get the player changed rotation. Add this amount to the start rotation

        Y Recovery Notes:
            We only care about backtracking the amount of y added by recoil.
                So we dont care how much the player changes it, we only remove the amount added by recoil.

    TODO:
        Recoil Recovery:
            [DONE]      Goes to the amount moved if the user looks up while recoiling
            [DONE]      Goes to the lowest point the user looks while recoiling
            [DONE]      Setup Y recovery
            [DONE]      Flick Bug
            [SKIP]      Remove lowest x from "ApplyRecoil" and have it caculated entirely in OnDelayBeforeRecoveryFinished  

            [WORKING]   Discus the previous TODO with others as well as the need for a state machine (might not be needed after all) 
            [WORKING]   Refactor/Cleanup
            [WORKING]   Weird interaction when shooting. acts like applied recoil is being multiplies by itself but is not consistent enough to debug properly.
*/
[Serializable]
public class Recoil
{
    public enum RecoilState { Idle, Recoiling, Recovering, DelayBeforeRecovery }
    public RecoilState State;

    public float RecoilSpeed;
    public float DelayBeforeRecovery;
    public float RecoveryTime;
    public RecoilData Data;
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
        _eulerModifer = Vector3.Lerp(Vector3.zero, new Vector3(Data.XAmount,Data.YAmount,0) , _applyRecoilTimer.Elapsed / _applyRecoilTimer.Goal);

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


        //DebugOverlay.Instance.ChangeVectorValue("deducedStart", deducedStart.eulerAngles);

        //DebugOverlay.Instance.ChangeVectorValue("User Difference", userDif.eulerAngles);

        float roundX = Mathf.Floor(userDif.eulerAngles.x);
        float roundY = Mathf.Floor(userDif.eulerAngles.y);


        //DebugOverlay.Instance.ChangeVectorValue("Rounded Diff", new Vector3(roundX,roundY,0));


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
    /*Fields required: 
    [Recoil Pattern | Start] - These values could be later removed and added to a type of scriptable object to create recoil patterns.
    x amount                    - weapon rotation in the x direction
    y amount                    - weapon rotation in the Y direction
    [Recoil Pattern | End]

    RecoilSpeed                 - how long it takes to reach the dsired recoil 
    depth amount                - weapon kick
    lowest x rot                - lowest point during recoil 
    recover delay
    recover time

    */
    public float XAmount;
    public float YAmount;

    public float DepthAmount;
}
