using Unity.VisualScripting;
using UnityEngine;

public interface IInterActions
{
    public EEntityType GetEntityType();
    public string GetInteractionMessage();
    void OnInteraction();
}
