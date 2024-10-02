using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Stimuli))]
public class NoiseComponent : MonoBehaviour
{
    private float _rawNoiseAmount;
    [SerializeField][Range(0, 1)] private float currentNoiseMultiplier;
    [SerializeField][Range(0, 10)] private float triggerTimerCooldown = 1.0f;
    [SerializeField] private bool bIsTriggerTimer = false;
    private bool _bIsTriggered = false;

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
        _bIsTriggered = true;
        _rawNoiseAmount = 100 * currentNoiseMultiplier;
        if (bIsTriggerTimer)
        {
            StartCoroutine(TriggerTimerCooldown());
        }
    }
    private IEnumerator TriggerTimerCooldown() 
    {
        yield return new WaitForSeconds(triggerTimerCooldown);
        _bIsTriggered = false;
        StopCoroutine(TriggerTimerCooldown());
    }
    
}
