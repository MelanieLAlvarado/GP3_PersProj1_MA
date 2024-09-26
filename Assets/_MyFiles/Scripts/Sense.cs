using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sense : MonoBehaviour
{
    [Header("Sense Options")]
    [SerializeField] private bool bDrawDebug = true;
    [SerializeField][Range(0, 10)] private float forgetTime = 0.0f;

    private static HashSet<Stimuli> _registeredStimuliSet = new HashSet<Stimuli>();
    private HashSet<Stimuli> _currentSensibleStimuliSet = new HashSet<Stimuli>();

    private Dictionary<Stimuli, Coroutine> _forgettingCoroutines = new Dictionary<Stimuli, Coroutine>();

    public HashSet<Stimuli> GetCurrentSensibleStimuliSet() 
    {
        //Debug.Log($"current Sensible stim set count: {_currentSensibleStimuliSet.Count}");
        return _currentSensibleStimuliSet; 
    }
    public bool GetCurrentSensibleStimuliSetIsntEmpty() { return _currentSensibleStimuliSet.Count > 0; }
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
        //Debug.Log($"registered Stim: {_registeredStimuliSet.Count}");
        foreach (Stimuli stimuli in _registeredStimuliSet)
        {
            //Debug.Log($"{stimuli.gameObject.name} is sensible?: {IsStimuliSensible(stimuli)}");
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
        //Debug.Log($" added stim: {stimuli.gameObject.name}");

        if (_forgettingCoroutines.ContainsKey(stimuli))
        {
            StopCoroutine(_forgettingCoroutines[stimuli]);
            _forgettingCoroutines.Remove(stimuli);
            return;
        }
    }

    private void HandleNoSensibleStimuli(Stimuli stimuli)
    {
        if (!_currentSensibleStimuliSet.Contains(stimuli)) { return; }

        _currentSensibleStimuliSet.Remove(stimuli);

        if (forgetTime > 0.0f)
        {
            Coroutine forgettingCoroutine = StartCoroutine(ForgetStimuli(stimuli));
            _forgettingCoroutines.Add(stimuli, forgettingCoroutine);
        }
    }
    private IEnumerator ForgetStimuli(Stimuli stimuli) 
    {
        yield return new WaitForSeconds(forgetTime);
        _forgettingCoroutines.Remove(stimuli);
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
