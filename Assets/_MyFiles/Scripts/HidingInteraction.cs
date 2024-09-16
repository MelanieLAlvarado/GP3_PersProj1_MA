using System.Collections;
using UnityEngine;

public class HidingInteraction : MonoBehaviour, IInterActions
{
    PlayerControls _player;
    [SerializeField] private Transform hidePos;
    Transform _playerLastPos; //will used for remembering the player's location before hiding

    public Transform GetHidePos() { return hidePos; } //(WIP)
    public EEntityType GetEntityType()
    {
        return EEntityType.HidingSpace;
    }
    public string GetInteractionMessage() 
    {
        if (_player.GetIsHiding() == false)
        {
            return "Press 'E' to hide.";
        }
        return "Press 'E' to leave";
    }
    public void OnInteraction() 
    {
        if (_player)
        {
            if (_player.GetIsHiding() == false)
            {
                //save player position prior to hiding (WIP)
                _player.SetPrevHidePos(_player.transform.position);
                Debug.Log("HIDE HERE");
            }
            _player.ToggleIsHiding();
            if (_player.GetIsHiding() == false)//place player at last unhidden position (slerp when possible) (WIP)
            {
                //StartCoroutine(PutPlayerAtUnhiddenPos());//(WIP)
            }
            GameManager.m_Instance.GetUIManager().SetInteractionText(GetInteractionMessage());
        }
    }
    private IEnumerator PutPlayerAtUnhiddenPos() //will be used to place player at hiding spot (might move)
    {
        yield return new WaitForSeconds(0.25f);
        Debug.Log("Unhide");
        //_player.transform.position = _player.GetPrevHidePos();
        StopCoroutine(PutPlayerAtUnhiddenPos());
    }
    private void OnTriggerEnter(Collider other)
    {
        _player = other.GetComponent<PlayerControls>();
        if (_player && _player.GetEntityType() == EEntityType.Player)
        {
            //Debug.Log("near hiding place");
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
