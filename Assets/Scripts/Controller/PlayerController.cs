using System;
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

    public float MaxVerticalLook = 255;
    public float MinVerticalLook = 105;
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
        //Cursor.visible = false;
        
        _yawEuler = Model.localRotation.eulerAngles;
        _pitchEuler = CameraContainer.localRotation.eulerAngles;

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


        float x = Util.AddToAngleClamp(Input.GetAxis("Mouse Y") * TurnSensitivity.x * Time.deltaTime,_pitchEuler.x,MinVerticalLook, MaxVerticalLook);

        float y = Input.GetAxis("Mouse X") * TurnSensitivity.y * Time.deltaTime;

        if (x == 0 && y == 0)
        {
            return;
        }
        _pitchEuler.x += -x;
        _yawEuler.y += y;

        
        Model.localRotation = Quaternion.Euler(_yawEuler);
        CameraContainer.localRotation = Quaternion.Euler(_pitchEuler);

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

        if (Input.GetMouseButtonUp(0))
        {
            inventory.CurrentWeapon?.ShootFinished();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            inventory.CurrentWeapon?.Reload();
        }

        //ADS Defualting to HOLD for now...
        if (Input.GetMouseButtonDown(1))
        {
            ChangeToADSPositon();
        }

        if (Input.GetMouseButtonUp(1))
        {
            ChangeToHipPostion();
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

    private void ChangeToADSPositon()
    {
        if (inventory.CurrentWeapon == null)
        {
            IsAimingDownSight = false;
            return;
        }

        IsAimingDownSight = true;

        if (_isTransitioning == true)
        {
            _aimTransitionTimer.SetElapsedTime(_aimTransitionTimer.Goal - _aimTransitionTimer.Elapsed);
            _aimTransitionTimer.Pause();
        }

        _isTransitioning = true;
    }

    private void ChangeToHipPostion()
    {
        if (IsAimingDownSight == false)
        {
            return;
        }

        IsAimingDownSight = false;

        if (_isTransitioning == true)
        {
            _aimTransitionTimer.SetElapsedTime(_aimTransitionTimer.Goal - _aimTransitionTimer.Elapsed);
            _aimTransitionTimer.Pause();
        }

        _isTransitioning = true;
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

        _isTransitioning = false;
        _aimTransitionTimer.Stop();
        IsAimingDownSight = false;
        inventory.CurrentWeapon.transform.position = HipPosition.position;
    }

    public void SetPlayerRotation(Vector3 euler)
    {

        float x = euler.x;//ClampXAngle(euler.x);

        float y = euler.y;

        if (x == 0 && y == 0)
        {
            return;
        }
        _pitchEuler.x = -x;
        _yawEuler.y = y;

        
        Model.localRotation = Quaternion.Euler(_yawEuler);
        CameraContainer.localRotation = Quaternion.Euler(_pitchEuler);

    }

    public void ApplyPlayerRotation(Vector3 euler)
    {
        float x = Util.AddToAngleClamp(euler.x * Time.deltaTime,_pitchEuler.x,MinVerticalLook, MaxVerticalLook);


        float y = euler.y* Time.deltaTime;

        if (x == 0 && y == 0)
        {
            return;
        }
        _pitchEuler.x += -x;
        _yawEuler.y += y;

        
        Model.localRotation = Quaternion.Euler(_yawEuler);
        CameraContainer.localRotation = Quaternion.Euler(_pitchEuler);

    }

    public Quaternion GetRotation()
    {
        return Quaternion.Euler(_pitchEuler.x,_yawEuler.y,0);
    }

    public Ray GetHitScanRay()
    {
        return Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
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
