using System.Drawing;
using System.Linq;
using UnityEngine;

public class VisionComponent : Sense
{
    [Header("Field of View Options")]
    [SerializeField] private float closeRadius = 2f;
    [SerializeField] private float visualRadius;
    [Range(0, 360)][SerializeField] private float visualAngle;

    [SerializeField] private LayerMask exceptionLayer;

    private bool _bCanSeeVisualTarget = false;
    private GameObject _visualTarget = null;///debug purposes

    public bool GetCanSeeVisualTarget() { return _bCanSeeVisualTarget; } 
    public GameObject GetVisualTarget() { return _visualTarget; }
    protected override bool IsStimuliSensible(Stimuli stimuli)
    {
        if (stimuli.GetIsVisuallyDetectable() == false) ///stimuli cannot be chased/seen
        {
            return false;
        }
        if (!transform.InRangeOf(stimuli.transform, visualRadius))
        {
            _bCanSeeVisualTarget = false;
            _visualTarget = null;
            return false;
        }
        if (!transform.InAngleOf(stimuli.transform, visualAngle/2) && !transform.InRangeOf(stimuli.transform, closeRadius))
        {
            _bCanSeeVisualTarget = false;
            _visualTarget = null;
            return false;
        }

        RaycastHit hitPoint;
        if (transform.IsBlockedTo(stimuli.transform, Vector3.up + Vector3.forward, out hitPoint, visualRadius))
        {
            ///does not get blocked if something is that layer
            if (transform.IsHitExceptionLayer(hitPoint, exceptionLayer))  
            {
                return CanSeeStimuli(stimuli);
            }
            return false;
        }

        return CanSeeStimuli(stimuli);
    }
    private bool CanSeeStimuli(Stimuli stimuli) 
    {///determines if the stimuli is hidden before vision cone
        if (stimuli.GetIsCurrentlyChaseable() == true)
        {
            _bCanSeeVisualTarget = true;
            if (_visualTarget == null)
            { 
                _visualTarget = stimuli.gameObject;
            }
            return true;
        }
        if (_bCanSeeVisualTarget == false)
        {
            _visualTarget = null;
            return false;
        }
        return _bCanSeeVisualTarget;
    }
    protected override void OnDrawDebug()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up, closeRadius);

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
