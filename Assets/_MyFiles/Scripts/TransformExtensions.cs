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

    ///other transform need collision to work
    public static bool IsBlockedTo(this Transform transform, Transform otherTransform, Vector3 startOffSet,
        out RaycastHit hitPoint, float checkDistance = Mathf.Infinity) ///nothing layer
    {
        bool isRaycast = Physics.Raycast(transform.position + startOffSet,
                transform.GetDir(otherTransform), out RaycastHit hit, checkDistance);
        hitPoint = hit;
        if (isRaycast)
        {
            if (hit.collider.gameObject != otherTransform.gameObject)
            {
                return true; ///raycast is blocked
            }
        }
        return false;///raycast is not blocked
    }
    public static bool IsHitExceptionLayer(this Transform transform, RaycastHit hit, 
        int ignoreLayerVal = 0)
    {
        string hitLayerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
        LayerMask hitLayer = LayerMask.GetMask(hitLayerName);

        if (hitLayer.value == ignoreLayerVal)
        {
            return true;///raycast hitting an object with the ignored layer
        }
        return false;
    }
}
