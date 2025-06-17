using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Selector : Node 
{
    protected List<Node> m_nodes = new List<Node>();

    public Selector(List<Node> nodes) 
    {
        m_nodes = nodes;
    }

    public override NodeStates Evaluate() 
    {
        foreach (Node node in m_nodes) 
        {
            switch (node.Evaluate())
            {
                case NodeStates.FAILURE:
                    continue;
                case NodeStates.SUCCESS:
                    m_nodeState = NodeStates.SUCCESS;
                    return m_nodeState;
                case NodeStates.RUNNING:
                    m_nodeState = NodeStates.RUNNING;
                    return m_nodeState;
                default:
                    continue;
            }
        }
        m_nodeState = NodeStates.FAILURE;
        return m_nodeState;
    }
}
