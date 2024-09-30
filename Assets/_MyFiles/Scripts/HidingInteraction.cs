using System.Collections;
using UnityEngine;

public class HidingInteraction : MonoBehaviour, IInterActions
{
    PlayerControls _player;
    [SerializeField] private Transform hidePos;
    Transform _playerLastPos; //will used for remembering the player's location before hiding
    [SerializeField][Range(0, 5)] private float detectionRange = 2f; ///Edit sphere collider size on start
    private void Start()
    {
        SphereCollider detectionCollider = gameObject.AddComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRange;
    }
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
            if (_player.GetIsHideLerp() == false && !_player.GetIsHiding())
            {
                //save player position prior to hiding (WIP)
                _player.SetPrevHidePos(_player.transform);
            }
            _player.ToggleIsHiding();
            GameManager.m_Instance.GetUIManager().SetInteractionText(GetInteractionMessage());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControls>())
        {
            _player = other.GetComponent<PlayerControls>();
            if (_player.GetEntityType() == EEntityType.Player)
            { 
                //Debug.Log("near hiding place");
                _player.SetTargetInteractible(this.gameObject);
            }
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
