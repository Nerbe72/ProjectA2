using TMPro;
using UnityEngine;

using GameStuff;

public class QuestIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text QuestName;
    [SerializeField] private TMP_Text QuestProgress;

    private Animator animator;
    private int showHash;
    private int completeHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        showHash = AnimationHash.GetHash("Show");
        completeHash = AnimationHash.GetHash("Complete");
    }

    private void Start()
    {
        Singleton.Get<QuestManager>().OnTargetQuestChanged += SetQuestProgress;
        animator.SetBool(showHash, false);
        animator.SetBool(completeHash, false);

        Singleton.Player.OnQuestStateChanged += InvokeQuestState;
    }

    private void InvokeQuestState()
    {
        SetQuestProgress(Singleton.Get<QuestManager>().GetTargetedQuest());
    }

    public void SetQuestProgress(int _questID)
    {
        if (_questID <= 0)
        {
            animator.SetBool(showHash, false);
            animator.SetBool(completeHash, false);
            Singleton.Get<QuestPathManager>().ClearPath();
            return;
        }

        var quest_selected = Singleton.Get<QuestManager>().GetQuestInfo(_questID);
        var quest_progress = Singleton.Player.QuestStateInstance.GetQuestState(_questID);
        var table = Singleton.Get<TableDataManager>().Table;
        var locale = table.Locale;

        string name = locale.Get(quest_selected.NameID, GameManager.CurrentLocale);

        QuestName.text = name;

        int count = quest_progress.Objectives.Count;
        for (int i = 0; i < count; i++)
        {
            ObjectiveInfo objective = quest_selected.Objectives[i];

            switch (objective.ObjectiveType)
            {
                case ObjectiveType.Interact:
                    {
                        var instance = quest_progress.GetObjectiveInstance<QuestObjectiveInstance>(objective.ObjectiveIndex);
                        var npc_selected = table.NPC.Get(objective.TargetID);
                        string interactFormat = locale.Get(82000003, GameManager.CurrentLocale);
                        string targetName = locale.Get(npc_selected.NameID, GameManager.CurrentLocale);
                        QuestProgress.text = string.Format(interactFormat, targetName);
                        break;
                    }
                case ObjectiveType.Kill:
                    {
                        var instance = quest_progress.GetObjectiveInstance<KillObjectiveInstance>(objective.ObjectiveIndex);
                        string targetName = locale.Get(objective.TargetID, GameManager.CurrentLocale);
                        int progressCount = instance.Current;
                        int requireCount = instance.Required;

                        QuestProgress.text = $"{targetName}| {progressCount} / {requireCount}";
                        break;
                    }
                case ObjectiveType.Collect:
                    {
                        var instance = quest_progress.GetObjectiveInstance<CollectObjectiveInstance>(objective.ObjectiveIndex);
                        string targetName = locale.Get(objective.TargetID, GameManager.CurrentLocale);
                        int progressCount = instance.Current;
                        int requireCount = instance.Required;

                        QuestProgress.text = $"{targetName}| {progressCount} / {requireCount}";
                        break;
                    }
                default:
                    break;
            }
        }

        animator.SetBool(showHash, true);
        animator.SetBool(completeHash, quest_progress.State == QuestState.Achieved);

        // path
        if (quest_progress.State == QuestState.Accepted)
            Singleton.Get<QuestPathManager>().DrawPath(_questID);
        else
            Singleton.Get<QuestPathManager>().ClearPath();
    }
}
