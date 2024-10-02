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

    [Header("Position Info [Read Only]")]
    [SerializeField] GameObject playerRef;
    [SerializeField] private Transform targetPos; ///position that holds other positions

    [Header("Speed Options")] 
    private NavMeshAgent _enemy_NavMeshAgent;
    private Vector3 _prevPosition;
    private float _currentSpeed;

    public EEnemyState GetEnemyState() { return enemyState; }
    public bool IsTargetPlayer() 
    {
        if (targetPos && targetPos.GetComponent<PlayerControls>())
        {
            return true;
        }
        return false; 
    }

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
                GoToTarget();
                break;
            case EEnemyState.chase:
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
        if (targetPos == null)
        {
            return;
        }
        _enemy_NavMeshAgent.destination = targetPos.transform.position;
        Vector3 currentMove = transform.position - _prevPosition;
        _currentSpeed = currentMove.magnitude/Time.deltaTime;
        _prevPosition = transform.position;
        if (_currentSpeed == 0 && enemyState != EEnemyState.wait) ///Player is stuck check
        {
            Debug.Log("Enemy is stuck!!!");
            targetPos = null;
            SetEnemyState(EEnemyState.wait);
        }
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
                Stimuli targetStimuli = targetPos.GetComponent<Stimuli>();
                if (_visionComponent.GetCanSeeVisualTarget() && !targetStimuli.GetIsChaseable())
                { 
                    PullPlayerFromHidingPlace();
                }
                else if (targetStimuli.GetIsChaseable() && targetPos == playerRef.transform)
                {
                    Attack();
                }
            }
        }
        /*else 
        {
            ///Playerlost. returns to roaming after waiting
            SetEnemyState(EEnemyState.wait);
        }*/
    }
    private void PullPlayerFromHidingPlace() ///(WIP)
    {
        Debug.Log("Player Pull out!!!");
        PlayerControls playerTargetPos = targetPos.GetComponent<PlayerControls>();
        
        if (!playerTargetPos) { return; }

        if (playerTargetPos.GetIsHiding() && playerTargetPos.GetIsDead() == false)
        {
            Debug.Log($"CAught Player!");
            playerTargetPos.SetIsDead(true);
            playerTargetPos.ToggleIsHiding();//Interact with hiding place?
        }
        Attack();
    }
    private void Attack() 
    {
        Debug.Log("Attack!");
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
