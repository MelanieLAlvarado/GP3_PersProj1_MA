using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(VisionComponent))]
[RequireComponent(typeof(HearingComponent))]
[RequireComponent(typeof(RoamComponent))]
[RequireComponent(typeof(TimerComponent))]
public class EnemyAI : MonoBehaviour
{
    private VisionComponent _visionComponent;
    private HearingComponent _hearingComponent;
    private RoamComponent _roamingComponent;

    private TimerComponent _waitTimer;
    [SerializeField] EEnemyState enemyState;

    [Header("Roam/WaitTime Options")]
    [Range(0f, 10f)][SerializeField] private float waitTime = 1.5f;

    [Header("Manager Info [Read Only]")]
    [SerializeField] EnemyManager enemyManager;

    [Header("Position Info [Read Only]")]
    [SerializeField] GameObject playerRef;
    [SerializeField] private Transform targetPos; ///position that holds other positions

    [Header("Speed Options")] 
    private NavMeshAgent _enemy_NavMeshAgent;
    private Vector3 _prevPosition;
    [SerializeField] private float currentSpeed; //might make changeable later

    ///Might move to its own script... Vision Component

    [Header("Field of View Options")]
    /*[SerializeField] private float visualRadius;
    [Range(0, 360)][SerializeField] private float visualAngle;

    [SerializeField] private LayerMask visualTargetMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] [Range(0, 30)] private float playerLostCooldown;*/
    [SerializeField] private bool bCanSeePlayer;

    //private bool _isPlayerLostCoolDown; //might removed

    private void Start()
    {
        StartCoroutine(FindPlayerRef());

        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();

        _visionComponent = GetComponent<VisionComponent>();
        _hearingComponent = GetComponent<HearingComponent>();
        _roamingComponent = GetComponent<RoamComponent>();

        //Timer to be moved out
        _waitTimer = GetComponents<TimerComponent>()[0];
        _waitTimer.SetTimerMax(waitTime);
        _waitTimer.ResetTimer();

        if (_roamingComponent != null)
        {
            targetPos = _roamingComponent.GetRoamPos();
        }
        else
        {
            targetPos = new GameObject("TempPos").transform;
        }

        SetEnemyState(EEnemyState.wait);
    }
    private void Update()
    {
        ProcessEnemyAI();
    }
    private IEnumerator FindPlayerRef()
    {
        yield return new WaitForSeconds(0.5f);
        if (!playerRef && GameManager.m_Instance.GetPlayer() != null)
        {
            playerRef = GameManager.m_Instance.GetPlayer();
            StopCoroutine(FindPlayerRef());
        }
    }
    private void ProcessEnemyAI() ///checks conditions and sets enemy state
    {
        if (enemyState == EEnemyState.wait && !_waitTimer.IsTimerFinished())
        {
            ///Waiting based on timer component (waitTimer)
            return;
        }
        //timer for when player is lost?
        if (_visionComponent.GetCurrentSensibleStimuliSetIsntEmpty())
        {
            SetEnemyState(EEnemyState.chase);
        }
        else if (_hearingComponent.GetIsAudibleNoisesPresent())
        {
            SetEnemyState(EEnemyState.curious);
        }
        else
        {
            SetEnemyState(EEnemyState.roam);
        }
    }
    private void SetEnemyState(EEnemyState stateToSet) ///determines enemy action
    {
        enemyState = stateToSet;
        switch (enemyState)
        {
            case EEnemyState.wait:
                ///do nothing
                _hearingComponent.ClearAudibleNoiseInfo();
                targetPos = _roamingComponent.GetRoamPos();
                targetPos.position = transform.position;
                _waitTimer.ResetTimer();
                break;
            case EEnemyState.roam:
                if (_roamingComponent)
                {
                    targetPos = _roamingComponent.Roam(targetPos); ///Add wait functionality
                    GoToTarget();
                }
                else
                {
                    SetEnemyState(EEnemyState.wait);
                }
                
                break;
            case EEnemyState.curious:
                ///swap target to an audible sound
                targetPos = _hearingComponent.GetHearingTarget();
                InvestigateNoise();
                if (targetPos == null)
                {
                    return;
                }
                GoToTarget();
                break;
            case EEnemyState.chase:
                //  and will include a way to decide to pull player out of hiding spots if the player
                //  was seen as they hid. 
                if (_hearingComponent.GetIsAudibleNoisesPresent())
                {
                    _hearingComponent.ClearAudibleNoiseInfo();
                }
                ChasePlayer();
                GoToTarget();
                break;
        }
    }
    private bool IsTargetAtStoppingDistance() 
    {
        float targetDist = Vector3.Distance(transform.position, _enemy_NavMeshAgent.destination);
        if (targetDist < _enemy_NavMeshAgent.stoppingDistance)
        {
            return true;
        }
        return false;
    }
    private void GoToTarget() 
    {
        _enemy_NavMeshAgent.destination = targetPos.transform.position;
        Vector3 currentMove = transform.position - _prevPosition;
        currentSpeed = currentMove.magnitude/Time.deltaTime;
        _prevPosition = transform.position;
    }
    private void ChasePlayer() 
    {
        if (targetPos != null && playerRef)
        {
            Debug.Log("Following Player!...");
            targetPos = playerRef.transform;
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
            if (IsTargetAtStoppingDistance())
            {
                if (bCanSeePlayer && PlayerHiddenCheck())
                { 
                    PullPlayerFromHidingPlace();//<-- still needs to be programmed (WIP)
                }
                else if (!PlayerHiddenCheck())
                {
                    //get player here!! (WIP)
                }
            }
        }
        else 
        {
            ///Playerlost. returns to roaming after waiting
            SetEnemyState(EEnemyState.wait);
        }
    }
    private bool PlayerHiddenCheck() ///Shorthand way of checking if player is hidden
    {
        EPlayerState tempPlayerState = playerRef.GetComponent<PlayerControls>().GetPlayerState();
        if (tempPlayerState == EPlayerState.hiding) 
        {
            return true;
        }
        return false;
    }
    private void PullPlayerFromHidingPlace() ///(WIP)
    {
        //include a way to decide to pull player out of hiding spots if the player
        //  was seen as they hid. 
    }
    private void InvestigateNoise()
    {
        Debug.Log("Investigating noise...");
        ///TargetPos is determined by hearing component.
        if (IsTargetAtStoppingDistance()) 
        {
            _hearingComponent.ClearAudibleNoiseInfo();
            NoiseComponent targetNoise = null;
            if (targetPos != null)
            {
                targetNoise = targetPos.gameObject.GetComponent<NoiseComponent>();
            }
            if (targetNoise != null)
            {
                targetNoise.SetIsTriggered(false);
                _hearingComponent.RemoveFromAudibleNoiseDict(targetPos.gameObject);
            }
            SetEnemyState(EEnemyState.wait);
        }
    }
}
public enum EEnemyState { wait, roam, curious, chase} ///Enemy action state
