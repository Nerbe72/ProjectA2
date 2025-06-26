//액션을 설정할 리프 노드이다. 실행할 행동을 대리자delegate로 지정할 수 있다.
public class ActionNode : Node
{
    public delegate NodeStates ActionNodeDelegate();

    private ActionNodeDelegate m_action;

    public ActionNode(ActionNodeDelegate action)
    {
        m_action = action;
    }

    public override NodeStates Evaluate()
    {
        switch (m_action())
        {
            case NodeStates.SUCCESS:
                m_nodeState = NodeStates.SUCCESS;
                break;
            case NodeStates.FAILURE:
                m_nodeState = NodeStates.FAILURE;
                break;
            case NodeStates.RUNNING:
                m_nodeState = NodeStates.RUNNING;
                break;
            default:
                m_nodeState = NodeStates.FAILURE;
                break;
        }
        return m_nodeState;
    }

}
