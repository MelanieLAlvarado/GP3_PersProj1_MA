using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasVisualTargets", story: "[Self] has VisualTargets [Condition]", category: "Conditions", id: "04f9b1b09bd9478431782b652d04b651")]
public partial class HasVisualTargetsCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> Condition;

    public override bool IsTrue()
    {
        VisionComponent vision = Self.Value.GetComponent<VisionComponent>();
        bool hasVisualTargets = vision != null && vision.GetCurrentSensibleStimuliSetIsntEmpty();
        return hasVisualTargets;
    }
}
