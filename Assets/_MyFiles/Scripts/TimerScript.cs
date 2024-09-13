using UnityEngine;

public class TimerScript : MonoBehaviour
{
    private float _timerValue;
    private float _timerMax;

    public void SetTimerMax(float valToSet) { _timerMax = valToSet; }
    public void ResetTimer() { _timerValue = _timerMax; }
    public bool RunTimer() //boolean to say if the timer is finished
    {
        if (_timerValue <= 0)
        {
            return true;
        }
        _timerValue -= Time.deltaTime;
        return false;
    }
}
