using UnityEngine;

public class NoiseComponent : MonoBehaviour
{
    private NoiseManager noiseManager;
    private float rawSoundAmount;
    private float currentSoundMultiplier;
    public float GetSoundMultiplier()  { return currentSoundMultiplier; }
    public float GetRawSoundAmount() { return rawSoundAmount; }
    private void Start()
    {
        noiseManager = GameManager.m_Instance.GetNoiseManager();
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
        noiseManager.AddNoise(this.gameObject);
    }
}
