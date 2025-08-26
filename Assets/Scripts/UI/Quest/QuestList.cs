using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using GameStuff;

public class QuestList : MonoBehaviour
{
    [SerializeField] private GameObject ContentContainer;
    [SerializeField] private GameObject Prefab;

    private Player player;
    private TableLocale localeTable;

    private Dictionary<int, QuestContent> quests = new Dictionary<int, QuestContent>();
    public event Action<int> OnSelectQuest;

    private void OnEnable()
    {
        if (player == null)
        {
            player = Singleton.Player;

            if (player == null) return;

            player.OnQuestStateChanged += SetList;
        }

        if (localeTable == null)
        {
            localeTable = Singleton.Get<TableDataManager>().Table.Locale;

            if (localeTable == null) Debug.LogError($"Table Error : QuestList");
        }

        SetList();
    }

    private void SetList()
    {
        QuestStateInstances instance = player.QuestStateInstance;
        var states = instance.QuestStates.Values.ToList();
        states.Sort((a, b) => b.QuestID.CompareTo(a.QuestID));
        int count = states.Count;
        for (int i = 0; i < count; i++)
        {
            var id = states[i].QuestID;
            if (quests.ContainsKey(id))
            {
                string header = GetListHeader(states[i].State);
                quests[id].SetHeader(header);
                continue;
            }
            AddQuestList(id);
        }
    }

    public void AddQuestList(int _id)
    {
        if (localeTable == null)
        {
            localeTable = Singleton.Get<TableDataManager>()?.Table.Locale;

            if (localeTable == null)
            {
                Debug.LogError($"Table Error : QuestList");
                return;
            }
        }

        GameObject obj = Instantiate(Prefab);
        obj.transform.parent = ContentContainer.transform;
        QuestContent questContent = obj.GetComponent<QuestContent>();

        var quest_selected = Singleton.Get<QuestManager>().GetQuestInfo(_id);
        var progress_selected = Singleton.Player.QuestStateInstance.GetQuestState(_id);
        var nameLocale = localeTable.Get(quest_selected.NameID, GameManager.CurrentLocale);

        string stateLocale = GetListHeader(progress_selected.State);

        UnityAction onTargetClicked = () => { Singleton.Get<QuestManager>().SetTargetedQuest(_id); };
        UnityAction<bool> onToggleChanged = (selected) => { if (selected) { OnSelectQuest?.Invoke(_id); } };

        if (quests.ContainsKey(_id))
        {
            quests[_id].SetData(stateLocale, nameLocale, progress_selected.State == QuestState.Completed, onTargetClicked, onToggleChanged);
            return;
        }
        quests.Add(_id, questContent);
        questContent.SetData(stateLocale, nameLocale, progress_selected.State == QuestState.Completed, onTargetClicked, onToggleChanged);
    }

    private string GetListHeader(QuestState _state)
    {
        var questStateLocale = localeTable.Get((int)_state, GameManager.CurrentLocale);

        string stateLocale = "";
        switch (_state)
        {
            case QuestState.Available:
                stateLocale = $"<color=#FFF89C>[{questStateLocale}]</color>";
                break;
            case QuestState.Accepted:
                stateLocale = $"<color=#60FF5B>[{questStateLocale}]</color>";
                break;
            case QuestState.Achieved:
                stateLocale = $"<color=#FFAE00>[{questStateLocale}]</color>";
                break;
            case QuestState.Completed:
            default:
                stateLocale = $"<color=#FF3D39>[{questStateLocale}]</color>";
                break;
        }

        return stateLocale;
    }
}
