using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class RandomSelector : Node
{
    protected List<Node> nodes = new List<Node>();
    int count = 0;
    int select = 0;
    float time = 0f;
    float interval = 1f;

    public RandomSelector(List<Node> _nodes, float _resetInterval = 1f)
    {
        nodes = _nodes;
        count = nodes.Count;
        interval = _resetInterval;
        select = Random.Range(0, count);
    }

    public override NodeStates Evaluate()
    {
        time += Time.deltaTime;

        if (time >= interval)
        {
            time = 0f;
            for(int i = 0; i < 100; i++)
            {
                int tempSelect = Random.Range(0, count);
                if (select == tempSelect)
                {
                    select = Random.Range(0, count);
                    continue;
                }
                break;
            }
        }

        Node selected = nodes[select];

        switch (selected.Evaluate())
        {
            case NodeStates.FAILURE:
                break;
            case NodeStates.SUCCESS:
                m_nodeState = NodeStates.SUCCESS;
                return m_nodeState;
            case NodeStates.RUNNING:
                m_nodeState = NodeStates.RUNNING;
                return m_nodeState;
            default:
                break;
        }

        m_nodeState = NodeStates.FAILURE;
        return m_nodeState;
    }
}
