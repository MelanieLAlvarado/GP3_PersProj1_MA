using UnityEngine;

public class INoiseInteraction : MonoBehaviour, IInterActions
{
    PlayerControls player;
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
            Debug.Log("NOISE HERE");
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
