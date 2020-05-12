﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public const bool IS_DEBUG = true;
    public Action<string> PlayerInteractable { get; set; }

    public bool IsGrounded { get { return GroundCheck(); } }
    public bool IsAimingDownSight { get; private set; }


    public Transform CameraContainer;
    public Transform Model;
    public LayerMask CharacterLayer;
    public CharacterController Character;


    [Header("Movment")]
    public float Speed = 6f;
    public float StrafeSpeed = 5f;

    public float SprintMultiplier = 1.25f;
    public float WalkSpeed = 3;

    public Vector2 MinMaxVerticalLook;
    public Vector2 TurnSensitivity;

    [Header("Jumping")]
    public float JumpHeight = 2;
    public float GroundCheckHeight = 1.1f;

    [Header("Interaction")]
    public float Distance = 1.5f;
    public LayerMask InteractionLayer;

    [Header("Aim Transition")]
    public float AimTransitionTime;
    public Transform HipPosition;
    public Transform AimDownSightsPosition;


    [Header("Inventory")]
    public Inventory inventory;
    
    public List<ReserveAmmo> StartingAmmo = new List<ReserveAmmo>();

    private bool _isTransitioning = false;
    private Vector3 _yawEuler;
    private Vector3 _pitchEuler;
    private Color _groundCheckColor = Color.magenta;
    

    private Vector3 _velocity;
    private Timer _aimTransitionTimer;
    private Func<int,bool> _shootInput;

    private void OnEnable()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Awake()
    {
        inventory.Initialize(this);
        inventory.Ammo.SetStartingAmmo(StartingAmmo);
        inventory.WeaponChanged += OnWeaponChanged;
        _aimTransitionTimer = new Timer(AimTransitionTime, null);
    }

    private void Update()
    {
        PlayerInput();
        PlayerInteraction();
        GroundCheck();
        AimTransitionUpdate();

        _aimTransitionTimer.Update(Time.deltaTime);
    }

    private void AimTransitionUpdate()
    {
        if (_isTransitioning == true)
        {
            _aimTransitionTimer.Start();

            if (IsAimingDownSight == true)
            {
                //tranisiton to aming down sight position.
                inventory.CurrentWeapon.transform.position = Vector3.Lerp (HipPosition.position, AimDownSightsPosition.position,_aimTransitionTimer.Elapsed / _aimTransitionTimer.Goal);

                //Lerp (Linear-interpolation)
                //Percentage of A and B [0-1] over time of t
                //Exmaple: 
                //A = 0
                //B = 10
                //t = 0.5f
                //Result = 5
            
            }
            else
            {
                //transition to hip position.
                inventory.CurrentWeapon.transform.position = Vector3.Lerp (AimDownSightsPosition.position, HipPosition.position,_aimTransitionTimer.Elapsed / _aimTransitionTimer.Goal);
            }

            if (_aimTransitionTimer.Elapsed == _aimTransitionTimer.Goal)
            {
                _isTransitioning = false;
                _aimTransitionTimer.Stop();
            }
        }

    }

    private void PlayerInput()
    {
        PlayerMovement();
        PlayerRotation();
        PlayerWeaponInput();
    }

    private void PlayerMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        float speed = Speed;
        float strafe = StrafeSpeed;

        if (IsGrounded == true && Input.GetKeyDown(KeyCode.Space))
        {
            _velocity.y = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y);
        }


        if (IsGrounded == true && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }


        if (Input.GetKey(KeyCode.LeftControl))
        {
            speed = WalkSpeed;
            strafe = WalkSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = Speed * SprintMultiplier;
        }

        _velocity.x = x * strafe;
        _velocity.z = z * speed;

        _velocity.y += Physics.gravity.y * Time.deltaTime;

        Character.Move(transform.localRotation * _velocity * Time.deltaTime);
    }

    private void PlayerRotation()
    {

        float x = ClampXAngle(Input.GetAxis("Mouse Y") * TurnSensitivity.x * Time.deltaTime);

        float y = Input.GetAxis("Mouse X") * TurnSensitivity.y * Time.deltaTime;

        if (x == 0 && y == 0)
        {
            return;
        }
        _pitchEuler.x += -x;
        _yawEuler.y += y;

        Quaternion yaw = Quaternion.Euler(_yawEuler);
        Quaternion pitch = Quaternion.Euler(_pitchEuler);


        Model.localRotation = yaw;
        CameraContainer.localRotation = pitch;

    }

    private void PlayerWeaponInput()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (inventory.HasEmptySlot == false && scrollWheel != 0)
        {
            inventory.SwitchWeapon();
        }

        if (inventory.CurrentWeapon != null && Input.GetKeyDown(KeyCode.G))
        {
            //drop current weapon
            inventory.DropCurrentWeapon();
        }

        if (_shootInput?.Invoke(0) == true)
        {
            inventory.CurrentWeapon?.Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            inventory.CurrentWeapon?.Reload();
        }

        //ADS Defualting to HOLD for now...
        if (Input.GetMouseButtonDown(1))
        {
            ChangeWeaponPosition();
        }

        if (Input.GetMouseButtonUp(1))
        {
            ChangeWeaponPosition();
        }
    }

    private bool GroundCheck()
    {

        RaycastHit hit;

        //Boxcast/spherecast
        bool check = Physics.SphereCast(transform.position +Vector3.up * (Character.radius + Physics.defaultContactOffset),
         Character.radius - Physics.defaultContactOffset,
         Vector3.down,
         out hit ,GroundCheckHeight,~CharacterLayer);

        _groundCheckColor = check == true ? Color.magenta : Color.red;


        return check;

    }

    private void PlayerInteraction()
    {
        RaycastHit hit;
        if (Physics.Raycast(CameraContainer.transform.position, CameraContainer.transform.forward,out hit,Distance, InteractionLayer, QueryTriggerInteraction.Collide))
        {

            IInteractable interactable = hit.transform.root.GetComponent<IInteractable>();

            if (interactable.CanInteract(this))
            {
                PlayerInteractable?.Invoke(interactable.GetUIMessage(this));
                if (Input.GetKeyDown(KeyCode.F))
                {
                    interactable.Interact(this);
                }
            }
            else
            {
                PlayerInteractable?.Invoke(null);
            }
        }
        else
        {
            PlayerInteractable?.Invoke(null);
        }
    }

    private void ChangeWeaponPosition()
    {
        if (inventory.CurrentWeapon == null)
        {
            IsAimingDownSight = false;
            return;
        }

        IsAimingDownSight = !IsAimingDownSight;

        if (_isTransitioning == true)
        {
            _aimTransitionTimer.SetElapsedTime(_aimTransitionTimer.Goal - _aimTransitionTimer.Elapsed);
            _aimTransitionTimer.Pause();
        }

        _isTransitioning = true;

    }

    private float ClampXAngle(float eulerX)
    {
        float x = eulerX;


        float angle = _pitchEuler.x - 180;

        if (angle < 0)
        {
            angle += 360;
        }

        if (x > 0 && angle > MinMaxVerticalLook.y || x < 0 && angle < MinMaxVerticalLook.x)
        {
            x = eulerX;
        }
        else
        {
            x = 0;
        }

        return x;
    }

    private void OnWeaponChanged()
    {
        if (inventory.CurrentWeapon.Data.WeaponFireType == 0)
        {
            _shootInput = Input.GetMouseButton;
        }
        else 
        {
            _shootInput = Input.GetMouseButtonDown;
        }
    }

        private void OnDrawGizmos()
    {
        if (IS_DEBUG == true)
        {
            //Boxcast/spherecast

        

            Vector3 pos = transform.position + transform.up/2*-1;


            Gizmos.color = _groundCheckColor;
            Gizmos.DrawSphere(transform.position +Vector3.up * (Character.radius + Physics.defaultContactOffset),Character.radius - Physics.defaultContactOffset);
        }
    }

}
