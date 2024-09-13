using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TimerComponent))]
//[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    private TimerComponent _timerComponent;
    [SerializeField] EEnemyState enemyState;
    [Range(0f, 10f)][SerializeField] private float waitTime = 1.5f;
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
    [Header("Hearing Range Options")]
    [Range(1.0f, 30.0f)][SerializeField] float hearingRange = 15f;
    [Range(0f, 100f)][SerializeField] float hearingThreshold = 40f;
    //[SerializeField] private GameObject _hRange; ///visual of hearing range (temp)

    [SerializeField] private List<GameObject> noiseObjsInRange = new List<GameObject>();
    [SerializeField] private List<GameObject> audibleNoiseList = new List<GameObject>();
    [SerializeField] private List<float> noiseCalculatedValues = new List<float>();

    [Header("Field of View Options")]
    [SerializeField] private float visualRadius;
    [Range(0, 360)][SerializeField] private float visualAngle;

    [SerializeField] private LayerMask visualTargetMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] [Range(0, 30)] private float playerLostCooldown;
    private bool _canSeePlayer;
    private bool _isPlayerLostCoolDown; //This is a cooldown after the player left FOV

    //may add a timer to follow, even after player leaves visual field...
    //and a way for the enemy to face the player when lost.
    public float GetVisualRadius() { return visualRadius; }
    public float GetVisualAngle() { return visualAngle; }
    public bool GetCanSeePlayer() { return FieldOfViewCheck(); }

    public Transform GetTargetPos() { return targetPos; }
    public List<GameObject> GetNoisesObjsInRangeList() { return noiseObjsInRange; }
    public void AddTriggeredNoiseToList(GameObject noiseToAdd) 
    {
        if (audibleNoiseList.Contains(noiseToAdd))
        {
            Debug.Log("This obj is already in list!");
            return; 
        }
        Debug.Log("Noise has been added...");
        audibleNoiseList.Add(noiseToAdd); 
    }
    private void Start()
    {
        StartCoroutine(FindPlayerRef());
        tempCallPos = new GameObject("TempPos").transform;
        tempCallPos.position = transform.position;

        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();

        _timerComponent = GetComponent<TimerComponent>();
        _timerComponent.SetTimerMax(waitTime);
        _timerComponent.ResetTimer();

        SetEnemyState(EEnemyState.wait);
        //StartCoroutine(EnemyIntellegence());
    }
    private void Update()
    {
        ///separate second part into a separate piece... (will determine if enemy chases player or not while hiding)
        ///

        if (enemyState == EEnemyState.wait && !_timerComponent.IsTimerFinished())
        {
            ///Waiting based on timer component
            return;
        }

        //timer for when player is lost?
        if (FieldOfViewCheck() && !PlayerHiddenCheck())
        {
            SetEnemyState(EEnemyState.chase);
        }
        else if (audibleNoiseList.Count > 0)
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
                _timerComponent.ResetTimer();
                break;
            case EEnemyState.roam:
                Roam(); //Add wait functionality
                GoToTarget();
                break;
            case EEnemyState.curious:
                //swap target to an audible sound
                if (noiseCalculatedValues.Count == 0 || targetPos == playerRef.transform)//separate into hearing component??
                {
                    CalculateSoundValues();
                    ChooseNoiseTarget();
                    //HearingComponent hearingComponent = GetComponent<HearingComponent>()
                    //_noiseCalculatedValues = CalculateSoundValues(_audibleNoiseList, _hearingRange)
                    //_targetPos = ChooseNoiseTarget(_audibleNoiseList, _noiseCalculatedValues);

                }
                InvestigateNoise();
                GoToTarget();
                break;
            case EEnemyState.chase:
                //will evolve to include chasing the player for a short time after leaving the visual field
                //  and will include a way to decide to pull player out of hiding spots if the player
                //  was seen as they hid. 
                
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
    private void CheckHidingPlace() 
    {
        //will include chasing the player for a short time after leaving the visual field
        //  and will include a way to decide to pull player out of hiding spots if the player
        //  was seen as they hid. 
    }
    private void CalculateSoundValues()
    {
        if (audibleNoiseList.Count > 0) //might find a more understandable way to gauge sound value later...
        {
            GameObject noiseTemp = audibleNoiseList[0];
            for (int i = 0; i < audibleNoiseList.Count; i++) //calculate sound values
            {
                float iDistance = Vector3.Distance(transform.position, audibleNoiseList[i].transform.position);

                float distMultiplier = iDistance / hearingRange;

                NoiseComponent noiseComp = audibleNoiseList[i].GetComponent<NoiseComponent>();
                noiseCalculatedValues.Add(noiseComp.GetRawSoundAmount() * distMultiplier);
            }
        }
    }
    private void ChooseNoiseTarget()
    {
        //iterates through the list to find the highest noise value
        int index = 0;
        float noiseNum = noiseCalculatedValues[index];
        targetPos = audibleNoiseList[index].transform;
        for (int i = 1; i < audibleNoiseList.Count; i++) ///choose the sound with the highest noise
        {
            float iNoiseNum = noiseCalculatedValues[i];
            if (noiseNum <= iNoiseNum)
            {
                noiseNum = iNoiseNum;
                index = i;
            }
            targetPos = audibleNoiseList[index].transform;
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
            audibleNoiseList.Clear();
            noiseCalculatedValues.Clear();
            SetEnemyState(EEnemyState.wait);
        }
    }
    private void Roam() 
    {
        if (targetPos != null)
        {
            //Debug.Log("Roaming...");
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
            Vector3 directionToTarget = (visualTarget.position - transform.position).normalized; //raw direction vector
            if (Vector3.Angle(transform.forward, directionToTarget) < visualAngle / 2)
            {///  ^--- creating the FOV angle from forward vector by halfing the angle and distributing it equally on both sides

                float distanceToTarget = Vector3.Distance(transform.position, visualTarget.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    targetPos = visualTarget;
                    return true; ///if the visual target within the angle, range, and not obtructed: then chase
                }
                return false; /// The visual target is not within the angle of the FOV
            }
        }
        return false; /// There's nothing in the sphere as a visual target mask or in the angle of the FOV
    }
    private bool PlayerHiddenCheck() 
    {
        EPlayerState tempPlayerState = playerRef.GetComponent<PlayerControls>().GetPlayerState();
        //_timerComponent.SetTimerMax(playerLostCooldown);
        //_isPlayerLostCoolDown = !_timerComponent.IsTimerFinished();
        if (tempPlayerState == EPlayerState.hiding/* && !_isPlayerLostCoolDown*/) 
        {
            return true; 
        }
        return false;
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

    private void OnTriggerEnter(Collider other)
    {
        ///for when hearing range touches a hearable object
        Debug.Log(other);
        if (other.gameObject.GetComponent<INoiseInteraction>())
        {
            Debug.Log("Enemy: near noisemaker");
            noiseObjsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ///for when hearing range leaves a hearable object
        Debug.Log(other);
        if (other.gameObject.GetComponent<INoiseInteraction>())
        {
            Debug.Log("Enemy: lost noisemaker");
            noiseObjsInRange.Remove(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
public enum EEnemyState { wait, roam, curious, chase}
