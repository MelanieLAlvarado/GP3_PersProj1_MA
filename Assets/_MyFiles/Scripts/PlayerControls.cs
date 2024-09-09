using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControls : MonoBehaviour
{
    [SerializeField] private EEntityType _entityType = EEntityType.Player;
    [SerializeField] private EPlayerState _playerState;

    private PlayerInputActions _playerInputActions;
    private CharacterController _playerController;

    [Header("Movement")] 
    [SerializeField]private float _standingHeight;
    [SerializeField]private float _sneakHeight;
    private bool _isSneaking;
    [SerializeField] [Range(1.0f, 6.0f)] private float sneakSpeed = 2f;
    
    [SerializeField] [Range(1.0f, 8.0f)] private float walkSpeed = 6f;

    [SerializeField] [Range(1.0f, 10.0f)]private float sprintSpeed = 8f;
    private bool _isSprinting;
    [SerializeField] private float speed = 5f;
    private Vector3 playerVelocity;

    private bool _isGrounded;
    private float _gravity = -9.8f;
    private bool _defaultMovement = true;

    [Header("Camera Options")] 
    [SerializeField] private Camera _playerCam;

    private float _xRotation = 0f;
    [SerializeField] private float xSensitivity = 30f;
    [SerializeField] private float ySensitivity = 30f;
    
    [Header("Interaction info")]
    [SerializeField]private GameObject _targetInteractible;
    [SerializeField] private bool _isInteracting;
    [SerializeField] private bool _isHiding;
    private Vector3 _prevHidePos;

    [Header("Noise Options")]
    private NoiseComponent _noiseComponent;
    [SerializeField][Range(0, 1)] private float _idleMultiplier = 0f;
    [SerializeField][Range(0, 1)] private float _sneakMultiplier = 0.2f;
    [SerializeField][Range(0, 1)] private float _walkMultiplier = 0.4f;
    [SerializeField][Range(0, 1)] private float _sprintMultiplier = 0.6f;


    public EEntityType GetEntityType() { return _entityType; }
    public EPlayerState GetPlayerState() { return _playerState; }

    public GameObject GetTargetInteractible() { return _targetInteractible; }
    public bool GetIsHiding() { return _isHiding; }
    public Vector3 GetPrevHidePos() { return _prevHidePos; }
    public void SetPrevHidePos(Vector3 posToSet) { _prevHidePos = posToSet; }

    public void SetTargetInteractible(GameObject interactionToSet) 
    {
        _targetInteractible = interactionToSet;
    }
    public void ToggleIsHiding() 
    {
        _isHiding = !_isHiding;
        if (_isHiding)
        {
            _playerState = EPlayerState.hiding;
        }
        else
        {
            _playerState = EPlayerState.walking;
        }
    }
    private void Start()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        //programmatically add playerinputactions to actions list and connect unity events??
        _playerController = GetComponent<CharacterController>();

        _noiseComponent = GetComponent<NoiseComponent>();

        _isSneaking = false;
        _isSprinting = false;
        speed = walkSpeed;
        _playerState = EPlayerState.walking;


        _isInteracting = false;
        _isHiding = false;
    }

    private void FixedUpdate()
    {
        if (_isHiding) 
        {
            ProcessHide();
            return;
        }
        ProcessMovement();
        ProcessSneak();
    }

    private void Update()
    {
        _isGrounded = _playerController.isGrounded;
    }

    private void LateUpdate()
    {
        ProcessLook();
    }
    private void ProcessHide() 
    {
        //at this point, the targetinteractible should be the hiding interaction
        if (_isHiding && _targetInteractible.GetComponent<HidingInteraction>())
        {
            transform.position = _targetInteractible.GetComponent<HidingInteraction>().GetHidePos().position;
            Debug.Log("ProcessHide...");
        }
    }
    private void ProcessMovement()
    {
        if (_defaultMovement == true && !_isHiding)
        {
            Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();
            Vector3 movementDirection = new Vector3(inputVector.x, 0, inputVector.y);
            _playerController.Move(transform.TransformDirection(movementDirection) * (speed * Time.deltaTime));

            playerVelocity.y += Time.deltaTime * _gravity;
            if (_isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = -2f;
            }

            _playerController.Move(Time.deltaTime * playerVelocity);
        }
        ProcessNoise();
    }

    private void ProcessSneak()
    {
        float heightChange;
        if (_isSneaking == true && !_isHiding)
        {
            heightChange = _sneakHeight;
        }
        else
        {
            heightChange = _standingHeight;
        }
        if (_playerController.height != heightChange)
        {
            _playerController.height = Mathf.Lerp(_playerController.height, heightChange, sneakSpeed);
        }
    }

    private void ProcessNoise() 
    {
        if (!_noiseComponent)
        {
            return;
        }
        switch (_playerState)
        {
            case EPlayerState.idle:
                _noiseComponent.TriggerNoise(_idleMultiplier);
                break;
            case EPlayerState.walking:
                _noiseComponent.TriggerNoise(_walkMultiplier);
                break;
            case EPlayerState.sneaking:
                _noiseComponent.TriggerNoise(_sneakMultiplier);
                break;
            case EPlayerState.sprinting:
                _noiseComponent.TriggerNoise(_sprintMultiplier);
                break;
            case EPlayerState.hiding:
                _noiseComponent.TriggerNoise(_idleMultiplier);
                break;
        }
    }
    private void ProcessLook()
    {
        Vector2 lookVector = _playerInputActions.Player.Look.ReadValue<Vector2>();

        _xRotation -= (lookVector.y * Time.deltaTime) * ySensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        
        _playerCam.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        transform.Rotate(Vector3.up * (Time.deltaTime * lookVector.x) * xSensitivity);
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed && _isSneaking == false && _playerState != EPlayerState.hiding)
        {
            _isSprinting = true;
            _playerState = EPlayerState.sprinting;
            speed = sprintSpeed;
        }

        if (context.canceled && _isSneaking == false && _playerState != EPlayerState.hiding)
        {
            _isSprinting = false;
            _playerState = EPlayerState.walking;
            speed = walkSpeed;
        }
    }

    public void Sneak(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded == true && _playerState != EPlayerState.hiding)
        {
            _isSneaking = true;
            _playerState = EPlayerState.sneaking;
            speed = sneakSpeed;
        }

        if (context.canceled && _playerState != EPlayerState.hiding)
        {
            _isSneaking = false;
            _playerState = EPlayerState.walking;
            speed = walkSpeed;
        }
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && _targetInteractible) 
        {
            _isInteracting = true;
            _targetInteractible.GetComponent<IInterActions>().OnInteraction();
        }
        if (context.canceled)
        {
            _isInteracting = false;
        }
    }
}
public enum EPlayerState { idle, walking, sneaking, sprinting, hiding } //might add more or convert hiding into a bool