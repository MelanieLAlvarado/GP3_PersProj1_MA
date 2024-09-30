using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

///Components for project
[RequireComponent(typeof(NoiseComponent))]
[RequireComponent(typeof(HearingComponent))]
[RequireComponent(typeof(TimerComponent))]
public class PlayerControls : MonoBehaviour
{
    [SerializeField] UIManager _uIManager;
    [Header("Player Enum Info [READ ONLY]")]
    [SerializeField] private EEntityType entityType = EEntityType.Player;
    [SerializeField] private EPlayerState playerState; ///(Idle, Sneaking, Walking, Sprinting, Hiding)

    private PlayerInputActions _playerInputActions;
    private CharacterController _playerController;

    [Header("Movement")]
    [SerializeField] private float standingHeight;
    [SerializeField] private float sneakHeight;
    private bool _bIsSneaking;

    [SerializeField] private float sneakLerpSpeed = 10f;
    [SerializeField][Range(1.0f, 6.0f)] private float sneakSpeed = 2f;
    [SerializeField][Range(1.0f, 8.0f)] private float walkSpeed = 6f;
    [SerializeField][Range(1.0f, 10.0f)] private float sprintSpeed = 8f;
    private bool _bIsSprinting;
    [SerializeField] private float speed = 5f;
    private Vector3 _playerVelocity;

    private bool _bIsGrounded;
    private float _gravity = -9.8f;
    private bool _bIsDefaultMovement = true;

    private bool _isDead = false;

    [Header("Camera Options")]
    [SerializeField] private Camera playerCam;

    private float _xRotation = 0f;
    [SerializeField] private float xSensitivity = 30f;
    [SerializeField] private float ySensitivity = 30f;

    [Header("Interaction info")]
    [SerializeField] private GameObject targetInteractible; ///interactible player has nearby
    [SerializeField] private bool bIsInteracting; ///(for seeing input in editor)
    [SerializeField] private bool bIsHiding;     ///(for seeing hiding bool in editor)
    private bool _bIsHideLerping = false;
    private Transform _prevHidePos;
    [SerializeField] private float hideLerpSpeed = 2f;

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
    public bool GetIsDead() { return _isDead; }
    public void SetIsDead(bool stateToSet) {  _isDead = stateToSet; }
    public bool GetIsHiding() { return bIsHiding; }
    public bool GetIsHideLerp() { return _bIsHideLerping; }
    public Transform GetPrevHidePos() { return _prevHidePos; }
    public void SetPrevHidePos(Transform transToSet) { _prevHidePos = transToSet; }

    public void SetTargetInteractible(GameObject interactionToSet)
    {
        targetInteractible = interactionToSet;
        if (targetInteractible != null)
        {
            IInterActions interactionObj = targetInteractible.GetComponent<IInterActions>();
            _uIManager.SetInteractionText(interactionObj.GetInteractionMessage());
        }
    }
    public void ToggleIsHiding()
    {
        Debug.Log("toggle Hide!");
        if (_bIsHideLerping)
        {
            return;
        }
        bIsHiding = !bIsHiding;
        _bIsHideLerping = true;
        if (bIsHiding)
        {
            playerState = EPlayerState.hiding;
        }
        else
        {
            playerState = EPlayerState.idle;
        }
        if (GetComponent<Stimuli>())
        {
            GetComponent<Stimuli>().SetIsChaseable(!bIsHiding); ///for sense detectiblity
        }
    }
    private void Start()
    {
         _uIManager = GameManager.m_Instance.GetUIManager();

        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();

        _playerController = GetComponent<CharacterController>();

        _noiseComponent = GetComponent<NoiseComponent>();
        _hearingComponent = GetComponent<HearingComponent>();
        _timerComponent = GetComponent<TimerComponent>();

        _timerComponent.SetTimerMax(1.0f);

        GetComponent<Stimuli>().SetIsChaseable(true);

        _bIsSneaking = false;
        _bIsSprinting = false;
        speed = walkSpeed;

        playerState = EPlayerState.idle;
        _currentMultiplier = walkMultiplier;

        bIsInteracting = false;
        bIsHiding = false;

        _hearingComponent.SetHearingThreshold(hearingThreshold);
    }

    private void FixedUpdate()
    {
        /*if (_prevHidePos)
        { 
            Debug.Log($"prev hide pos: { _prevHidePos.position}");
        }*/
        ProcessNoise();
        if (_bIsHideLerping) 
        {
            ProcessHide();
        }
        ProcessMovement();
        ProcessSneak();
    }

    private void Update()
    {
        _bIsGrounded = _playerController.isGrounded;
    }

    private void LateUpdate()
    {
        ProcessLook();
    }
    private void ProcessHide()
    {
        ///at this point, the targetinteractible should be the hiding interaction
        Transform hidePos = targetInteractible.GetComponent<HidingInteraction>().GetHidePos();
        Transform startPos;
        Transform endPos;
        if (bIsHiding)  ///will change to lerp/slerp later.
        {
            startPos = _prevHidePos;
            endPos = hidePos;
        }
        else
        {
            startPos = hidePos;
            endPos = _prevHidePos;
        }
        if (transform.position != endPos.position)
        {
            transform.position = Vector3.Slerp(startPos.position, endPos.position, Time.deltaTime * hideLerpSpeed);
        }
        else
        {
            _bIsHideLerping = false;
        }
        Debug.Log($"Hide lerp is: {_bIsHideLerping}");
    }
    private void ProcessMovement()
    {
        if (_bIsDefaultMovement == true && !bIsHiding)
        {
            Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();
            ProcessMovementType(inputVector);
            
            Vector3 movementDirection = new Vector3(inputVector.x, 0, inputVector.y);
            _playerController.Move(transform.TransformDirection(movementDirection) * (speed * Time.deltaTime));

            _playerVelocity.y += Time.deltaTime * _gravity;
            if (_bIsGrounded && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -2f;
            }

            _playerController.Move(Time.deltaTime * _playerVelocity);
        }
    }
    private void ProcessMovementType(Vector3 inputVector) 
    {
        if (inputVector.x == 0.0f && inputVector.y == 0.0f)
        {
            playerState = EPlayerState.idle;
        }
        else
        {
            if (_bIsSprinting)
            {
                playerState = EPlayerState.sprinting;
            }
            else if (_bIsSneaking)
            {
                playerState = EPlayerState.sneaking;
            }
            else
            {
                playerState = EPlayerState.walking;
            }
        }
    }
    private void ProcessSneak()
    {
        float heightChange;
        if (_bIsSneaking == true && !bIsHiding)
        {
            heightChange = sneakHeight;
        }
        else
        {
            heightChange = standingHeight;
        }
        if (_playerController.height != heightChange)
        {
            _playerController.height = Mathf.Lerp(_playerController.height, heightChange, Time.deltaTime * sneakLerpSpeed);
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
    private void ProcessNoise() 
    {
        ProcessNoiseType(); ///checking type and setting current multiplier        
        UpdateNoiseMeter(); ///checking if there's audio and updating UI
        if (_noiseComponent)
        { 
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
    private void UpdateNoiseMeter()
    {
        if (_hearingComponent && _uIManager)
        {///not using transform, but getting float values calculated for noisemeter
            float hearingTargetNoise = _hearingComponent.GetTargetNoiseCalculatedValue();
            _uIManager.UpdateNoiseMeterUI(hearingTargetNoise);
        }
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed && _bIsSneaking == false && playerState != EPlayerState.hiding)
        {
            _bIsSprinting = true;
            speed = sprintSpeed;
        }

        if (context.canceled && _bIsSneaking == false && playerState != EPlayerState.hiding)
        {
            _bIsSprinting = false;
            speed = walkSpeed;
        }
    }

    public void Sneak(InputAction.CallbackContext context)
    {
        if (context.performed && _bIsGrounded == true && playerState != EPlayerState.hiding)
        {
            _bIsSneaking = true;
            speed = sneakSpeed;
        }

        if (context.canceled && playerState != EPlayerState.hiding)
        {
            _bIsSneaking = false;
            speed = walkSpeed;
        }
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && targetInteractible) 
        {
            bIsInteracting = true;
            targetInteractible.GetComponent<IInterActions>().OnInteraction();///Hiding or noisemakers interactible
        }
        if (context.canceled)
        {
            bIsInteracting = false;
        }
    }
}
public enum EPlayerState { idle, walking, sneaking, sprinting, hiding } //might add more or convert hiding into a bool