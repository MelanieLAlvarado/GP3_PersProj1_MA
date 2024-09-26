using UnityEngine;
using UnityEngine.UI;

public static class TransformExtensions
{
    public static bool InRangeOf(this Transform transform, Transform otherTransform, float range)
    { 
        return Vector3.Distance(transform.position, otherTransform.position) <= range;
    }
    public static Vector3 GetDir(this Transform transform, Transform otherTransform) 
    {
        return (otherTransform.position - transform.position).normalized;
    }
    public static bool InAngleOf(this Transform transform, Transform otherTransform, float angle)
    {
        return Vector3.Angle(transform.forward, transform.GetDir(otherTransform)) <= angle;
    }

    //other transform need collision to work
    public static bool IsBlockedTo(this Transform transform, Transform otherTransform, Vector3 startOffSet,
        float checkDistance = Mathf.Infinity)
    {
        if (Physics.Raycast(transform.position + startOffSet, transform.GetDir(otherTransform),
                out RaycastHit hit, checkDistance))
        {
            if (hit.collider.gameObject != otherTransform.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
