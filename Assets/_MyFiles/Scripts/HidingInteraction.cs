using System.Collections;
using UnityEngine;

public class HidingInteraction : MonoBehaviour, IInterActions
{
    PlayerControls player;
    [SerializeField] private Transform hidePos;
    Transform playerLastPos;

    public Transform GetHidePos() { return hidePos; }
    public EEntityType GetEntityType()
    {
        return EEntityType.HidingSpace;
    }
    public string GetInteractionMessage()
    {
        return "Press 'E' to hide.";
    }
    public void OnInteraction()
    {
        if (player)
        {
            if (player.GetIsHiding() == false)
            {
                //save player position prior to hiding
                player.SetPrevHidePos(player.transform.position);
                Debug.Log("HIDE HERE");
            }
            player.ToggleIsHiding();
            if (player.GetIsHiding() == false)//place player at last unhidden position (slerp when possible)
            {
                //StartCoroutine(PutPlayerAtUnhiddenPos());
            }
        }
    }
    private IEnumerator PutPlayerAtUnhiddenPos() 
    {
        yield return new WaitForSeconds(0.25f);
        Debug.Log("Unhide");
        player.transform.position = player.GetPrevHidePos();
        StopCoroutine(PutPlayerAtUnhiddenPos());
    }
    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerControls>();
        if (player && player.GetEntityType() == EEntityType.Player)
        {
            Debug.Log("near hiding place");
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
