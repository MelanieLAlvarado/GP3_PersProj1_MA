using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RoamAction", story: "[Self] Roam", category: "Action", id: "e9548984348f50649ee41894775afd54")]
public partial class RoamActionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnUpdate()
    {
        Self.Value.GetComponent<EnemyAI>().Roam();
        return Status.Success;
    }
}

