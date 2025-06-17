using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionNode : Node
{
    protected Func<bool> condition;

    public ConditionNode(Func<bool> _condition)
    {
        condition = _condition;
    }

    public override NodeStates Evaluate()
    {
        return condition() ? NodeStates.SUCCESS : NodeStates.FAILURE;
    }
}
