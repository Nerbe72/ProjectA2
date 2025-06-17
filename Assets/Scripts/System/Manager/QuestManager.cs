using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public int InitializationPriority => 1;

    private Player player;
    private Inventory inventory;

    private int targetedQuestID = 0;
    private List<QuestNode> rootNodes = new List<QuestNode>();
    private Dictionary<int, QuestNode> questNodeMap = new Dictionary<int, QuestNode>();
    private Dictionary<int, QuestInfo> quests = new Dictionary<int, QuestInfo>();
    private Dictionary<int, ObjectiveInfo> questObjectiveRewards = new Dictionary<int, ObjectiveInfo>();
    private string dataPath;

    public Action<int> OnTargetQuestChanged;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        LoadQuestData();
    }

    private void Start()
    {
        player = Singleton.Player;
        inventory = Singleton.Inventory;
        CreateTree();
    }

    private void LoadQuestData()
    {
        dataPath = Path.Combine(Application.persistentDataPath, "quests.json");

        var text = File.ReadAllText(dataPath);
        var questWrapper = JsonUtility.FromJson<QuestDataContainer>(text);

        int count = questWrapper.questList.Count;
        for (int i = 0; i < count; i++)
        {
            quests.Add(questWrapper.questList[i].QuestID, questWrapper.questList[i]);
        }
    }

    private void CreateTree()
    {
        var questList = new List<QuestInfo>(quests.Values);
        int count = questList.Count;
        for (int i = 0; i < questList.Count; i++)
        {
            var info = questList[i];
            questNodeMap.Add(info.QuestID, new QuestNode(info));
        }

        for (int i = 0; i < count; i++)
        {
            var info = questList[i];
            var node = questNodeMap[info.QuestID];

            for(int j = 0; j < info.NextQuestIDs.Count; j++)
            {
                int nextQuestID = info.NextQuestIDs[j];
                if (questNodeMap.TryGetValue(nextQuestID, out var nextNode))
                {
                    node.AddNextQuest(nextNode);
                    nextNode.Parent = node;
                }
            }
        }

        var rootNodes = questNodeMap.Values.Where(n => n.Info.PrerequisiteQuestID == 0).ToList();

        foreach (var node in questNodeMap.Values)
        {
            foreach (var child in node.Next)
            {
                if (child.Value.Parent != node)
                    Debug.LogError($"노드 구조 오류: {node.Info.QuestID} -> {child.Value.Info.QuestID}");
            }
        }

        for (int i = 0; i < rootNodes.Count; i++)
        {
            AddQuest(rootNodes[i].Info.QuestID, QuestState.Available);
        }
    }
    
    private void LoadPlayerQuestInstance()
    {

    }

    public QuestInfo GetQuestInfo(int _id)
    {
        if (quests.ContainsKey(_id)) return quests[_id];

        return null;
    }

    public void AddQuest(int _id, QuestState _addedState = QuestState.Accepted)
    {
        var quest_selected = quests[_id];

        QuestStateInstances states = Singleton.Player.QuestStateInstance;

        QuestInstance instance = states.GetQuestState(_id);

        if (instance == null)
        {
            
            instance = new QuestInstance(_id, _addedState);

            int count = quest_selected.Objectives.Count;
            for (int i = 0; i < count; i++)
            {
                instance.AddObjectives(quest_selected.Objectives[i]);
            }
        }
        else
        {
            instance.State = QuestState.Accepted;
        }

        player.AddQuest(instance);

        if (instance.State == QuestState.Accepted && targetedQuestID == 0) SetTargetedQuest(instance.QuestID);
    }

    public void DeclineQuest(int _id)
    {
        // TODO: 퀘스트 거절 처리
    }

    public bool CompleteQuest(int _id)
    {
        var states = Singleton.Player.QuestStateInstance;
        var questInstance = states.GetQuestState(_id);
        if (questInstance == null) return false;

        // 모든 목표가 완료되었는지 확인
        bool allCompleted = true;
        foreach (var obj in questInstance.Objectives.Values)
        {
            if (!obj.Completed)
            {
                allCompleted = false;
                break;
            }
        }
        if (!allCompleted) return false;

        // 상태를 완료로 변경
        questInstance.State = QuestState.Completed;
        player.AddQuest(questInstance);

        // 보상 지급
        var questInfo = GetQuestInfo(_id);
        if (questInfo != null)
            GiveQuestReward(questInfo);

        // 다음 퀘스트 자동 활성화
        if (questInfo != null && questInfo.NextQuestIDs != null)
        {
            foreach (var nextId in questInfo.NextQuestIDs)
            {
                AddQuest(nextId, QuestState.Available);
            }
        }

        Debug.Log($"퀘스트 완료: {_id}");
        targetedQuestID = 0;
        OnTargetQuestChanged?.Invoke(targetedQuestID);
        return true;
    }

    private void GiveQuestReward(QuestInfo _questInfo)
    {
        inventory.AddCurrency((uint)_questInfo.Reward.Currency);
    }

    public void SetTargetedQuest(int _questID)
    {
        targetedQuestID = _questID;
        OnTargetQuestChanged?.Invoke(targetedQuestID);
    }

    public int GetTargetedQuest()
    {
        return targetedQuestID;
    }
}