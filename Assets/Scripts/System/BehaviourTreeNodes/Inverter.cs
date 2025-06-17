using UnityEngine;
using System.Collections;

public class Inverter : Node 
{
    private Node m_node;

    public Node node 
    {
        get { return m_node; }
    }

    public Inverter(Node node) 
    {
        m_node = node;
    }

    public override NodeStates Evaluate() 
    {
        switch (m_node.Evaluate()) 
        {
            case NodeStates.FAILURE:
                m_nodeState = NodeStates.SUCCESS;
                return m_nodeState;
            case NodeStates.SUCCESS:
                m_nodeState = NodeStates.FAILURE;
                return m_nodeState;
            case NodeStates.RUNNING:
                m_nodeState = NodeStates.RUNNING;
                return m_nodeState;
        }
        m_nodeState = NodeStates.SUCCESS;
        return m_nodeState;
    }
}
