using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] EEnemyState _enemyState;
    [Range(0f, 10f)][SerializeField] private float _waitTime;
    [Range(1.0f, 30.0f)][SerializeField] float _roamingRange = 20f;
    [SerializeField] private Transform _callPos;

    [Header("Manager Info [Read Only]")]
    [SerializeField] EnemyManager _enemyManager;

    [Header("Position Info [Read Only]")]
    [SerializeField] GameObject _playerRef;
    [SerializeField] private Transform _targetPos;

    [Header("Speed Options")]
    private NavMeshAgent _enemy_NavMeshAgent;
    private Vector3 _prevPosition;
    [SerializeField] private float _currentSpeed;

    [Header("Hearing Range Options")]
    [Range(1.0f, 30.0f)][SerializeField] float _hearingRange = 15f;
    [SerializeField] private GameObject _hRange; //visual of hearing range (temp)

    [SerializeField] private List<GameObject> _audibleNoiseList = new List<GameObject>();

    ///Might move to its own script... Field of View
    [Header("Field of View Options")]
    [SerializeField] private float _visualRadius;
    [Range(0, 360)][SerializeField] private float _visualAngle;

    [SerializeField] private LayerMask _visualTargetMask;
    [SerializeField] private LayerMask _obstructionMask;
    [SerializeField] private bool _canSeePlayer;


    //may add a timer to follow, even after player leaves visual field...
    //and a way for the enemy to face the player when lost.

    public float GetVisualRadius() { return _visualRadius; }
    public float GetVisualAngle() { return _visualAngle; }
    public bool GetCanSeePlayer() { return FieldOfViewCheck(); }
    public Transform GetTargetPos() { return _targetPos; }
    public void AddToNoiseList(GameObject noiseToAdd) 
    {
        _audibleNoiseList.Insert(_audibleNoiseList.Count, noiseToAdd);
    }
    private void Start()
    {
        StartCoroutine(FindPlayerRef());
        _prevPosition = transform.position;
        _enemy_NavMeshAgent = GetComponent<NavMeshAgent>();
        SetEnemyState(EEnemyState.wait);
        //StartCoroutine(EnemyIntellegence());
    }
    private void Update()
    {
        ///separate second part into a separate piece... (will determine if enemy chases player or not while hiding)
        if (FieldOfViewCheck() && !PlayerHiddenCheck())
        {
            SetEnemyState(EEnemyState.chase);
        }
        else if (_audibleNoiseList.Count != 0)//items will be added to list via Noise Manager (and maybe EnemyManager too)
        {
            SetEnemyState(EEnemyState.curious);
        }
        else
        {
            //add a better wait time later perhaps?
            SetEnemyState(EEnemyState.roam);
        }
    }
    /*private IEnumerator EnemyIntellegence()    
    {
        if (_enemyState == EEnemyState.wait) 
        {
            yield return new WaitForSeconds(_waitTime);
        }

        ///separate second part into a separate piece... (will determine if enemy chases player or not while hiding)
        if (FieldOfViewCheck() && _playerRef.GetComponent<PlayerControls>().GetPlayerState() != EPlayerState.hiding)
        {
            SetEnemyState(EEnemyState.chase);
        }
        else if (_audibleNoiseList.Count != 0)
        {
            SetEnemyState(EEnemyState.curious);
        } 
        else 
        {
            //add a better wait time later perhaps?
            SetEnemyState(EEnemyState.roam);
        }
        yield return new WaitForEndOfFrame();
    }*/
    private IEnumerator FindPlayerRef()
    {
        yield return new WaitForSeconds(0.5f);
        if (!_playerRef && GameManager.m_Instance.GetPlayer() != null)
        {
            _playerRef = GameManager.m_Instance.GetPlayer();
            StopCoroutine(FindPlayerRef());
        }
    }
    private void SetEnemyState(EEnemyState stateToSet)
    {
        _enemyState = stateToSet;
        switch (_enemyState)
        {
            case EEnemyState.wait:
                ///do nothing
                _targetPos = _callPos;
                //Will add a wait function eventually...
                _enemyState = EEnemyState.roam;
                break;
            case EEnemyState.roam:
                Debug.Log("FLAG 1: start roam");
                Roam(); //WIP
                GoToTarget();
                break;
            case EEnemyState.curious:
                //swap target to an audible sound
                ChooseNoiseTarget();
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
        //when @ first target pos... -> go to wait (unless player)
        /*
        NoiseList.removeAt(0);
        NoiseList.Insert(NoiseList.count, newSound)

         */
    }
    private void GoToTarget() 
    {
        Debug.Log("going to target...");
        _enemy_NavMeshAgent.destination = _targetPos.transform.position;
        Vector3 currentMove = transform.position - _prevPosition;
        _currentSpeed = currentMove.magnitude/Time.deltaTime;
        _prevPosition = transform.position;
    }
    private void ChasePlayer() 
    {
        if (FieldOfViewCheck() && _targetPos != null && _playerRef)
        {
            _targetPos = _playerRef.transform;
            //_enemy_NavMeshAgent.destination = _targetPos.transform.position;
        }
    }
    private void CheckHidingPlace() 
    {
        //will include chasing the player for a short time after leaving the visual field
        //  and will include a way to decide to pull player out of hiding spots if the player
        //  was seen as they hid. 
    }
    private void ChooseNoiseTarget()
    {
        if (_audibleNoiseList.Count > 0) 
        {
            GameObject noiseTemp = _audibleNoiseList[0];
            for (int i = 1;i < _audibleNoiseList.Count ; i++)
            {
                /*
                //NoiseScript noiseTempScript = noiseTemp.GetComponent<NoiseScript>().GetNoiseLevel();
                NoiseScript iNoiseScript = _audibleNoiseList[i].GetComponent<NoiseScript>();
                if(noiseTempScript.GetNoiseLevel() < iNoiseScript.GetNoiseLevel())
                {
                    noiseTemp = _audibleNoiseList[i];
                    continue;
                } 
                else if ((noiseTempScript.GetNoiseLevel() == iNoiseScript.GetNoiseLevel())
                {
                    float tempDistance = Vector3.Distance(transform.position, noiseTemp.position);
                    float iDistance = Vector3.Distance(transform.position, _audibleNoiseList[i].position);
                    if(tempDistance > iDistance)
                    {
                        noiseTemp = _audibleNoiseList[i];
                    }
                    continue;
                }
                */
            }
            _targetPos = noiseTemp.transform;
        }
    }
    private void InvestigateNoise() 
    {
        Debug.Log("Investigating noise...");
        _targetPos = _audibleNoiseList[0].transform;
        _enemy_NavMeshAgent.destination = _targetPos.transform.position;
        float targetDist = Vector3.Distance(_prevPosition, _targetPos.position);
        if (targetDist <= _enemy_NavMeshAgent.stoppingDistance) 
        {
            Debug.Log("At current target...");
            _audibleNoiseList.RemoveAt(0);
            SetEnemyState(EEnemyState.wait);
        }
    }
    private void Roam() 
    {
        if (_targetPos != null)
        {
            Debug.Log("Roaming...");
            float roamDist = Vector3.Distance(transform.position, _targetPos.position);
            Debug.Log(roamDist);
            _targetPos = _callPos;
            _targetPos.position = new Vector3(_targetPos.position.x, transform.position.y, _targetPos.position.z);
            if (roamDist <= _enemy_NavMeshAgent.stoppingDistance)
            {
                Vector3 point;
                if (RandomPoint(transform.position, _roamingRange, out point))
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);

                    _targetPos.position = new Vector3(point.x, transform.position.y, point.z);
                }
            }
            _enemy_NavMeshAgent.destination = _targetPos.transform.position;
        }
        else
        {
            _callPos.position = transform.position;
            _targetPos = _callPos;
        }
    }
    private bool FieldOfViewCheck() 
    {
        Collider[] visualChecks = Physics.OverlapSphere(transform.position, _visualRadius, _visualTargetMask);

        if (visualChecks.Length != 0)
        {
            Transform visualTarget = visualChecks[0].transform;
            Vector3 directionToTarget = (visualTarget.position - transform.position).normalized; //raw direction vector
            if (Vector3.Angle(transform.forward, directionToTarget) < _visualAngle / 2)
            {///  ^--- creating the FOV angle from forward vector by halfing the angle and distributing it equally on both sides

                float distanceToTarget = Vector3.Distance(transform.position, visualTarget.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstructionMask))
                {
                    _targetPos = visualTarget;
                    return true; ///if the visual target within the angle, range, and not obtructed: then chase
                }
                return false; /// The visual target is not within the angle of the FOV
            }
        }
        return false; /// There's nothing in the sphere as a visual target mask or in the angle of the FOV
    }
    private bool PlayerHiddenCheck() 
    {
        EPlayerState tempPlayerState = _playerRef.GetComponent<PlayerControls>().GetPlayerState();
        if (tempPlayerState == EPlayerState.hiding /*&& timer check*/) 
        {
            return true; 
        }
        return false;
    }
    /*public bool HearingCheck(GameObject objToCheck) 
    {
        float hearingDist = Vector3.Distance(transform.position, objToCheck.position);
        if (hearingDist < _hearingRange)
        {
            return true;
        }
        return false;
    }*/
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
