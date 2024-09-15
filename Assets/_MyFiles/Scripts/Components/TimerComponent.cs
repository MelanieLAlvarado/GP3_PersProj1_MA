using UnityEngine;

public class TimerComponent : MonoBehaviour
{
    private float _timerValue = 0;
    private float _timerMax;
    private bool _runTimer = false;

    public void SetTimerMax(float valToSet) { _timerMax = valToSet; }
    public void ResetTimer() { _timerValue = _timerMax; }
    public void SetRunTimer(bool timerToSet) { _runTimer = timerToSet; }
    public bool IsTimerFinished() //boolean to say if the timer is finished
    {
        if (_timerValue <= 0) //Set run timer to true when player leaves enemy sight so linger chase.
        {
            //_runTimer = false; //debug this
            Debug.Log($"{this.gameObject.name}'s Timer is finished!!!");
            return true;
        }
        _timerValue -= Time.deltaTime;
        return false;
    }
    public bool IsTimerRunning()
    {
        return !IsTimerFinished() && !_runTimer;
    }
}
