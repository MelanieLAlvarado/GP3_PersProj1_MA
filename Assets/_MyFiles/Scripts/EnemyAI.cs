using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(HearingComponent))]
[RequireComponent(typeof(TimerComponent))]
public class EnemyAI : MonoBehaviour
{
    private NoiseManager _noiseManager;
    private HearingComponent _hearingComponent;
    private TimerComponent _waitTimer;
    private TimerComponent _chaseTimer; //will be added to continue chasing the player for a short time after lost
    [SerializeField] EEnemyState enemyState;
    [Range(0f, 10f)][SerializeField] private float additionalChaseTime = 2f; //will be used to change chase timer max val

    [Header("Roam/WaitTime Options")]
    [Range(0f, 10f)][SerializeField] private float waitTime = 1.5f;
    [Range(1.0f, 30.0f)][SerializeField] float roamingRange = 20f;

    [Header("Manager Info [Read Only]")]
    [SerializeField] EnemyManager enemyManager;

    [Header("Position Info [Read Only]")]
    [SerializeField] GameObject playerRef;
    [SerializeField] private Transform tempCallPos; ///temp pos used for roaming
    [SerializeField] private Transform targetPos; ///position that holds other positions

    [Header("Speed Options")]
    private NavMeshAgent _enemy_NavMeshAgent;
    private Vector3 _prevPosition;
    [SerializeField] private float currentSpeed; //might make changeable later

    ///Might move to its own script... Vision Component

    [Header("Field of View Options")]
    [SerializeField] private float visualRadius;
    [Range(0, 360)][SerializeField] private float visualAngle;

    [SerializeField] private LayerMask visualTargetMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] [Range(0, 30)] private float playerLostCooldown;
    [SerializeField] private bool canSeePlayer;

    //private bool _isPlayerLostCoolDown; //might removed

    //  may add a timer to follow, even after player leaves visual field...
    //  and a way for the enemy to face the player when lost.


    public float GetVisualRadius() { return visualRadius; }
    public float GetVisualAngle() { return visualAngle; }
    public bool GetIsPlayerInFOV() { return FieldOfViewCheck(); }

    public Transform GetTargetPos() { return targetPos; }
    private void Start()
    {
        StartCoroutine(FindPlayerRef());

        tempCallPos = new GameObject("TempPos").transform; //for roaming. target may be set to this
        tempCallPos.position = transform.position;

        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();

        _noiseManager = GameManager.m_Instance.GetComponent<NoiseManager>();
        _hearingComponent = GetComponent<HearingComponent>();
        
        _waitTimer = GetComponents<TimerComponent>()[0];
        _waitTimer.SetTimerMax(waitTime);
        _waitTimer.ResetTimer();
    
        ///ChaseTimer is not done. but will allow the enemy to keep chasing player for a short time after lost
        _chaseTimer = gameObject.AddComponent<TimerComponent>();
        
        _chaseTimer.SetTimerMax(additionalChaseTime);
        _chaseTimer.ResetTimer();

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
        if (FieldOfViewCheck())
        {
            SetEnemyState(EEnemyState.chase);
        }
        else if (_hearingComponent.GetIsAudibleNoisesPresent())
        {
            SetEnemyState(EEnemyState.curious);
        }
        else
        {
            //add a better wait time later perhaps?
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
                tempCallPos.position = transform.position;
                targetPos = tempCallPos;
                _hearingComponent.ClearAudibleLists();
                _waitTimer.ResetTimer();
                //_waitTimer.SetRunTimer(true);
                break;
            case EEnemyState.roam:
                Roam(); ///Add wait functionality
                GoToTarget();
                break;
            case EEnemyState.curious:
                ///swap target to an audible sound

                if (_hearingComponent.GetNoiseCalculatedValues().Count == 0 || targetPos == tempCallPos.transform)
                {
                    targetPos = _hearingComponent.ChooseNoiseTarget();
                    if (targetPos == null)
                    {
                        SetEnemyState(EEnemyState.wait);//may replace with a continue chase timer
                    }
                }
                InvestigateNoise();
                GoToTarget();
                break;
            case EEnemyState.chase:
                //will evolve to include chasing the player for a short time after leaving the visual field
                //  and will include a way to decide to pull player out of hiding spots if the player
                //  was seen as they hid. 
                if (_hearingComponent.GetIsAudibleNoisesPresent())
                {
                    _hearingComponent.ClearAudibleLists(); //Might change later...
                }
                ChasePlayer();
                GoToTarget();
                break;
        }
    }
    private bool IsTargetAtStoppingDistance() 
    {
        float targetDist = Vector3.Distance(transform.position, targetPos.position);
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
        if (FieldOfViewCheck() && targetPos != null && playerRef)
        {
            Debug.Log("Following Player!...");
            targetPos = playerRef.transform;
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
            if (IsTargetAtStoppingDistance())
            {
                if (canSeePlayer && PlayerHiddenCheck())
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
            targetPos = tempCallPos; ///Playerlost. returns to roaming after waiting
            SetEnemyState(EEnemyState.wait);
        }
    }
    private bool FieldOfViewCheck()
    {
        
        Collider[] visualChecks = Physics.OverlapSphere(transform.position, visualRadius, visualTargetMask, QueryTriggerInteraction.Ignore);

        if (visualChecks.Length != 0)
        {
            Transform visualTarget = visualChecks[0].transform;
            Vector3 directionToTarget = (visualTarget.position - transform.position).normalized; ///raw direction vector
            if (Vector3.Angle(transform.forward, directionToTarget) < visualAngle / 2)
            {///  ^--- creating the FOV angle from forward vector by halfing the angle and distributing it equally on both sides

                float distanceToTarget = Vector3.Distance(transform.position, visualTarget.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    targetPos = visualTarget;
                    if (!PlayerHiddenCheck())
                    {
                        canSeePlayer = true;
                    }
                    if (canSeePlayer == false)
                    {
                        return false; ///player hid before coming inside FOV
                    }
                    return true; ///if the visual target within the angle, range, and not obtructed: then chase
                }
                canSeePlayer = false;
                return false; /// The visual target is not within the angle of the FOV
            }
        }
        canSeePlayer = false;
        return false; /// There's nothing in the sphere as a visual target mask or in the angle of the FOV
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
        _enemy_NavMeshAgent.destination = targetPos.transform.position;
        if (IsTargetAtStoppingDistance()) 
        {
            _noiseManager.ClearActiveNoiseSources();//May change
            _hearingComponent.ClearAudibleLists();
            SetEnemyState(EEnemyState.wait);
        }
    }
    private void Roam() ///Gets a random transform position and saves it on tempCallPos which is set to TargetPos
    {
        if (targetPos != null)
        {
            targetPos = tempCallPos;
            targetPos.position = new Vector3(targetPos.position.x, transform.position.y, targetPos.position.z);
            if (IsTargetAtStoppingDistance())
            {
                Vector3 point;
                if (RandomPoint(transform.position, roamingRange, out point)) ///Gets random point
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);

                    targetPos.position = new Vector3(point.x, transform.position.y, point.z);
                    ///Sets targetPos to the point x and y but the enemy's y position (might change)
                }
            }
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
        }
        else
        {
            targetPos = tempCallPos; ///if the targetPos is null it is set to the tempCallPos
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result) 
    {///Gets Random point in roamrange that is on the navmesh
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}
public enum EEnemyState { wait, roam, curious, chase} ///Enemy action state
