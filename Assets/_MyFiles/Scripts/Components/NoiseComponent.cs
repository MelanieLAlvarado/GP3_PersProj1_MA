using UnityEngine;

public class NoiseComponent : MonoBehaviour
{
    private NoiseManager _noiseManager;
    private float _rawSoundAmount;
    private float _currentSoundMultiplier;
    private bool _isNoiseTriggered = false;

    public float GetSoundMultiplier()  { return _currentSoundMultiplier; }
    public float GetRawSoundAmount() { return _rawSoundAmount; }
    private void Start()
    {
        _noiseManager = GameManager.m_Instance.GetNoiseManager();
    }
    public void GetIsNoiseTriggered() 
    {
        
    }
    public void TriggerNoise(float soundMultiplier)
    {
        Debug.Log("Noise triggereed!");
        _currentSoundMultiplier = soundMultiplier;
        _rawSoundAmount = 100 * _currentSoundMultiplier;
        if (!_noiseManager) 
        {
            _noiseManager = GameManager.m_Instance.GetNoiseManager();
        }
        _noiseManager.AddActiveNoiseSource(this.gameObject);
    }
}
