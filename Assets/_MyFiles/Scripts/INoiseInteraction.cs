using UnityEngine;

[RequireComponent(typeof(NoiseComponent))]
public class INoiseInteraction : MonoBehaviour, IInterActions
{
    PlayerControls player;
    NoiseComponent noiseComponent;
    [SerializeField] [Range(0, 1)] private float noiseMultiplier = 0.8f;
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
        if (player)
        {
            //spawn transform/ping location here for enemy
            if (noiseComponent = this.GetComponent<NoiseComponent>())
            {
                Debug.Log("NOISE HERE");
                noiseComponent.TriggerNoise(noiseMultiplier);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerControls>();
        if (player && player.GetEntityType() == EEntityType.Player)
        {
            Debug.Log("near noisemaker");
            player.SetTargetInteractible(this.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (player && other.GetComponent<PlayerControls>() == player
            && this.gameObject == player.GetTargetInteractible())
        {
            player.SetTargetInteractible(null);
            player = null;
        }
    }
}
