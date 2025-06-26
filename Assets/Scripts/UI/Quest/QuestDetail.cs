using TMPro;
using UnityEngine;

public class QuestDetail : MonoBehaviour
{
    [SerializeField] private TMP_Text questName;
    [SerializeField] QuestColumn objective;
    [SerializeField] private QuestColumn reward;
    [SerializeField] private QuestColumn description;

    private QuestManager questManager;
    private Player player;
    private TableLocale localeTable;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetData(int _id)
    {
        if (questManager == null) questManager = Singleton.Get<QuestManager>();
        if (player == null) player = Singleton.Player;
        if (localeTable == null) localeTable = Singleton.Get<TableDataManager>().Table.Locale;

        Locale currentLocale = GameManager.CurrentLocale;
        var quest_selected = questManager.GetQuestInfo(_id);
        var progress_selected = player.QuestStateInstance.GetQuestState(_id);

        questName.text = localeTable.Get(quest_selected.NameID, currentLocale);
        objective.Title.text = localeTable.Get(10000031, currentLocale);
        reward.Title.text = localeTable.Get(10000032, currentLocale);
        description.Title.text = localeTable.Get(10000033, currentLocale);

        // 설명
        description.Detail.text = localeTable.Get(quest_selected.DescriptionID, currentLocale);

        // 보상
        string rewardText = string.Empty;
        if (quest_selected.Reward != null)
        {
            if (quest_selected.Reward.Currency > 0)
            {
                rewardText += $"$ {quest_selected.Reward.Currency}\n";
            }
            if (quest_selected.Reward.itemIds != null && quest_selected.Reward.itemIds.Count > 0)
            {
                var tableMgr = Singleton.Get<TableDataManager>().Table;
                foreach (var itemId in quest_selected.Reward.itemIds)
                {
                    var itemInfo = tableMgr.Item.Get(itemId);
                    if (itemInfo != null)
                    {
                        string itemName = tableMgr.Locale.Get(itemInfo.Name, currentLocale);
                        rewardText += $"{itemName}\n";
                    }
                }
            }
        }
        reward.Detail.text = rewardText.Trim();

        // 목표
        string objectiveText = string.Empty;
        int count = quest_selected.Objectives.Count;
        if (quest_selected.Objectives != null && count > 0)
        {
            var tableMgr = Singleton.Get<TableDataManager>().Table;
            for (int i = 0; i < count; i++)
            {
                string objectiveDescription = "";
                ObjectiveInfo objectiveInfo = quest_selected.Objectives[i];
                switch (objectiveInfo.ObjectiveType)
                {
                    case ObjectiveType.Kill:
                        var enemy_selected = tableMgr.Enemy.Get(objectiveInfo.TargetID);
                        string enemyName = enemy_selected != null ? localeTable.Get(enemy_selected.Name, currentLocale) : objectiveInfo.TargetID.ToString();
                        string killFormat = localeTable.Get(82000001, currentLocale);
                        string[] killDatas = { enemyName, objectiveInfo.Required.ToString() };
                        objectiveDescription = string.Format(killFormat, killDatas);
                        break;
                    case ObjectiveType.Collect:
                        var itemInfo = tableMgr.Item.Get(objectiveInfo.TargetID);
                        string itemName = itemInfo != null ? tableMgr.Locale.Get(itemInfo.Name, currentLocale) : objectiveInfo.TargetID.ToString();
                        string collectFormat = localeTable.Get(82000002, currentLocale);
                        string[] collectDatas = { itemName, objectiveInfo.Required.ToString() };
                        objectiveDescription = string.Format(collectFormat, collectDatas);
                        break;
                    case ObjectiveType.Interact:
                        var npcInfo = tableMgr.NPC.Get(objectiveInfo.TargetID);
                        string npcName = npcInfo != null ? tableMgr.Locale.Get(npcInfo.NameID, currentLocale) : localeTable.Get(npcInfo.NameID, currentLocale);
                        string interactFormat = localeTable.Get(82000003, currentLocale);
                        string[] interactDatas = { npcName, interactFormat };
                        objectiveDescription = string.Format(interactFormat, interactDatas);
                        break;
                    default:
                        objectiveDescription = "";
                        break;
                }
                objectiveText += objectiveDescription + "\n";
            }
        }
        objective.Detail.text = objectiveText.Trim();

        gameObject.SetActive(true);
    }
}
