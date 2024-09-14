using System.Collections;
using UnityEngine;

public class NoiseComponent : MonoBehaviour
{
    private NoiseManager _noiseManager;
    private float _rawNoiseAmount;
    private float _currentNoiseMultiplier;
    private GameObject _ownerObject;
    private bool _canMakeNoise = false;

    public bool GetCanMakeNoise() //use later to make cooldown's for noise objects
    {
        return _canMakeNoise;
    }
    private void Awake()
    {
        StartCoroutine(AddObjToNoiseManager());
    }
    private IEnumerator AddObjToNoiseManager() 
    {
        yield return new WaitForSeconds(0.5f);
        if (!_noiseManager)
        { 
            _noiseManager = GameManager.m_Instance.GetNoiseManager();
            _ownerObject = this.gameObject;
            _noiseManager.AddNoiseSource(_ownerObject);
            _canMakeNoise = true;
            StopCoroutine(AddObjToNoiseManager());
        }
        yield return new WaitForEndOfFrame();
    }
    public float GetNoiseMultiplier()  { return _currentNoiseMultiplier; }
    public void SetNoiseMultiplier(float amountToSet)
    {
        _currentNoiseMultiplier = amountToSet;
    }
    public float GetRawNoiseAmount() { return _rawNoiseAmount; }
    public void TriggerNoise()
    {
        Debug.Log($"{_ownerObject}'s Noise triggereed!");
        _rawNoiseAmount = 100 * _currentNoiseMultiplier;
        if (!_noiseManager)
        {
            _noiseManager = GameManager.m_Instance.GetNoiseManager();
        }
        if (this.gameObject && _ownerObject)
        {
            Debug.Log("Owner is present!");
        }
        if (_noiseManager != null && _ownerObject != null && !_noiseManager.IsObjInActiveNoiseList(_ownerObject))
        {
            //Debug.Log($"the gameobject is: {_ownerObject}");
            _noiseManager.AddActiveNoiseSource(_ownerObject);
            _noiseManager.CheckNearbyHearingObjects();
        }
    }
}
