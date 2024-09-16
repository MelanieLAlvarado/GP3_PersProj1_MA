using System.Collections;
using UnityEngine;

public class NoiseComponent : MonoBehaviour
{
    private NoiseManager _noiseManager;
    private float _rawNoiseAmount;
    private float _currentNoiseMultiplier;
    private GameObject _ownerObject;
    private bool _canMakeNoise = false;

    public bool GetCanMakeNoise() ///for allowing noise to be made and turned on once noiseManager is found
    {
        return _canMakeNoise;
    }
    private void Awake()
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
    public float GetRawNoiseAmount() { return _rawNoiseAmount; } ///calculated by 100 * noiseMultiplier
    public void TriggerNoise()
    {
        //Debug.Log($"{_ownerObject}'s Noise triggereed!");
        _rawNoiseAmount = 100 * _currentNoiseMultiplier;
        if (!_noiseManager)
        {
            _noiseManager = GameManager.m_Instance.GetNoiseManager();
        }
        if (_noiseManager != null && _ownerObject != null)
        {
            //Debug.Log($"the gameobject is: {_ownerObject}");
            _noiseManager.AddActiveNoiseSource(_ownerObject); ///adding owner to activeNoises in noisemanager
            _noiseManager.CheckNearbyHearingObjects();
        }
    }
}
