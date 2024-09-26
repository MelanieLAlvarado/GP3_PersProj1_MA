using UnityEngine;

public class Stimuli : MonoBehaviour
{
    [SerializeField] private bool isVisuallyDetectable = false;
    private bool _isChaseable = true;

    public bool GetIsVisuallyDetectable() { return isVisuallyDetectable; }
    public bool GetIsChaseable() { return _isChaseable; }
    public void SetIsChaseable(bool stateToSet) { _isChaseable = stateToSet; }
    void Start()
    {
        Sense.RegisterStimuli(this);
    }

    private void OnDestroy()
    {
        Sense.UnRegisterStimuli(this);
    }
}
