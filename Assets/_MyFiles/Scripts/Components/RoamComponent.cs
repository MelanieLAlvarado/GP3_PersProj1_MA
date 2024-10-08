using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RoamComponent : MonoBehaviour
{
    NavMeshAgent _ownerAgent;
    private Transform _roamPos;

    [Header("Roaming Settings")]
    [SerializeField] private string tempCallPosName = "RoamPos";
    [Range(1.0f, 30.0f)][SerializeField] float roamingRange = 20f;
    [SerializeField] private bool enableIndependentRoam = false; 
    /// ^-- Determines if owner can roam without other script influence
    
    ///Movement Info
    private float _independentRoamSpeed;
    Vector3 _prevPosition;
    public Transform GetRoamPos()
    {
        return _roamPos;
    }
    private void Awake()
    {
        _ownerAgent = GetComponent<NavMeshAgent>();

        _roamPos = new GameObject(tempCallPosName).transform;
        _roamPos.position = transform.position;

        _roamPos.transform.position = AtRoamPositionCheck(_roamPos.transform);
    }
    private void Update()
    {
        if (enableIndependentRoam)
        {
            _ownerAgent.destination = _roamPos.transform.position;
            Vector3 currentMove = transform.position - _prevPosition;
            _independentRoamSpeed = currentMove.magnitude / Time.deltaTime;
            _prevPosition = transform.position;
            Roam(_roamPos);
        }
    }

    public Transform Roam(Transform targetPos) 
    {///Gets a random position and saves it on tempCallPos which is set to TargetPos
        if (targetPos != null)
        {
            targetPos = _roamPos;
            targetPos.position = AtRoamPositionCheck(_roamPos);
        }
        else
        {
            targetPos = _roamPos; ///if the targetPos is null it is set to the tempCallPos
        }
        _ownerAgent.destination = targetPos.transform.position;
        return targetPos;
    }
    private Vector3 AtRoamPositionCheck(Transform targetPos) 
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPos.transform.position);
        if (distanceToTarget < _ownerAgent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(transform.position, roamingRange, out point)) ///Gets random point
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);

                targetPos.position = new Vector3(point.x, transform.position.y, point.z);
                ///Sets targetPos to the point x and y but the enemy's y position
            }
        }
        return targetPos.position;
    }
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {   ///Gets Random point in roamrange that is on the navmesh
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
