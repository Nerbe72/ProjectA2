using System;
using System.Collections.Generic;

[Serializable]
public class QuestProgress
{
    public int QuestID;
    public List<ObjectiveProgress> objectiveProgresses;

    public QuestProgress()
    {
        objectiveProgresses = new List<ObjectiveProgress>();
    }
}

[Serializable]
public class ObjectiveProgress
{
    public int TargetID;
}

[Serializable]
public class KillProgress : ObjectiveProgress
{
    public int Required;
    public int Current;
}

[Serializable]
public class CollectProgress : ObjectiveProgress
{
    public int Required;
    public int Current;
}

[Serializable]
public class InteractProgress : ObjectiveProgress
{
    public bool Interacted;
}

//퀘스트 데이터

[System.Serializable]
public class QuestDataContainer
{
    public List<QuestInfo> questList;
}

[System.Serializable]
public class QuestInfo
{
    public int QuestID;
    public int NameID;
    public int DescriptionID;
    public int Type;
    public int PrerequisiteQuestID;
    public List<int> NextQuestIDs;
    public int ReceiverNPCID;
    public bool Repeatable;
    public List<ObjectiveInfo> Objectives;
    public RewardInfo Reward;
}

[System.Serializable]
public class ObjectiveInfo
{
    public int ObjectiveIndex;
    public int TargetID;
    public int Required;
    public bool Interacted;
    public ObjectiveType ObjectiveType;

    public string GetLocalizedDescription()
    {
        var tableMgr = Singleton.Get<TableDataManager>().Table;
        var localeTable = tableMgr.Locale;

        string objectiveDescription = "";
        switch (ObjectiveType)
        {
            case ObjectiveType.Kill:
                var enemy_selected = tableMgr.Enemy.Get(TargetID);
                string enemyName = enemy_selected != null ? localeTable.Get(enemy_selected.EnemyID, GameManager.CurrentLocale) : TargetID.ToString();
                string killFormat = localeTable.Get(82000001, GameManager.CurrentLocale);
                string[] killDatas = { enemyName, Required.ToString() };
                objectiveDescription = string.Format(killFormat, killDatas);
                break;
            case ObjectiveType.Collect:
                var itemInfo = tableMgr.Item.Get(TargetID);
                string itemName = itemInfo != null ? tableMgr.Locale.Get(itemInfo.Name, GameManager.CurrentLocale) : TargetID.ToString();
                string collectFormat = localeTable.Get(82000002, GameManager.CurrentLocale);
                string[] collectDatas = { itemName, Required.ToString() };
                objectiveDescription = string.Format(collectFormat, collectDatas);
                break;
            case ObjectiveType.Interact:
                var npcInfo = tableMgr.NPC.Get(TargetID);
                string npcName = npcInfo != null ? tableMgr.Locale.Get(npcInfo.NameID, GameManager.CurrentLocale) : localeTable.Get(npcInfo.NameID, GameManager.CurrentLocale);
                string interactFormat = localeTable.Get(82000003, GameManager.CurrentLocale);
                string[] interactDatas = { npcName, interactFormat };
                objectiveDescription = string.Format(interactFormat, interactDatas);
                break;
            default:
                objectiveDescription = "";
                break;
        }

        return objectiveDescription;
    }
}

[System.Serializable]
public class RewardInfo
{
    public int Currency;
    public List<int> itemIds;
}