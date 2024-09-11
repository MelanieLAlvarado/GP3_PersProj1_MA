using System.Collections.Generic;
using UnityEngine;

public class HearingComponent : MonoBehaviour
{
    //WIP rework to function on player

    private List<float> CalculateSoundValues(List<GameObject> noiseObjsToReceive, float hearingRange)
    {
        List<float> soundVals = new List<float>();
        if (noiseObjsToReceive.Count > 0) //might find a more understandable way to gauge sound value later...
        {
            GameObject noiseTemp = noiseObjsToReceive[0];
            List<float> distancesFromEnemy = new List<float>();
            for (int i = 0; i < noiseObjsToReceive.Count; i++) //calculate sound values
            {
                float iDistance = Vector3.Distance(transform.position, noiseObjsToReceive[i].transform.position);
                distancesFromEnemy.Add(iDistance);

                float distMultiplier = iDistance / hearingRange;

                NoiseComponent noiseComp = noiseObjsToReceive[i].GetComponent<NoiseComponent>();
                soundVals.Add(noiseComp.GetRawSoundAmount() * distMultiplier);
            }
        }
        return soundVals;
    }
    private Transform ChooseNoiseTarget(List<GameObject> noiseObjsToReceive, List<float> soundValsToReceive)
    {
        //iterates through the list to find the highest noise value
        int index = 0;
        float noiseNum = soundValsToReceive[index];
        Transform targetPos = noiseObjsToReceive[index].transform;
        for (int i = 1; i < noiseObjsToReceive.Count; i++) ///choose the sound with the highest noise
        {
            float iNoiseNum = soundValsToReceive[i];
            if (noiseNum <= iNoiseNum)
            {
                noiseNum = iNoiseNum;
                index = i;
            }
            targetPos = noiseObjsToReceive[index].transform;
        }
        return targetPos;
    }
}
