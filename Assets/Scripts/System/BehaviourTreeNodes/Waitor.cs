using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waitor : Node
{
    Node node;

    float waitTime = 0f;
    float time = 0;

    public Waitor(float _waitTime, Node _node = null)
    {
        waitTime = _waitTime;
        node = _node;
    }

    public override NodeStates Evaluate()
    {
        time += Time.deltaTime;

        //수초 뒤 success 반환
        if (time >= waitTime)
        {
            time = 0f;
            if (node != null) return node.Evaluate();
            return NodeStates.FAILURE;
        }

        return NodeStates.SUCCESS;
    }
}
