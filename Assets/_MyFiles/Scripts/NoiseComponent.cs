using UnityEngine;

public class NoiseComponent : MonoBehaviour
{
    private NoiseManager noiseManager;
    private float rawSoundAmount;
    private float currentSoundMultiplier;
    private bool isNoiseTriggered = false;

    public float GetSoundMultiplier()  { return currentSoundMultiplier; }
    public float GetRawSoundAmount() { return rawSoundAmount; }
    private void Start()
    {
        noiseManager = GameManager.m_Instance.GetNoiseManager();
    }
    public void GetIsNoiseTriggered() 
    {
        
    }
    public void TriggerNoise(float soundMultiplier)
    {
        Debug.Log("Noise triggereed!");
        currentSoundMultiplier = soundMultiplier;
        rawSoundAmount = 100 * currentSoundMultiplier;
        if (!noiseManager) 
        {
            noiseManager = GameManager.m_Instance.GetNoiseManager();
        }
        noiseManager.AddActiveNoiseSource(this.gameObject);
    }
}
