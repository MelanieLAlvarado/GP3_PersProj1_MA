using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NoiseComponent))]
public class INoiseInteraction : MonoBehaviour, IInterActions
{
    PlayerControls _player;
    NoiseComponent _noiseComponent;
    [SerializeField][Range(0, 5)] private float detectionRange = 2f; ///Edit sphere collider size on start

    private void Start()
    {
        SphereCollider detectionCollider = gameObject.AddComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRange;

        _noiseComponent = this.GetComponent<NoiseComponent>();
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
        if (_player && _noiseComponent)
        {
            ///triggering noise component
            _noiseComponent.TriggerNoise();
            _player.GetComponent<HearingComponent>().BeginRemoveNoiseDelay(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        _player = other.GetComponent<PlayerControls>();
        if (_player && _player.GetEntityType() == EEntityType.Player)
        {
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
