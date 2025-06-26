using System.Collections.Generic;
using UnityEngine;

public class NPCQuestRoot : MonoBehaviour
{
    public int NPCID;
    public Dictionary<int, QuestNode> Quests = new Dictionary<int, QuestNode>();

    public NPCQuestRoot(int _npcID)
    {
        NPCID = _npcID;
    }

    public void ReBuild()
    {

    }
}

public class QuestNode
{
    public QuestInfo Info;
    public QuestNode Parent;
    //public Dictionary<QuestState, int> states = new Dictionary<QuestState, int>();
    public Dictionary<int, QuestNode> Next = new Dictionary<int, QuestNode>();

    public QuestNode(QuestInfo _info, QuestNode _parent = null)//, int _available, int _accepted, int _completed)
    {
        Info = _info;
        Parent = _parent;
        //states.Add(QuestState.Available, _available);
        //states.Add(QuestState.Accepted, _accepted);
        //states.Add(QuestState.Completed, _completed);
    }

    public void AddNextQuest(QuestNode _node)
    {
        Parent = this;
        Next.Add(_node.Info.QuestID, _node);
    }
}
