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
        float checkDistance = Mathf.Infinity/*, int ignoreLayer = 0*/) ///nothing layer
    {
        
        if (Physics.Raycast(transform.position + startOffSet, transform.GetDir(otherTransform),
                out RaycastHit hit, checkDistance/*, Physics.AllLayers, QueryTriggerInteraction.Ignore*/))
        {
            /*string hitLayerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            LayerMask hitLayer = LayerMask.GetMask(hitLayerName);
            /*Debug.Log($"{otherTransform} : exceptionLayer == {ignoreLayer}");

            Debug.Log($"{LayerMask.GetMask(ignoreLayer.ToString())}");
            Debug.Log($"ignore Layer == {ignoreLayer}");
            Debug.Log($"hit Layer == {hitLayer.value}");

            Debug.Log($"{hit.collider.gameObject} != {otherTransform.gameObject}");
            Debug.Log($"{hitLayer.value} != {ignoreLayer}");*/
            if (hit.collider.gameObject != otherTransform.gameObject/* && hitLayer.value != ignoreLayer*/)
            {
                Debug.Log($"Hit gameobjhect is: {hit.collider.gameObject}");
                return true;
            }
        }
        return false;
    }
}
