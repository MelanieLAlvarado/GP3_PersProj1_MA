using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ProcessWaitAction", story: "[self] process wait", category: "Action", id: "7ebaf6d4a84f5dea1995748d2cc005a0")]
public partial class ProcessWaitAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnUpdate()
    {
        Self.Value.GetComponent<EnemyAI>().ProcessWait();
        return Status.Success;
    }
}

