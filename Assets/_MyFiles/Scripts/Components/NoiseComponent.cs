using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Stimuli))]
public class NoiseComponent : MonoBehaviour
{
    private NoiseManager _noiseManager;
    private float _rawNoiseAmount;
    [SerializeField][Range(0, 1)] private float currentNoiseMultiplier;
    [SerializeField][Range(0, 10)] private float triggerTimerValue = 1.0f;
    [SerializeField] private bool bIsTriggerTimer = false;
    private GameObject _ownerObject;
    //private bool _bCanMakeNoise = false;
    private bool _bIsTriggered = false;

    /*public bool GetCanMakeNoise() ///for allowing noise to be made and turned on once noiseManager is found
    {
        return _bCanMakeNoise;
    }
    /*private void Awake()
    {
        StartCoroutine(AddObjToNoiseManager());
    }
    private IEnumerator AddObjToNoiseManager() 
    {
        yield return new WaitForSeconds(0.5f);///delay while noisemanager spawns
        if (!_noiseManager)
        { 
            _noiseManager = GameManager.m_Instance.GetNoiseManager();
            _ownerObject = this.gameObject;
            _noiseManager.AddNoiseSourceInScene(_ownerObject); ///adding owner to noisesInScene in noise manager
            _bCanMakeNoise = true;
            StopCoroutine(AddObjToNoiseManager());
        }
        yield return new WaitForEndOfFrame();
    }*/
    public bool GetIsTriggered() { return _bIsTriggered; }
    public void SetIsTriggered(bool stateToSet) { _bIsTriggered = stateToSet; }
    public float GetNoiseMultiplier()  { return currentNoiseMultiplier; }
    public void SetNoiseMultiplier(float amountToSet)
    {
        currentNoiseMultiplier = amountToSet;
    }
    public float GetRawNoiseAmount() { return _rawNoiseAmount; } ///calculated by 100 * noiseMultiplier
    public void TriggerNoise()
    {
        //Debug.Log($"{_ownerObject}'s Noise triggereed!");
        _bIsTriggered = true;
        _rawNoiseAmount = 100 * currentNoiseMultiplier;
        if (bIsTriggerTimer)
        {
            StartCoroutine(TriggerTimer());
        }
        /*if (!_noiseManager)
        {
            _noiseManager = GameManager.m_Instance.GetNoiseManager();
        }
        if (_noiseManager != null && _ownerObject != null)
        {
            //Debug.Log($"the gameobject is: {_ownerObject}");


            //Should be removed once stimuli/sense are working
            //_noiseManager.AddActiveNoiseSource(_ownerObject); ///adding owner to activeNoises in noisemanager
            //_noiseManager.CheckNearbyHearingObjects();
        }*/
    }
    private IEnumerator TriggerTimer() 
    {
        yield return new WaitForSeconds(triggerTimerValue);
        _bIsTriggered = false;
        StopCoroutine(TriggerTimer());
    }
    
}
