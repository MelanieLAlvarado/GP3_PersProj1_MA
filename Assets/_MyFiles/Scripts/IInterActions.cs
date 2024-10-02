using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public interface IInterActions
{
    public EEntityType GetEntityType();///to get the type of object if needed
    public string GetInteractionMessage();///message to be displayed on UI
    void OnInteraction();///When interacted with by player or enemy
}
