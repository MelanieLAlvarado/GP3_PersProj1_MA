using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "[self] Chases", category: "Action", id: "03e33aa73303bc2f567279411b90c300")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnUpdate()
    {
        Self.Value.GetComponent<EnemyAI>().Chase();
        return Status.Success;
    }
}

