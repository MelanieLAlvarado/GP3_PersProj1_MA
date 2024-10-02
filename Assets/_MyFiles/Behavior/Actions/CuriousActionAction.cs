using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CuriousAction", story: "[self] Curious", category: "Action", id: "64b5da7d69521d4fa407dc3eab5ceb46")]
public partial class CuriousActionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Self.Value.GetComponent<EnemyAI>().Curious();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

