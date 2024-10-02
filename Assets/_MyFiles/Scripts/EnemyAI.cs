using System;
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
    [SerializeField] private Transform targetPos; ///position that holds other positions

    [Header("Speed Options")] 
    private NavMeshAgent _enemy_NavMeshAgent;
    private Vector3 _prevPosition;
    [SerializeField] private float _currentSpeed;

    [Header("Attack Options")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;


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
        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();

        _visionComponent = GetComponent<VisionComponent>();
        _hearingComponent = GetComponent<HearingComponent>();
        _roamingComponent = GetComponent<RoamComponent>();

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
    private void ProcessEnemyAI() ///checks conditions and sets enemy state
    {
        if (enemyState == EEnemyState.wait && !_waitTimer.IsTimerFinished())
        {
            ///Waiting based on timer component (waitTimer)
            return;
        }
        if (_visionComponent && _visionComponent.GetCurrentSensibleStimuliSetIsntEmpty())
        {
            SetEnemyState(EEnemyState.chase);
        }
        else if (_hearingComponent && _hearingComponent.GetIsAudibleNoisesPresent())
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
                if (_hearingComponent)
                { 
                    _hearingComponent.ClearAudibleNoiseInfo();
                }
                targetPos = this.transform;
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
                targetPos = _visionComponent.GetVisualTarget().transform;
                ChaseVisual();
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
        if (targetPos)
        { 
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
        }
        Vector3 currentMove = transform.position - _prevPosition;
        _currentSpeed = currentMove.magnitude/Time.deltaTime;
        _prevPosition = transform.position;
        if (_currentSpeed == 0 && enemyState != EEnemyState.wait) ///Enemy is stuck check
        {
            Debug.Log("Enemy is stuck!!!");
            targetPos = null;
            SetEnemyState(EEnemyState.wait);
        }
    }
    private void ChaseVisual() 
    {
        if (targetPos == null) { return; }


        if (IsTargetAtStoppingDistance())
        {
            Stimuli targetStimuli = targetPos.GetComponent<Stimuli>();
            if (_visionComponent.GetCanSeeVisualTarget() && !targetStimuli.GetIsChaseable())
            {
                PullPlayerFromHidingPlace();
            }
            else if (IsInAttackRange())
            {
                Attack();
            }
        }
    }
    private void PullPlayerFromHidingPlace()
    {
        PlayerControls player = targetPos.GetComponent<PlayerControls>();
        
        if (!player) 
        {
            SetEnemyState(EEnemyState.wait);
            return; 
        }

        if (player.GetIsHiding() && player.GetIsDead() == false)
        {
            IInterActions hidingInteraction = player.GetTargetInteractible().GetComponent<IInterActions>();
            hidingInteraction.OnInteraction();
        }
    }

    private bool IsInAttackRange()
    {
        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, attackRange);
        foreach (Collider hitTarget in hitTargets) 
        {
            if (hitTarget.gameObject == targetPos.gameObject)
            { 
                return true;
            }
        }
        return false;
    }
    private void Attack() 
    {
        if (!targetPos) { return; }
        PlayerControls player = targetPos.GetComponent<PlayerControls>();
        if (player && !player.GetIsHideLerp() && player.GetIsDead() == false)
        {
            Debug.Log("Attack!");

            player.SetIsDead(true);
        }
    }
    private void InvestigateNoise()
    {
        ///TargetPos is determined by hearing component.
        if (IsTargetAtStoppingDistance()) 
        {
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
            _hearingComponent.ClearAudibleNoiseInfo();
        }
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (attackPoint)
        { 
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
public enum EEnemyState { wait, roam, curious, chase} ///Enemy action state
