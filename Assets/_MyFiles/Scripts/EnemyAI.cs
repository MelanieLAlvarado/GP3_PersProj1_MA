using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngineInternal;

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
    private bool _isWait = false;

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
    public bool GetIsWait() { return _isWait; }

    private void Start()
    {
        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();

        _visionComponent = GetComponent<VisionComponent>();
        _hearingComponent = GetComponent<HearingComponent>();
        _roamingComponent = GetComponent<RoamComponent>();

        _waitTimer = GetComponents<TimerComponent>()[0];
        _waitTimer.SetTimerMax(waitTime);

        if (_roamingComponent != null)
        {
            targetPos = _roamingComponent.GetRoamPos();
        }
        else
        {
            targetPos = new GameObject("TempPos").transform;
        }
        Wait();
    }
    private void Update()
    {
        ProcessWait();
    }
    public void Roam() 
    {
        enemyState = EEnemyState.Roam;
        if (_roamingComponent)
        {
            targetPos = _roamingComponent.Roam(targetPos); ///Add wait functionality
            GoToTarget();
        }
        else
        {
            Wait();
        }
    }
    public void Curious()
    {
        enemyState = EEnemyState.Curious;
        ///swap target to an audible sound
        targetPos = _hearingComponent.GetHearingTarget();
        InvestigateNoise();
        GoToTarget();
    }
    public void Chase()
    {
        enemyState = EEnemyState.Chase;
        if (_hearingComponent.GetIsAudibleNoisesPresent())
        {
            _hearingComponent.ClearAudibleNoiseInfo();
        }
        if (_visionComponent.GetVisualTarget())
        {
            targetPos = _visionComponent.GetVisualTarget().transform;
        }
        ChaseVisual();
        GoToTarget();
    }
    private void ProcessWait() ///checks conditions and sets enemy state
    {
        if (enemyState == EEnemyState.Wait && !_waitTimer.IsTimerFinished())
        {
            _isWait = false;
        }
    }
    private void Wait()
    {
        ///do nothing
        _isWait = true;
        enemyState = EEnemyState.Wait;
        if (_hearingComponent)
        {
            _hearingComponent.ClearAudibleNoiseInfo();
        }
        if (!targetPos)
        {
            targetPos = this.transform;
            targetPos.position = transform.position;
        }
        _waitTimer.ResetTimer();
        
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
        else
        { 
            _enemy_NavMeshAgent.destination = this.transform.position;
        }
        Vector3 currentMove = transform.position - _prevPosition;
        _currentSpeed = currentMove.magnitude/Time.deltaTime;
        _prevPosition = transform.position;
        if (_currentSpeed == 0 && IsTargetAtStoppingDistance()) ///Enemy is stuck check
        {
            Debug.Log("Enemy is stuck!!!");
            targetPos = null;
            Wait();
        }
    }
    private void ChaseVisual() 
    {
        if (targetPos == null) { return; }

        if (IsTargetAtStoppingDistance())
        {
            Stimuli targetStimuli = targetPos.GetComponent<Stimuli>();
            if (_visionComponent.GetCanSeeVisualTarget() && !targetStimuli.GetIsCurrentlyChaseable())
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
            Wait();
            return; 
        }

        if (player.GetIsHiding() && player.GetIsDead() == false)
        {
            //player.SetPrevHidePos(attackPoint.position);
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
//public enum EEnemyState { Wait, Roam, Curious, Chase} ///Enemy action state
