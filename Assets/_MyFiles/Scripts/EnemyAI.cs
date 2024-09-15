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
    private TimerComponent _chaseTimer;
    [SerializeField] EEnemyState enemyState;
    [Range(0f, 10f)][SerializeField] private float waitTime = 1.5f;
    [Range(0f, 10f)][SerializeField] private float additionalChaseTime = 2f;
    [Range(1.0f, 30.0f)][SerializeField] float roamingRange = 20f;

    [Header("Manager Info [Read Only]")]
    [SerializeField] EnemyManager enemyManager;

    [Header("Position Info [Read Only]")]
    [SerializeField] GameObject playerRef;
    [SerializeField] private Transform tempCallPos;
    [SerializeField] private Transform targetPos;

    [Header("Speed Options")]
    private NavMeshAgent _enemy_NavMeshAgent;
    private Vector3 _prevPosition;
    [SerializeField] private float currentSpeed; //might use??

    ///Might move to its own script... Hearing Component

    [Header("Field of View Options")]
    [SerializeField] private float visualRadius;
    [Range(0, 360)][SerializeField] private float visualAngle;

    [SerializeField] private LayerMask visualTargetMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] [Range(0, 30)] private float playerLostCooldown;
    [SerializeField] private bool canSeePlayer;

    private bool _isPlayerLostCoolDown; //This is a cooldown after the player left FOV

    //may add a timer to follow, even after player leaves visual field...
    //and a way for the enemy to face the player when lost.
    public float GetVisualRadius() { return visualRadius; }
    public float GetVisualAngle() { return visualAngle; }
    public bool GetCanSeePlayer() { return FieldOfViewCheck(); }

    public Transform GetTargetPos() { return targetPos; }
    private void Start()
    {
        StartCoroutine(FindPlayerRef());
        tempCallPos = new GameObject("TempPos").transform;
        tempCallPos.position = transform.position;

        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();

        _noiseManager = GameManager.m_Instance.GetComponent<NoiseManager>();
        _hearingComponent = GetComponent<HearingComponent>();
        
        _waitTimer = GetComponents<TimerComponent>()[0];
        _waitTimer.SetTimerMax(waitTime);
        _waitTimer.ResetTimer();

        Debug.Log(GetComponents<TimerComponent>().Length);

     
        _chaseTimer = gameObject.AddComponent<TimerComponent>();
        
        _chaseTimer.SetTimerMax(additionalChaseTime);
        _chaseTimer.ResetTimer();

        SetEnemyState(EEnemyState.wait);
    }
    private void Update()
    {
        ///separate second part into a separate piece... (will determine if enemy chases player or not while hiding)
        ///

        if (enemyState == EEnemyState.wait && !_waitTimer.IsTimerFinished())
        {
            ///Waiting based on timer component
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
    private IEnumerator FindPlayerRef()
    {
        yield return new WaitForSeconds(0.5f);
        if (!playerRef && GameManager.m_Instance.GetPlayer() != null)
        {
            playerRef = GameManager.m_Instance.GetPlayer();
            StopCoroutine(FindPlayerRef());
        }
    }
    private void SetEnemyState(EEnemyState stateToSet)
    {
        enemyState = stateToSet;
        switch (enemyState)
        {
            case EEnemyState.wait:
                ///do nothing
                _hearingComponent.ClearAudibleLists();
                _waitTimer.ResetTimer();
                //_waitTimer.SetRunTimer(true);
                break;
            case EEnemyState.roam:
                Roam(); //Add wait functionality
                GoToTarget();
                break;
            case EEnemyState.curious:
                ///swap target to an audible sound

                //bool isOtherTarget = targetPos == playerRef.transform || targetPos == tempCallPos.transform;
                if (_hearingComponent.GetNoiseCalculatedValues().Count == 0 || targetPos == tempCallPos.transform)
                {
                    targetPos = _hearingComponent.ChooseNoiseTarget();
                    if (!targetPos)
                    {
                        targetPos = tempCallPos;
                        SetEnemyState(EEnemyState.wait);
                        //_noiseManager.ClearActiveNoiseSources();//Might swap later
                        //_hearingComponent.ClearAudibleLists();
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
                CheckHidingPlace();
                break;
        }
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
            //_enemy_NavMeshAgent.destination = _targetPos.transform.position;
        }
        else 
        {
            targetPos = tempCallPos;
            SetEnemyState(EEnemyState.wait);
        }
    }
    private void InvestigateNoise() 
    {
        Debug.Log("Investigating noise...");
        _enemy_NavMeshAgent.destination = targetPos.transform.position;
        float targetDist = Vector3.Distance(transform.position, targetPos.position);
        Debug.Log(targetDist);
        if (targetDist < _enemy_NavMeshAgent.stoppingDistance) 
        {
            _hearingComponent.ClearAudibleLists();
            _noiseManager.ClearActiveNoiseSources();
            SetEnemyState(EEnemyState.wait);
        }
    }
    private void Roam() 
    {
        if (targetPos != null)
        {
            targetPos = tempCallPos;
            float roamDist = Vector3.Distance(transform.position, targetPos.position);
            targetPos.position = new Vector3(targetPos.position.x, transform.position.y, targetPos.position.z);
            if (roamDist < _enemy_NavMeshAgent.stoppingDistance)
            {
                Vector3 point;
                if (RandomPoint(transform.position, roamingRange, out point))
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);

                    targetPos.position = new Vector3(point.x, transform.position.y, point.z);
                }
            }
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
        }
        else
        {
            targetPos = tempCallPos;
            _enemy_NavMeshAgent.destination = targetPos.transform.position;
        }
    }
    private bool FieldOfViewCheck() 
    {
        Collider[] visualChecks = Physics.OverlapSphere(transform.position, visualRadius, visualTargetMask);

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
                        return false; ///player is correctly hidden
                    }
                    //_chaseTimer.ResetTimer();
                    return true; ///if the visual target within the angle, range, and not obtructed: then chase
                }
                canSeePlayer = false;
                return false; /// The visual target is not within the angle of the FOV
            }
        }
        canSeePlayer = false;
        return false; /// There's nothing in the sphere as a visual target mask or in the angle of the FOV
    }
    private bool PlayerHiddenCheck() 
    {
        EPlayerState tempPlayerState = playerRef.GetComponent<PlayerControls>().GetPlayerState();
        if (tempPlayerState == EPlayerState.hiding)//&& _chaseTimer.IsTimerFinished()/* && !_isPlayerLostCoolDown*/) 
        {
            ///return !_chaseTimer.IsTimerFinished();
            /*if (!_canSeePlayer)
            { 
                return true; 
            }*/
            //return false;
            return true;
        }
        return false;
    }
    private void CheckHidingPlace()
    {
        //will include chasing the player for a short time after leaving the visual field
        //  and will include a way to decide to pull player out of hiding spots if the player
        //  was seen as they hid. 
    }
    private bool RandomPoint(Vector3 center, float range, out Vector3 result) 
    {
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
public enum EEnemyState { wait, roam, curious, chase}
