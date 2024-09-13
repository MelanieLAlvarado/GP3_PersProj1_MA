using UnityEngine;

public class TimerComponent : MonoBehaviour
{
    private float _timerValue;
    private float _timerMax;

    public void SetTimerMax(float valToSet) { _timerMax = valToSet; }
    public void ResetTimer() { _timerValue = _timerMax; }
    public bool IsTimerFinished() //boolean to say if the timer is finished
    {
        if (_timerValue <= 0)
        {
            return true;
        }
        _timerValue -= Time.deltaTime;
        return false;
    }
}
