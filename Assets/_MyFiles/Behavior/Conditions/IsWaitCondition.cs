using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsWaitCondition", story: "[self] is waiting [Condition]", category: "Conditions", id: "8781de3fc98090f87f8c947047fbd238")]
public partial class IsWaitCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> Condition;

    public override bool IsTrue()
    {
        EnemyAI enemy = Self.Value.GetComponent<EnemyAI>();
        return enemy.GetIsWait();
    }
}
