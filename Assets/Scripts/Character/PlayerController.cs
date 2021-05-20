using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Create shooting range :D 

public class PlayerController : MonoBehaviour, ICharacter
{
    public static PlayerController Instance { get; private set; }

    public const bool IS_DEBUG = true;
    public Action<string> PlayerInteractable { get; set; }

    public bool IsGrounded { get { return GroundCheck(); } }
    public bool IsAimingDownSight { get; private set; }

    public float MaxVerticalLook { get { return _maxVerticalLook; } }
    public float MinVerticalLook { get { return _minVerticalLook; } }

    public ArmsInfo ArmsInformation { get { return _arms; } }
    public Inventory InventorySystem { get { return _inventory; } } 

    public Transform CameraContainer;
    public Transform Model;
    public LayerMask GroundCheckExclude;
    public CharacterController Character;
    [SerializeField] private ArmsInfo _arms;

    [Header("Movment")]
    public float Speed = 6f;
    public float StrafeSpeed = 5f;

    public float SprintMultiplier = 1.25f;
    public float WalkSpeed = 3;

    [SerializeField] private float _maxVerticalLook = 75;
    [SerializeField] private float _minVerticalLook = -75;
    public Vector2 TurnSensitivity;

    [Header("Jumping")]
    public float JumpHeight = 2;
    public float GroundCheckHeight = 1.1f;

    [Header("Interaction")]
    public float Distance = 1.5f;
    public float Radius = 0.5f;
    public LayerMask InteractionLayer;

    [Header("Aim Transition")]
    public float AimTransitionTime;
    public Transform HipPosition;
    public Transform AimDownSightsPosition;

    [Header("Inventory")]
    [SerializeField] private Inventory _inventory;

    public List<ReserveAmmo> StartingAmmo = new List<ReserveAmmo>();

    private bool _isTransitioning = false;
    private Vector3 _yawEuler;
    private Vector3 _pitchEuler;
    private Color _groundCheckColor = Color.magenta;

    private float _jumpVelocity;
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
        _arms.Initialize();
        _inventory.WeaponChanged += OnWeaponChanged;
        _inventory.Initialize(this);
        _inventory.Ammo.SetStartingAmmo(StartingAmmo);
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
                _arms.Arms.position = Vector3.Lerp (HipPosition.position, AimDownSightsPosition.position,_aimTransitionTimer.Elapsed / _aimTransitionTimer.Goal);

            }
            else
            {
                //transition to hip position.
                _arms.Arms.position = Vector3.Lerp (AimDownSightsPosition.position, HipPosition.position,_aimTransitionTimer.Elapsed / _aimTransitionTimer.Goal);
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
            _jumpVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y);
        }

        if (IsGrounded == true && _jumpVelocity < 0)
        {
            _jumpVelocity = 0;
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

        //Check to make sure the magnitude is not greater then 1
        Vector3 move = new Vector3(x,0,z);

        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        //Apply movement modifiers
        move.x *= strafe;
        move.z *= speed;

        //Apply rotation to vector so the character is moving in the direction the character is facing
        move = transform.localRotation * move;

        Character.Move(move * Time.deltaTime);

        _jumpVelocity += Physics.gravity.y * Time.deltaTime;

        Character.Move(Vector3.up * _jumpVelocity * Time.deltaTime);
    }

    private void PlayerRotation()
    {
        float x = Util.AddAngleClamp(Input.GetAxis("Mouse Y") * TurnSensitivity.x ,_pitchEuler.x,_minVerticalLook, _maxVerticalLook);

        float y = Input.GetAxis("Mouse X") * TurnSensitivity.y;

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

        if (_inventory.HasEmptySlot == false && scrollWheel != 0)
        {
            _inventory.SwitchWeapon();
        }

        if (_inventory.CurrentWeapon != null && Input.GetKeyDown(KeyCode.G))
        {
            //drop current weapon
            _inventory.DropCurrentWeapon();
        }

        if (_shootInput?.Invoke(0) == true)
        {
            _inventory.CurrentWeapon?.Shoot();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _inventory.CurrentWeapon?.ShootFinished();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _inventory.CurrentWeapon?.Reload();
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
        Vector3 offset = new Vector3(0,-0.75f,0);
        //Boxcast/spherecast
        bool check = Physics.SphereCast(offset + transform.position + Vector3.up * (Character.radius + Physics.defaultContactOffset),
         Character.radius - Physics.defaultContactOffset,
         Vector3.down,
         out hit ,GroundCheckHeight,~GroundCheckExclude);

        _groundCheckColor = check == true ? Color.magenta : Color.red;


        return check;

    }

    private void PlayerInteraction()
    {
        RaycastHit hit;
        if (Physics.SphereCast(CameraContainer.transform.position,Radius,CameraContainer.transform.forward,out hit,Distance, InteractionLayer, QueryTriggerInteraction.Collide))
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
        if (_inventory.CurrentWeapon == null)
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

    private void SetArms(ArmWeaponInfoSO data)
    {

        SetLimbData(_arms.LeftUpperArm,   data.LeftArm.UpperArmPosition,  data.LeftArm.UpperArmRotation);
        SetLimbData(_arms.LeftForearm,    data.LeftArm.ForearmPosition,   data.LeftArm.ForearmRotation);
        SetLimbData(_arms.LeftHand,       data.LeftArm.HandPosition,      data.LeftArm.HandRotation);

        SetLimbData(_arms.RightUpperArm,  data.RightArm.UpperArmPosition,  data.RightArm.UpperArmRotation);
        SetLimbData(_arms.RightForearm,   data.RightArm.ForearmPosition,   data.RightArm.ForearmRotation);
        SetLimbData(_arms.RightHand,      data.RightArm.HandPosition,      data.RightArm.HandRotation);

    }

    private void SetLimbData(Transform limb, Vector3 position, Quaternion rotation)
    {
        limb.localPosition = position;
        limb.localRotation = rotation;
    }

    private void OnWeaponChanged()
    {

        if (_inventory.HasWeapon == false)
        {
            _arms.Arms.gameObject.SetActive(false);
        }
        else
        {
            if (_inventory.CurrentWeapon.Data.WeaponFireType == 0)
            {
                _shootInput = Input.GetMouseButton;
            }
            else 
            {
                _shootInput = Input.GetMouseButtonDown;
            }


            SetArms(_inventory.CurrentWeapon.Data.ArmData);
            _arms.Arms.gameObject.SetActive(true);

        }
    }

    public void SetRotation(Vector3 euler)
    {

        float x = euler.x;

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

    public void ApplyRotation(Vector3 euler)
    {
        float x = Util.AddAngleClamp(euler.x * Time.deltaTime,_pitchEuler.x,_minVerticalLook, _maxVerticalLook);


        float y = euler.y * Time.deltaTime;

        if (x == 0 && y == 0)
        {
            return;
        }
        _pitchEuler.x += -x;
        _yawEuler.y += y;

        
        Model.localRotation = Quaternion.Euler(_yawEuler);
        CameraContainer.localRotation = Quaternion.Euler(_pitchEuler);

    }

    public Transform GetArmsHolder()
    {
        return _arms.ArmsHolder;
    }

    public Inventory GetInventory()
    {
        return _inventory;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Quaternion GetRotation()
    {
        return Quaternion.Euler(_pitchEuler.x,_yawEuler.y,0);
    }

    public Ray GetHitScanRay()
    {
        Vector3 spread = Vector3.zero;

        if (IsAimingDownSight == false)
        {
            spread = (UnityEngine.Random.insideUnitSphere*0.1f) * _inventory.CurrentWeapon.Data.HipSpread;
        }
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
        ray.direction += spread;

        return ray;
    }

    private void OnDrawGizmos()
    {
        if (IS_DEBUG == true)
        {
            Vector3 pos = transform.position + transform.up/2*-1;

            Gizmos.color = _groundCheckColor;
            Vector3 offset = new Vector3(0,-0.75f,0);
            //Gizmos.DrawSphere(offset + transform.position + Vector3.up * (Character.radius + Physics.defaultContactOffset),Character.radius - Physics.defaultContactOffset);

            for (float i = 0; i < GroundCheckHeight; i+=0.1f)
            {
                Gizmos.DrawSphere(offset + (transform.position + Vector3.up * (Character.radius + Physics.defaultContactOffset) - Vector3.up * i),Character.radius - Physics.defaultContactOffset);
            }
        }
    }

}
