using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasRoamingComp", story: "[self] Has RoamingComponent [Condition]", category: "Conditions", id: "481c50391c7d34d7101228819038c820")]
public partial class HasRoamingCompCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> Condition;

    public override bool IsTrue()
    {
        bool hasRoamComponent = false;
        if (Self.Value.GetComponent<RoamComponent>())
        { 
            hasRoamComponent = true;
        }
        return hasRoamComponent;
    }

}
