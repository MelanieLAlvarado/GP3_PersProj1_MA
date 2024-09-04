using System;
using System.Collections;
using System.Numerics;
using Unity.Android.Gradle.Manifest;
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
    [Header("Movement")] private PlayerInputActions _playerInputActions;
    private CharacterController _playerController;

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

    private void Start()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        //programmatically add playerinputactions to actions list and connect unity events??
        _playerController = GetComponent<CharacterController>();

        _isSneaking = false;
    }

    private void FixedUpdate()
    {
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
        CheckInteractState();
    }

    private void ProcessMovement()
    {
        if (_defaultMovement == true)
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
    }

    private void ProcessSneak()
    {
        float heightChange;
        if (_isSneaking == true)
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

    private void ProcessLook()
    {
        Vector2 lookVector = _playerInputActions.Player.Look.ReadValue<Vector2>();

        _xRotation -= (lookVector.y * Time.deltaTime) * ySensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        
        _playerCam.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        transform.Rotate(Vector3.up * (Time.deltaTime * lookVector.x) * xSensitivity);
    }

    private void CheckInteractState()
    {
        //Might do in a separate script
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed && _isSneaking == false)
        {
            _isSprinting = true;
            speed = sprintSpeed;
        }

        if (context.canceled && _isSneaking == false)
        {
            _isSprinting = false;
            speed = walkSpeed;
        }
    }

    public void Sneak(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded == true)
        {
            _isSneaking = true;
            speed = sneakSpeed;
        }

        if (context.canceled)
        {
            _isSneaking = false;
            speed = walkSpeed;
        }
    }
}

