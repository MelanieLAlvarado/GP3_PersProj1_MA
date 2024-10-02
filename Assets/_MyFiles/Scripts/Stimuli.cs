using UnityEngine;

public class Stimuli : MonoBehaviour
{
    [SerializeField] private bool isVisuallyDetectable = false; ///tickable in editor
    private bool _isCurrentlyChaseable = false;

    public bool GetIsVisuallyDetectable() { return isVisuallyDetectable; }
    public bool GetIsCurrentlyChaseable() { return _isCurrentlyChaseable; }
    public void SetIsChaseable(bool stateToSet) { _isCurrentlyChaseable = stateToSet; }
    void Start()
    {
        Sense.RegisterStimuli(this);
    }

    private void OnDestroy()
    {
        Sense.UnRegisterStimuli(this);
    }
}
