using UnityEngine;

public class Stimuli : MonoBehaviour
{
    [SerializeField] private bool isVisuallyDetectable = false; ///tickable in editor
    private bool _isChaseable = false;

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
