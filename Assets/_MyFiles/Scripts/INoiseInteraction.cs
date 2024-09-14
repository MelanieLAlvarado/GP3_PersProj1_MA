using UnityEngine;

[RequireComponent(typeof(NoiseComponent))]
public class INoiseInteraction : MonoBehaviour, IInterActions
{
    PlayerControls _player;
    NoiseComponent _noiseComponent;
    [SerializeField] [Range(0, 1)] private float noiseMultiplier = 0.8f;
    private void Start()
    {
        GetComponent<NoiseComponent>().SetNoiseMultiplier(noiseMultiplier);
    }
    public EEntityType GetEntityType()
    {
        return EEntityType.NoiseMaker;
    }
    public string GetInteractionMessage()
    {
        return "Press 'E' to make noise.";
    }
    public void OnInteraction()
    {
        if (_player)
        {
            ///pinging enemy
            if (_noiseComponent = this.GetComponent<NoiseComponent>())
            {
                Debug.Log("NOISE HERE");
                _noiseComponent.TriggerNoise();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        _player = other.GetComponent<PlayerControls>();
        if (_player && _player.GetEntityType() == EEntityType.Player)
        {
            Debug.Log("near noisemaker");
            _player.SetTargetInteractible(this.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (_player && other.GetComponent<PlayerControls>() == _player
            && this.gameObject == _player.GetTargetInteractible())
        {
            _player.SetTargetInteractible(null);
            _player = null;
            GameManager.m_Instance.GetUIManager().HideInteractionText();
        }
    }
}
