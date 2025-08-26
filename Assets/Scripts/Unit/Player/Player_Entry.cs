using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameStuff;

public partial class Player : Character
{
    //quest state
    public NPCTalkCount TalkCount = new NPCTalkCount();
    public EnemyKillCount KillCount = new EnemyKillCount();
    public QuestStateInstances QuestStateInstance = new QuestStateInstances();

    public event Action OnQuestStateChanged;

    public void AddQuest(int _questID, QuestState _state)
    {
        if (QuestStateInstance.QuestStates.ContainsKey(_questID))
            return;

        var questInstance = new QuestInstance(_questID, _state);
        QuestStateInstance.QuestStates[_questID] = questInstance;
        QuestStateInstance.BuildTargetIndex();

        OnQuestStateChanged?.Invoke();
        
        SavePlayerDataWithoutPosition();
    }

    public void AddQuest(QuestInstance _quest)
    {
        if (QuestStateInstance.QuestStates.ContainsKey(_quest.QuestID))
        {
            QuestStateInstance.QuestStates[_quest.QuestID] = _quest;
            Debug.Log($"<color=green>{_quest.QuestID}</color>: 퀘스트 상태가 다음으로 변경됨 : <color=green>{QuestStateInstance.QuestStates[_quest.QuestID].State}</color>");
        }
        else
        {
            QuestStateInstance.QuestStates.Add(_quest.QuestID, _quest);
            Debug.Log($"<color=green>{_quest.QuestID} 퀘스트 추가됨</color>");
        }

        OnQuestStateChanged?.Invoke();
        QuestStateInstance.BuildTargetIndex();
        
        SavePlayerDataWithoutPosition();
    }

    public void LoadQuestData(List<QuestInstanceData> _questData, List<TalkCountData> _talkCountData, List<KillCountData> _killCountData)
    {
        if (_questData == null) _questData = new List<QuestInstanceData>();
        if (_talkCountData == null) _talkCountData = new List<TalkCountData>();
        if (_killCountData == null) _killCountData = new List<KillCountData>();

        // 퀘스트 상태 로드
        QuestStateInstance.QuestStates.Clear();
        for (int i = 0; i < _questData.Count; i++)
        {
            var questData = _questData[i];
            if (questData != null)
            {
                var questInstance = new QuestInstance(questData.QuestID, questData.State);
                
                if (questData.ObjectivesList != null)
                {
                    var objectivesDict = new Dictionary<int, QuestObjectiveInstance>();
                    
                    for (int j = 0; j < questData.ObjectivesList.Count; j++)
                    {
                        var objectiveData = questData.ObjectivesList[j];
                        if (objectiveData != null)
                        {
                            QuestObjectiveInstance objective;
                            
                            switch (objectiveData.Type)
                            {
                                case ObjectiveType.Kill:
                                    objective = new KillObjectiveInstance(objectiveData.Type, objectiveData.Required);
                                    break;
                                case ObjectiveType.Collect:
                                    objective = new CollectObjectiveInstance(objectiveData.Type, objectiveData.Required);
                                    break;
                                default:
                                    objective = new QuestObjectiveInstance(objectiveData.Type);
                                    break;
                            }
                            
                            objective.ObjectiveIndex = objectiveData.ObjectiveIndex;
                            objective.TargetID = objectiveData.TargetID;
                            objective.Completed = objectiveData.Completed;
                            
                            if (objective is KillObjectiveInstance kill)
                            {
                                kill.Current = objectiveData.Current;
                            }
                            else if (objective is CollectObjectiveInstance collect)
                            {
                                collect.Current = objectiveData.Current;
                            }
                            
                            objectivesDict[objectiveData.ObjectiveIndex] = objective;
                        }
                    }
                    
                    questInstance = new QuestInstance(questData.QuestID, questData.State, objectivesDict);
                }
                
                QuestStateInstance.QuestStates[questData.QuestID] = questInstance;
            }
        }
        QuestStateInstance.BuildTargetIndex();
        
        // 대화 기록 로드
        TalkCount.TalkCount.Clear();
        for (int i = 0; i < _talkCountData.Count; i++)
        {
            var talkData = _talkCountData[i];
            TalkCount.TalkCount[talkData.NPCID] = talkData.Count;
        }
        
        // 사냥 기록 로드
        KillCount.KillCount.Clear();
        for (int i = 0; i < _killCountData.Count; i++)
        {
            var killData = _killCountData[i];
            KillCount.KillCount[killData.EnemyID] = killData.Count;
        }
        
        OnQuestStateChanged?.Invoke();
    }
}

public class QuestObjectiveIndex
{
    public int QuestID;
    public int ObjectiveIndex;
    public ObjectiveType Type;

    public QuestObjectiveIndex(int _questID, int _objectiveIndex, ObjectiveType _type)
    {
        QuestID = _questID;
        ObjectiveIndex = _objectiveIndex;
        Type = _type;
    }
}

public class QuestStateInstances
{
    public Dictionary<int, QuestInstance> QuestStates = new Dictionary<int, QuestInstance>();
    private Dictionary<int, List<QuestObjectiveIndex>> targetToQuestMap = new();

    public QuestInstance GetQuestState(int _questID)
    {
        if (QuestStates.ContainsKey(_questID))
            return QuestStates[_questID];
        return null;
    }

    public void BuildTargetIndex()
    {
        targetToQuestMap.Clear();
        var _quests = QuestStates.Values.ToList();
        int questCount = _quests.Count;
        for (int i = 0; i < questCount; i++)
        {
            var _quest = _quests[i];

            if (_quest.Objectives == null || _quest.Objectives.Count == 0)
                continue;

            var _objectives = _quest.Objectives.Values.ToList();
            int objCount = _objectives.Count;
            for (int j = 0; j < objCount; j++)
            {
                var _obj = _objectives[j];
                if (!targetToQuestMap.ContainsKey(_obj.TargetID))
                    targetToQuestMap[_obj.TargetID] = new List<QuestObjectiveIndex>();
                targetToQuestMap[_obj.TargetID].Add(new QuestObjectiveIndex(_quest.QuestID, _obj.ObjectiveIndex, _obj.Type));
            }
        }
    }

    public void OnTargetEvent(int _targetID, ObjectiveType _type, int _progress = 1)
    {
        if (!targetToQuestMap.TryGetValue(_targetID, out var questList)) return;
        int questListCount = questList.Count;
        bool questDataChanged = false;
        
        for (int i = 0; i < questListCount; i++)
        {
            var index = questList[i];
            if (index.Type != _type) continue;

            if (!QuestStates.TryGetValue(index.QuestID, out var questInstance)) continue;
            if (questInstance.State != QuestState.Accepted) continue;

            if (questInstance.Objectives.TryGetValue(index.ObjectiveIndex, out var objective))
            {
                objective.SetQuestObjective(_progress);
                questDataChanged = true;

                var objectives = questInstance.Objectives.Values.ToList();
                int objCount = objectives.Count;
                bool allCompleted = true;
                for (int j = 0; j < objCount; j++)
                {
                    if (!objectives[j].Completed)
                    {
                        allCompleted = false;
                        break;
                    }
                }
                if (allCompleted && questInstance.State == QuestState.Accepted)
                {
                    questInstance.State = QuestState.Achieved;
                    Singleton.Player.AddQuest(questInstance.QuestID, QuestState.Achieved);
                    questDataChanged = true;
                }
            }
        }
        
        // 퀘스트 데이터 변경 시 플레이어 데이터 저장 (위치 제외)
        if (questDataChanged)
        {
            Singleton.Player.SavePlayerDataWithoutPosition();
        }
    }
}

public class NPCTalkCount
{
    public Dictionary<int, int> TalkCount = new Dictionary<int, int>();

    public void AddTalkCount(int _npcID)
    {
        if (TalkCount.ContainsKey(_npcID))
            TalkCount[_npcID]++;
        else
            TalkCount.Add(_npcID, 1);

        Singleton.Player.QuestStateInstance.OnTargetEvent(_npcID, ObjectiveType.Interact, TalkCount[_npcID]);
        Debug.Log($"{_npcID} 대화 카운트 증가. <color=orange>현재 카운트: {TalkCount[_npcID]}</color>");
        
        Singleton.Player.SavePlayerDataWithoutPosition();
    }

    public int GetTalkCount(int _npcID)
    {
        if (TalkCount.ContainsKey(_npcID))
            return TalkCount[_npcID];

        return 0;
    }
}

public class EnemyKillCount
{
    public Dictionary<int, int> KillCount = new Dictionary<int, int>();

    public void AddKillCount(int _enemyID)
    {
        if (KillCount.ContainsKey(_enemyID))
            KillCount[_enemyID]++;
        else
            KillCount.Add(_enemyID, 1);

        Singleton.Player.QuestStateInstance.OnTargetEvent(_enemyID, ObjectiveType.Kill, KillCount[_enemyID]);
        Debug.Log($"{_enemyID} 킬 카운트 증가. <color=orange>현재 카운트: {KillCount[_enemyID]}</color>");
        
        Singleton.Player.SavePlayerDataWithoutPosition();
    }

    public int GetKillCount(int _enemyID)
    {
        if (KillCount.ContainsKey(_enemyID))
            return KillCount[_enemyID];

        return 0;
    }
}
