using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

///Components for project
[RequireComponent(typeof(NoiseComponent))]
[RequireComponent(typeof(HearingComponent))]
[RequireComponent(typeof(TimerComponent))]
public class PlayerControls : MonoBehaviour
{
    [Header("Player Enum Info [READ ONLY]")]
    [SerializeField] private EEntityType entityType = EEntityType.Player;
    [SerializeField] private EPlayerState playerState;

    private PlayerInputActions _playerInputActions;
    private CharacterController _playerController;

    [Header("Movement")]
    [SerializeField] private float standingHeight;
    [SerializeField] private float sneakHeight;
    private bool _isSneaking;

    [SerializeField][Range(1.0f, 6.0f)] private float sneakSpeed = 2f;
    [SerializeField][Range(1.0f, 8.0f)] private float walkSpeed = 6f;
    [SerializeField][Range(1.0f, 10.0f)] private float sprintSpeed = 8f;
    private bool _isSprinting;
    [SerializeField] private float speed = 5f;
    private Vector3 _playerVelocity;

    private bool _isGrounded;
    private float _gravity = -9.8f;
    private bool _defaultMovement = true;

    [Header("Camera Options")]
    [SerializeField] private Camera playerCam;

    private float _xRotation = 0f;
    [SerializeField] private float xSensitivity = 30f;
    [SerializeField] private float ySensitivity = 30f;

    [Header("Interaction info")]
    [SerializeField] private GameObject targetInteractible; ///interactible player has nearby
    [SerializeField] private bool isInteracting; ///(for seeing input in editor)
    [SerializeField] private bool isHiding;     ///(for seeing hiding bool in editor)
    private Vector3 _prevHidePos;//Will be used to Lerp/Slerp player to before/after hiding

    [Header("Noise Options")] ///Will be passed onto NoiseComponent
    [SerializeField][Range(0, 50)] private float hearingThreshold = 0.0f;
    private NoiseComponent _noiseComponent;
    private float _currentMultiplier;
    [SerializeField][Range(0, 1)] private float idleMultiplier = 0f;
    [SerializeField][Range(0, 1)] private float sneakMultiplier = 0.3f;
    [SerializeField][Range(0, 1)] private float walkMultiplier = 0.5f;
    [SerializeField][Range(0, 1)] private float sprintMultiplier = 0.7f;

    private HearingComponent _hearingComponent;
    private TimerComponent _timerComponent;
    public EEntityType GetEntityType() { return entityType; }
    public EPlayerState GetPlayerState() { return playerState; }
    public GameObject GetTargetInteractible() { return targetInteractible; }
    public bool GetIsHiding() { return isHiding; }
    public Vector3 GetPrevHidePos() { return _prevHidePos; }
    public void SetPrevHidePos(Vector3 posToSet) { _prevHidePos = posToSet; }

    public void SetTargetInteractible(GameObject interactionToSet)
    {
        targetInteractible = interactionToSet;
        if (targetInteractible != null)
        {
            IInterActions interactionObj = targetInteractible.GetComponent<IInterActions>();
            UIManager uIManager = GameManager.m_Instance.GetUIManager();
            uIManager.SetInteractionText(interactionObj.GetInteractionMessage());
        }
    }
    public void ToggleIsHiding()
    {
        Debug.Log("toggle Hide!");
        isHiding = !isHiding;
        if (isHiding)
        {
            playerState = EPlayerState.hiding;
        }
        else
        {
            playerState = EPlayerState.idle;
        }
    }
    private void Start()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();

        //programmatically add playerinputactions to actions list and connect unity events??
        _playerController = GetComponent<CharacterController>();

        _noiseComponent = GetComponent<NoiseComponent>();
        _hearingComponent = GetComponent<HearingComponent>();
        _timerComponent = GetComponent<TimerComponent>();

        _timerComponent.SetTimerMax(1.0f);

        _isSneaking = false;
        _isSprinting = false;
        speed = walkSpeed;

        playerState = EPlayerState.walking;
        _currentMultiplier = walkMultiplier;

        isInteracting = false;
        isHiding = false;

        _hearingComponent.SetCanDetectSelf(true);
        _hearingComponent.SetHearingThreshold(hearingThreshold);
    }

    private void FixedUpdate()
    {
        ProcessNoise();
        if (isHiding)
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
        ///at this point, the targetinteractible should be the hiding interaction
        if (isHiding && targetInteractible.GetComponent<HidingInteraction>())
        {
            ///will change to lerp/slerp later.
            transform.position = targetInteractible.GetComponent<HidingInteraction>().GetHidePos().position;
        }
    }
    private void ProcessMovement()
    {
        if (_defaultMovement == true && !isHiding)
        {
            Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();
            if (!_isSprinting)
            {
                if (inputVector.x == 0 && inputVector.y == 0)
                {
                    playerState = EPlayerState.idle;
                }
                else if (_isSneaking) 
                {
                    playerState = EPlayerState.sneaking;
                }
                else
                {
                    playerState = EPlayerState.walking;
                }
            }
            Vector3 movementDirection = new Vector3(inputVector.x, 0, inputVector.y);
            _playerController.Move(transform.TransformDirection(movementDirection) * (speed * Time.deltaTime));

            _playerVelocity.y += Time.deltaTime * _gravity;
            if (_isGrounded && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -2f;
            }

            _playerController.Move(Time.deltaTime * _playerVelocity);
        }
    }

    private void ProcessSneak()
    {
        float heightChange;
        if (_isSneaking == true && !isHiding)
        {
            heightChange = sneakHeight;
        }
        else
        {
            heightChange = standingHeight;
        }
        if (_playerController.height != heightChange)
        {
            _playerController.height = Mathf.Lerp(_playerController.height, heightChange, sneakSpeed);
        }
    }
    private void ProcessNoisesHeard()
    {
        if (_hearingComponent && _hearingComponent.GetIsAudibleNoisesPresent())
        {///not using transform, but getting float values calculated for noisemeter
            Debug.Log("player hears a noise!!!");
            _hearingComponent.ChooseNoiseTarget();
            _hearingComponent.UpdateNoiseMeter();
            if (_timerComponent.IsTimerFinished())
            { 
                _hearingComponent.ClearAudibleLists();
                _timerComponent.ResetTimer();
            }
        }
    }
    private void ProcessNoise() 
    {
        ProcessNoisesHeard(); ///checking if there's audio and updating UI
        ProcessNoiseType(); ///checking type and setting current multiplier
        if (_noiseComponent.GetCanMakeNoise())
        {
            //Debug.Log("TRiggerNOSIel");
            _noiseComponent.TriggerNoise();
        }
        if (!_noiseComponent || _noiseComponent.GetNoiseMultiplier() == _currentMultiplier)
        {
            return;
        }
        _noiseComponent.SetNoiseMultiplier(_currentMultiplier);
    }
    private void ProcessNoiseType() 
    {
        switch (playerState)
        {
            case EPlayerState.idle:
                _currentMultiplier = idleMultiplier;
                break;
            case EPlayerState.walking:
                _currentMultiplier = walkMultiplier;
                break;
            case EPlayerState.sneaking:
                _currentMultiplier = sneakMultiplier;
                break;
            case EPlayerState.sprinting:
                _currentMultiplier = sprintMultiplier;
                break;
            case EPlayerState.hiding:
                _currentMultiplier = idleMultiplier;
                break;
        }
    }
    private void ProcessLook()
    {
        Vector2 lookVector = _playerInputActions.Player.Look.ReadValue<Vector2>();

        _xRotation -= (lookVector.y * Time.deltaTime) * ySensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        
        playerCam.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        transform.Rotate(Vector3.up * (Time.deltaTime * lookVector.x) * xSensitivity);
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed && _isSneaking == false && playerState != EPlayerState.hiding)
        {
            _isSprinting = true;
            playerState = EPlayerState.sprinting;
            speed = sprintSpeed;
        }

        if (context.canceled && _isSneaking == false && playerState != EPlayerState.hiding)
        {
            _isSprinting = false;
            playerState = EPlayerState.walking;
            speed = walkSpeed;
        }
    }

    public void Sneak(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded == true && playerState != EPlayerState.hiding)
        {
            _isSneaking = true;
            playerState = EPlayerState.sneaking;
            speed = sneakSpeed;
        }

        if (context.canceled && playerState != EPlayerState.hiding)
        {
            _isSneaking = false;
            playerState = EPlayerState.walking;
            speed = walkSpeed;
        }
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && targetInteractible) 
        {
            isInteracting = true;
            targetInteractible.GetComponent<IInterActions>().OnInteraction();///Hiding or noisemakers interactible
        }
        if (context.canceled)
        {
            isInteracting = false;
        }
    }
}
public enum EPlayerState { idle, walking, sneaking, sprinting, hiding } //might add more or convert hiding into a bool