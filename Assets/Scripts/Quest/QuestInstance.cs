using System;
using System.Collections.Generic;

using GameStuff;

[Serializable]
public class QuestInstance
{
    public int QuestID;
    public QuestState State;
    public Dictionary<int, QuestObjectiveInstance> Objectives;

    public QuestInstance(int _id, QuestState _state)
    {
        QuestID = _id;
        State = _state;
        Objectives = new Dictionary<int, QuestObjectiveInstance>();
    }

    public QuestInstance(int _id, QuestState _state, Dictionary<int, QuestObjectiveInstance> _objectives)
    {
        QuestID = _id;
        State = _state;
        Objectives = _objectives ?? new Dictionary<int, QuestObjectiveInstance>();
    }

    public void AddObjectives(ObjectiveInfo _info)
    {
        if (Objectives.ContainsKey(_info.ObjectiveIndex))
        {
            if (Objectives[_info.ObjectiveIndex] != null) return;

            Objectives.Remove(_info.ObjectiveIndex);
        }

        QuestObjectiveInstance instance = QuestObjectiveFactory.Create(_info);

        if (instance == null) return;

        instance.TargetID = _info.TargetID;
        instance.ObjectiveIndex = _info.ObjectiveIndex;

        Objectives.Add(_info.ObjectiveIndex, instance);
    }

    public T GetObjectiveInstance<T>(int _id) where T : QuestObjectiveInstance
    {
        return Objectives[_id] as T;
    }
}

[Serializable]
public class QuestObjectiveInstance
{
    public int ObjectiveIndex;
    public ObjectiveType Type;
    public int TargetID;
    public bool Completed = false;

    public QuestObjectiveInstance(ObjectiveType _type) { Type = _type; }

    public void SetQuestObjective(int _count = 0)
    {
        if (this is KillObjectiveInstance kill)
        {
            kill.Current = _count;
            Completed = kill.Current >= kill.Required;
        }
        else if (this is CollectObjectiveInstance collect)
        {
            collect.Current = _count;
            Completed = collect.Current >= collect.Required;
        }
        else
        {
            Completed = _count >= 1;
        }
    }
}

[Serializable]
public class KillObjectiveInstance : QuestObjectiveInstance
{
    public int Current = 0;
    public int Required;

    public KillObjectiveInstance(ObjectiveType _type, int _required) : base(_type)
    {
        Required = _required;
    }
}

[Serializable]
public class CollectObjectiveInstance : QuestObjectiveInstance
{
    public int Current = 0;
    public int Required;

    public CollectObjectiveInstance(ObjectiveType _type, int _required) : base(_type)
    {
        Required = _required;
    }
}

public static class QuestObjectiveFactory
{
    private static Dictionary<ObjectiveType, Func<ObjectiveInfo, QuestObjectiveInstance>> objectiveTypeMap = new Dictionary<ObjectiveType, Func<ObjectiveInfo, QuestObjectiveInstance>>
    {
        { ObjectiveType.Interact, (info) => new QuestObjectiveInstance(ObjectiveType.Interact) },
        { ObjectiveType.Kill, (info) => new KillObjectiveInstance(ObjectiveType.Kill, info.Required) },
        { ObjectiveType.Collect, (info) => new CollectObjectiveInstance(ObjectiveType.Collect, info.Required) }
    };

    public static QuestObjectiveInstance Create(ObjectiveInfo _info)
    {
        if (objectiveTypeMap.TryGetValue(_info.ObjectiveType, out var factory))
            return factory(_info);

        return null;
    }
}


