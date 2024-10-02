using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasAudibleNoisesCondition", story: "[Self] has AudibleNoisesPresent [Condition]", category: "Conditions", id: "3b1151b80a7f0d97ec383162463d0299")]
public partial class HasAudibleNoisesCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> Condition;

    public override bool IsTrue()
    {
        HearingComponent hearing = Self.Value.GetComponent<HearingComponent>();
        bool hasAudibleTargets = hearing != null && hearing.GetIsAudibleNoisesPresent();
        return hasAudibleTargets;
    }
}
