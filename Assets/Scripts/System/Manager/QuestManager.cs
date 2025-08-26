using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

using GameStuff;

public class QuestManager : MonoBehaviour
{
    private int targetedQuestID = 0;
    private List<QuestNode> rootNodes = new List<QuestNode>();
    private Dictionary<int, QuestNode> questNodeMap = new Dictionary<int, QuestNode>();
    private Dictionary<int, QuestInfo> quests = new Dictionary<int, QuestInfo>();
    private Dictionary<int, ObjectiveInfo> questObjectiveRewards = new Dictionary<int, ObjectiveInfo>();
    private string dataPath;

    public Action<int> OnTargetQuestChanged;

    private async void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        await LoadQuestData();
    }

    private void Start()
    {
        CreateTree();
    }

    private async Task LoadQuestData()
    {
        QuestDataContainer questWrapper = await Singleton.Get<AuthManager>().GetDataAsync<QuestDataContainer>(Request.quests);

        quests.Clear();
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

            for (int j = 0; j < info.NextQuestIDs.Count; j++)
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

        Singleton.Player?.AddQuest(instance);

        if (instance.State == QuestState.Accepted && targetedQuestID == 0) SetTargetedQuest(instance.QuestID);
    }

    public void DeclineQuest(int _id)
    {
        // 퀘스트 거절 처리 (무시)
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
        Singleton.Player?.AddQuest(questInstance);

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
        Singleton.Player?.SavePlayerDataWithoutPosition();
        targetedQuestID = 0;
        OnTargetQuestChanged?.Invoke(targetedQuestID);
        return true;
    }

    private void GiveQuestReward(QuestInfo _questInfo)
    {
        Singleton.Inventory?.AddCurrency((uint)_questInfo.Reward.Currency);

        var items = _questInfo.Reward.itemIds;
        var itemFactory = Singleton.Get<ItemFactory>();

        int count = items.Count;
        for (int i = 0; i < count; i++)
        {
            // ItemFactory를 사용하여 아이템 생성 (드롭 시스템과 동일한 방식)
            var itemInstance = itemFactory.CreateItem(items[i], true);
            
            if (itemInstance is IStackable stackable)
            {
                stackable.CurrentStack = 30;
            }

            if (itemInstance != null)
            {
                Singleton.Inventory?.TakeItem(itemInstance);
                Debug.Log($"퀘스트 보상 아이템 지급: {itemInstance.ItemID}");
            }
            else
            {
                Debug.LogError($"퀘스트 보상 아이템 생성 실패: {items[i]}");
            }
        }
    }

    public void SetTargetedQuest(int _questID)
    {
        // 이미 같은 퀘스트가 타겟되어 있으면 토글 (닫기)
        if (targetedQuestID == _questID)
        {
            targetedQuestID = 0;
        }
        else
        {
            targetedQuestID = _questID;
        }
        
        OnTargetQuestChanged?.Invoke(targetedQuestID);
    }

    public int GetTargetedQuest()
    {
        return targetedQuestID;
    }
}