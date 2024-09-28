using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class HearingComponent : Sense
{
    private UIManager _uIManager;
    [Header("Hearing Options")]
    [Range(1.0f, 50.0f)][SerializeField] float hearingRange = 30.0f;
    [Range(0f, 100f)][SerializeField] float hearingThreshold = 20.0f;
    private bool _bAreNoisesInaudible = true;

    [Header("Hearing List Info [READ ONLY]")]
    private Dictionary<Stimuli, float> _audibleNoiseDict = new Dictionary<Stimuli, float>();

    private float targetNoiseCalculatedValue = 0.0f;
    private Transform _hearingTarget;

    public float GetTargetNoiseCalculatedValue() { return targetNoiseCalculatedValue;  }
    public bool GetIsAudibleNoisesPresent() ///usually starts the hearing process in other scripts
    {
        return _audibleNoiseDict.Count > 0;
    }
    public void SetHearingThreshold(float amountToSet)
    {
        hearingThreshold = amountToSet;
    }
    public void RemoveFromAudibleNoiseDict(GameObject noiseToRemove)
    {
        Stimuli stimuliKey = noiseToRemove.GetComponent<Stimuli>();
        if (_audibleNoiseDict.ContainsKey(stimuliKey))
        {
            _audibleNoiseDict.Remove(stimuliKey);
        }
    }
    public void ClearAudibleNoiseInfo() ///reseting the sounds heard
    {
        _hearingTarget = null;
        targetNoiseCalculatedValue = 0.0f;
        _bAreNoisesInaudible = true;
        _audibleNoiseDict.Clear();
    }
    public Transform GetHearingTarget() 
    {
        return _hearingTarget; 
    }
    private void Update()
    {
        foreach (Stimuli stimuli in GetCurrentSensibleStimuliSet())
        {
            UpdateAudibleStimuliDict(stimuli);
        }
        /*Debug.Log($"{this.gameObject.name} -- audible noise dictionary: {_audibleNoiseDict.Count}");
        Debug.Log($"{this.gameObject.name} -- current sensible stimuli : {GetCurrentSensibleStimuliSet().Count}");*/
        SelectHearingTarget();
    }
    private void UpdateAudibleStimuliDict(Stimuli stimuli)
    {
        float stimuliNoise = CalculateSingleNoiseValue(stimuli.gameObject);
        if (stimuliNoise > hearingThreshold)
        {
            if (_audibleNoiseDict.ContainsKey(stimuli))
            {
                _audibleNoiseDict[stimuli] = stimuliNoise;
                return;
            }
            NoiseComponent noiseComp = stimuli.gameObject.GetComponent<NoiseComponent>();
            if (noiseComp && noiseComp.GetIsTriggered() == true)
            {
                _audibleNoiseDict.Add(stimuli, stimuliNoise);
            }
        }
        else
        {
            if (_audibleNoiseDict.ContainsKey(stimuli))
            {
                _audibleNoiseDict.Remove(stimuli);
            }
        }
        if (!IsNoisesCountMoreThanSenses())
        {
            _audibleNoiseDict.Clear();
        }
    }
    private bool IsNoisesCountMoreThanSenses()
    {
        return _audibleNoiseDict.Count <= GetCurrentSensibleStimuliSet().Count;
    }
    private float CalculateSingleNoiseValue(GameObject objToReceive)
    {
        float iDistance = Vector3.Distance(transform.position, objToReceive.transform.position);

        float distMultiplier = 1 - (iDistance / hearingRange); ///multiplier based on distance in range

        NoiseComponent noiseComp = objToReceive.GetComponent<NoiseComponent>();
        return noiseComp.GetRawNoiseAmount() * distMultiplier;
    }
    private Stimuli FindLoudestNoiseStimuli()
    {
        float loudestNoise = _audibleNoiseDict.ElementAt(0).Value;
        int loudestStimuliIndex = 0;
        for (int i = 1; i < _audibleNoiseDict.Count; i++) ///choose the sound with the highest noise
        {
            float iNoiseToCheck = _audibleNoiseDict.ElementAt(i).Value;
            if (loudestNoise <= iNoiseToCheck)
            {
                loudestNoise = iNoiseToCheck;
                loudestStimuliIndex = i;
            }
        }
        return _audibleNoiseDict.ElementAt(loudestStimuliIndex).Key;
    }
    private void SelectHearingTarget()
    {
        ///iterates through the list to find the highest noise value

        if (GetIsAudibleNoisesPresent() && IsNoisesCountMoreThanSenses())
        {
            Stimuli loudestStimuli = FindLoudestNoiseStimuli();
            float loudestNoiseVal = _audibleNoiseDict[loudestStimuli];
            if (loudestNoiseVal > hearingThreshold)
            {
                targetNoiseCalculatedValue = loudestNoiseVal;
                _hearingTarget = loudestStimuli.transform;
                _bAreNoisesInaudible = false;
                return;
            }
        }
        if (!_bAreNoisesInaudible)
        {
            _bAreNoisesInaudible = true;
            _hearingTarget = null;
            targetNoiseCalculatedValue = 0.0f;
        }
    }
    
    protected override void OnDrawDebug()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        if (_hearingTarget)
        {
            if (targetNoiseCalculatedValue > hearingThreshold)
            {
                Debug.DrawRay(_hearingTarget.position, Vector3.up, UnityEngine.Color.yellow, 0.1f);
            }
        }
    }

    protected override bool IsStimuliSensible(Stimuli stimuli)
    {
        float stimuliNoiseVal = CalculateSingleNoiseValue(stimuli.gameObject);
        return transform.InRangeOf(stimuli.transform, hearingRange) && stimuliNoiseVal > hearingThreshold;
    }
}
