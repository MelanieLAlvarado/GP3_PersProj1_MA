using System.Collections.Generic;
using UnityEngine;

public abstract class Sense : MonoBehaviour
{
    [SerializeField] private bool bDrawDebug = true;

    private static HashSet<Stimuli> _registeredStimuliSet = new HashSet<Stimuli>();
    private HashSet<Stimuli> _currentSensibleStimuliSet = new HashSet<Stimuli>();

    public HashSet<Stimuli> GetCurrentSensibleStimuliSet() 
    {
        Debug.Log($"current Sensible stim set count: {_currentSensibleStimuliSet.Count}");
        return _currentSensibleStimuliSet; 
    }
    public static void RegisterStimuli(Stimuli stimuli) 
    {
        _registeredStimuliSet.Add(stimuli);
        Debug.Log($"registered stim added. stim count: {_registeredStimuliSet.Count}");
    }
    public static void UnRegisterStimuli(Stimuli stimuli) 
    {
        _registeredStimuliSet.Remove(stimuli);
        Debug.Log($"registered stim removed. stim count: {_registeredStimuliSet.Count}");
    }

    protected abstract bool IsStimuliSensible(Stimuli stimuli);

    private void FixedUpdate()
    {
        Debug.Log($"registered Stim: {_registeredStimuliSet.Count}");
        foreach (Stimuli stimuli in _registeredStimuliSet)
        {
            Debug.Log($"{stimuli.gameObject.name} is sensible?: {IsStimuliSensible(stimuli)}");
            if (IsStimuliSensible(stimuli))
            {
                HandleSensibleStimuli(stimuli);
            }
            else
            {
                HandleNoSensibleStimuli(stimuli);
            }
        }
    }
    private void HandleSensibleStimuli(Stimuli stimuli)
    {
        if (_currentSensibleStimuliSet.Contains(stimuli)) { return; }

        _currentSensibleStimuliSet.Add(stimuli);
        Debug.Log($" added stim: {stimuli.gameObject.name}");
    }

    private void HandleNoSensibleStimuli(Stimuli stimuli)
    {
        if (!_currentSensibleStimuliSet.Contains(stimuli)) { return; }

        _currentSensibleStimuliSet.Remove(stimuli);
        Debug.Log($" removed stim: {stimuli.gameObject.name}");
    }
    private void OnDrawGizmos()
    {
        if (bDrawDebug)
        {
            OnDrawDebug();
        }
    }
    protected virtual void OnDrawDebug() 
    {
        ///override in child class
    }
}
