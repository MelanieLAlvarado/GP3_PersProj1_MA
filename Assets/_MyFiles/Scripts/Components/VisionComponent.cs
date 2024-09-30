using System.Drawing;
using UnityEngine;

public class VisionComponent : Sense
{
    [Header("Field of View Options")]
    [SerializeField] private float visualRadius;
    [Range(0, 360)][SerializeField] private float visualAngle;

    [SerializeField] private LayerMask exceptionLayer;

    [SerializeField] private bool bCanSeeVisualTarget = false;
    private GameObject _visualTarget = null;

    public bool GetCanSeeVisualTarget() { return bCanSeeVisualTarget; }

    private void Start()
    {
        Debug.Log($"Initial Layer Type: {exceptionLayer.value}");
    }

    protected override bool IsStimuliSensible(Stimuli stimuli)
    {
        if (stimuli.GetIsVisuallyDetectable() == false) ///stimuli cannot be chased/seen
        {
            /*bCanSeeVisualTarget = false;
            _visualTarget = null;*/
            return false;
        }
        Debug.Log($"{stimuli.gameObject.name}'s IsChaseable == {stimuli.GetIsChaseable()}");
        Debug.Log($"{stimuli.gameObject.name}'s CanSeeVisualTarget == {bCanSeeVisualTarget}");
        if (!transform.InRangeOf(stimuli.transform, visualRadius))
        {
            bCanSeeVisualTarget = false;
            _visualTarget = null;
            return false;
        }
        if (!transform.InAngleOf(stimuli.transform, visualAngle/2))
        {
            bCanSeeVisualTarget = false;
            _visualTarget = null;
            return false;
        }

        if (transform.IsBlockedTo(stimuli.transform, Vector3.up, visualRadius/*, exceptionLayer.value*/))
        {
            Debug.Log("IS PASSING BLOCK CHECK");
            if (stimuli.GetIsChaseable())
            {
                bCanSeeVisualTarget = true;
                _visualTarget = stimuli.gameObject;
                Debug.Log("Flag2 ++ canSeeVisTarget");
                return true;
            }
            /*else
            {
                bCanSeeVisualTarget = false;
            }*/
            if (bCanSeeVisualTarget == false)
            {
                Debug.Log("Flag1 ++ can NOT SeeVisTarget");
                _visualTarget = null;
                return false;
            }
        }
        Debug.Log("Flag3 ++ nothing Obscuring!");

        /*targetPos = visualTarget;
        if (!PlayerHiddenCheck())
        {
            bCanSeePlayer = true;
        }
        if (bCanSeePlayer == false)
        {
            return false; ///player hid before coming inside FOV
        }
        return true; ///if the visual target within the angle, range, and not obtructed: then chase*/

        Debug.Log($"CAN SEE {stimuli.gameObject.name}");
        bCanSeeVisualTarget = true; //Determine whether to chase here
        //stimuli.SetIsChaseable(true);
        _visualTarget = stimuli.gameObject;
        return true;
    }
    /*private bool FieldOfViewCheck()
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
                    bCanSeePlayer = true;
                }
                if (bCanSeePlayer == false)
                {
                    return false; ///player hid before coming inside FOV
                }
                return true; ///if the visual target within the angle, range, and not obtructed: then chase
            }
            bCanSeePlayer = false;
            return false; /// The visual target is not within the angle of the FOV
        }
    }
    bCanSeePlayer = false;
    return false; /// There's nothing in the sphere as a visual target mask or in the angle of the FOV
}*/

    protected override void OnDrawDebug()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up, visualRadius);
        
        Vector3 lineLeft = Quaternion.AngleAxis(visualAngle/2, Vector3.up) * transform.forward;
        Vector3 lineRight = Quaternion.AngleAxis(-visualAngle/2, Vector3.up) * transform.forward;

        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + lineLeft * visualRadius);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + lineRight * visualRadius);
        if (_visualTarget)
        {
            Debug.DrawRay(_visualTarget.transform.position, Vector3.up, UnityEngine.Color.red, 0.1f);
        }
    }
}
