using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDialogue : MonoBehaviour
{
    private QuestManager questManager;

    [SerializeField] private TMP_Text questName;
    [SerializeField] private QuestColumn ObjectiveColumn;
    [SerializeField] private QuestColumn RewardColumn;
    [SerializeField] private QuestColumn DescriptionColumn;
    [SerializeField] private Button AcceptButton;
    [SerializeField] private Button DeclineButton;

    public System.Action<bool> OnQuestResultSelected; // true: 수락, false: 거절

    private void Awake()
    {
        Singleton.Get<GameManager>().OnLocaleChanged += SetLocale;
    }

    private void OnEnable()
    {
        questManager = Singleton.Get<QuestManager>();
        SetLocale();
    }

    private void OnDestroy()
    {
        if (Singleton.Get<GameManager>() == null) return;
        Singleton.Get<GameManager>().OnLocaleChanged -= SetLocale;
    }

    private void SetLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        ObjectiveColumn.Title.text = table.Get(10000031, locale);
        RewardColumn.Title.text = table.Get(10000032, locale);
        DescriptionColumn.Title.text = table.Get(10000033, locale);

        AcceptButton.GetComponentInChildren<TMP_Text>().text = table.Get(10000034, locale);
        DeclineButton.GetComponentInChildren<TMP_Text>().text = table.Get(10000035, locale);
    }

    public bool SetQuest(int _id, QuestState _state)
    {
        if (_id == 0) return false;

        QuestInfo quest_selected = questManager.GetQuestInfo(_id);
        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;

        questName.text = localeTable.Get(quest_selected.NameID, GameManager.CurrentLocale);
        DescriptionColumn.Detail.text = localeTable.Get(quest_selected.DescriptionID, GameManager.CurrentLocale);

        // 퀘스트 보상
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
                        string itemName = tableMgr.Locale.Get(itemInfo.Name, GameManager.CurrentLocale);
                        rewardText += $"{itemName}\n";
                    }
                }
            }
        }
        RewardColumn.Detail.text = rewardText.Trim();

        // 퀘스트 완료 조건
        string objectiveText = string.Empty;
        int count = quest_selected.Objectives.Count;
        if (quest_selected.Objectives != null && count > 0)
        {
            var tableMgr = Singleton.Get<TableDataManager>().Table;
            for (int i = 0; i < count; i++)
            {
                objectiveText += quest_selected.Objectives[i].GetLocalizedDescription() + "\n";
            }
        }
        ObjectiveColumn.Detail.text = objectiveText.Trim();

        //버튼 설정
        AcceptButton.onClick.RemoveAllListeners();
        DeclineButton.onClick.RemoveAllListeners();
        switch (_state)
        {
            case QuestState.Available:
                {
                    AcceptButton.gameObject.SetActive(true);
                    DeclineButton.gameObject.SetActive(true);
                    AcceptButton.GetComponentInChildren<TMP_Text>().text = localeTable.Get(10000034, GameManager.CurrentLocale);
                    DeclineButton.GetComponentInChildren<TMP_Text>().text = localeTable.Get(10000035, GameManager.CurrentLocale);
                    AcceptButton.onClick.AddListener(() =>
                    {
                        questManager.AddQuest(_id);
                        OnQuestResultSelected?.Invoke(true);
                    });
                    DeclineButton.onClick.AddListener(() =>
                    {
                        questManager.DeclineQuest(_id);
                        OnQuestResultSelected?.Invoke(false);
                    });
                    return true;
                }
            case QuestState.Accepted:
                {
                    AcceptButton.gameObject.SetActive(false);
                    DeclineButton.gameObject.SetActive(true);
                    DeclineButton.GetComponentInChildren<TMP_Text>().text = localeTable.Get(10000036, GameManager.CurrentLocale);
                    DeclineButton.onClick.AddListener(() =>
                    {
                        OnQuestResultSelected?.Invoke(true);
                    });
                    return true;
                }
            default:
                OnQuestResultSelected?.Invoke(false);
                return false;
        }
    }
}
