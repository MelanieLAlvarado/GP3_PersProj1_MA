using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

[CustomEditor(typeof(EnemyAI))]
public class FOVEditor : Editor ///For visual of FOV in editor
{
    private void OnSceneGUI()
    {
        EnemyAI fov = (EnemyAI)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.GetVisualRadius());

        Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.GetVisualAngle() / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.GetVisualAngle() / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.GetVisualRadius());
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.GetVisualRadius());

        if (fov.GetIsPlayerInFOV()) 
        {
            Handles.color = Color.green;
            if (fov.GetTargetPos()) 
            {
                Handles.DrawLine(fov.transform.position, fov.GetTargetPos().position);
            }
        }
    }
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees) 
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}  
#endif