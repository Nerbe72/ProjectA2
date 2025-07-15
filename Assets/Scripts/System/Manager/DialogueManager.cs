using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private TableDataManager tableDataManager;

    public int InitializationPriority => 6;

    // UI, 연출, NPC 액션 매니저 등은 실제 구현에 맞게 연결
    private DialogueUI dialogueUI;

    private NPC currentNPC;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        dialogueUI = GetComponentInChildren<DialogueUI>(true);
    }

    private void Start()
    {
        tableDataManager = Singleton.Get<TableDataManager>();
    }

    public void StartDialogue(int _startID, NPC _currentNPC)
    {
        currentNPC = _currentNPC;
        ShowDialogue(_startID);
    }

    private void ShowDialogue(int _id)
    {
        var locale = GameManager.CurrentLocale;
        var main = tableDataManager.Table.Dialogue.Get(_id);

        var table = tableDataManager.Table.DialogueLocale;

        if (main == null || table == null)
        {
            dialogueUI?.Close();
            currentNPC.SetTalking(false);
            return;
        }

        var npc_selected = tableDataManager.Table.NPC.Get(currentNPC.NPCID);
        var npc_name_selected = tableDataManager.Table.Locale.Get(npc_selected.NameID, locale);
        currentNPC.SetTalking(true);
        // 대사 및 화자 출력
        dialogueUI?.Show(npc_name_selected, table.Get(main.TalkID, DialogueType.Talk, locale));

        // 카메라 연출
        if (!string.IsNullOrEmpty(main.CameraAction))
        {
            Singleton.Get<CameraManager>().SetCamera(main.CameraAction, currentNPC.ZoomTarget);
        }
        else
        {
            Singleton.Get<CameraManager>().SetCamera(CameraType.Main);
        }

        // NPC 애니메이션 동작 실행
        if (!string.IsNullOrEmpty(main.NPCAction) && currentNPC != null)
            currentNPC.PlayAnimation(main.NPCAction);

        // 플레이어 동작 실행
        //if (!string.IsNullOrEmpty(main.PlayerAction))
        //{
        //    var player = Singleton.Player;
        //    if (player != null)
        //    {
        //        player.SetPrayAnimation();
        //    }
        //}

        // 창 열기
        if (!string.IsNullOrEmpty(main.OpenWindow))
        {
            var tokens = main.OpenWindow.Split(':');
            string windowType = tokens[0].ToLower();
            Action onWindowClosed = () => ShowDialogue(main.NextID);
            switch (windowType)
            {
                case "level":
                    (Singleton.Get<LevelUpDialogueWindow>() as IWindowStack).ShowWindow();
                    Singleton.Get<LevelUpDialogueWindow>().OnWindowClosed += onWindowClosed;
                    break;
                case "quest":
                    int questId = tokens.Length > 1 ? int.Parse(tokens[1]) : 0;
                    QuestDialogueWindow window = Singleton.Get<QuestDialogueWindow>();
                    (window as IWindowStack).ShowWindow();
                    QuestInstance questInstance = Singleton.Player.QuestStateInstance.GetQuestState(questId);
                    if (!window.QuestDialogue.SetQuest(
                        questId,
                        questInstance == null ? QuestState.Available : questInstance.State))
                    {

                    }
                    window.QuestDialogue.OnQuestResultSelected = (accepted) =>
                    {
                        // Choice 분기 처리
                        var dialogueInfo = tableDataManager.Table.Dialogue.Get(_id);
                        if (dialogueInfo != null && !string.IsNullOrEmpty(dialogueInfo.Choice))
                        {
                            var choiceParts = dialogueInfo.Choice.Split('|');
                            int nextId = 0;
                            if (choiceParts.Length >= 2)
                            {
                                // accepted == true면 첫 번째, false면 두 번째 분기
                                nextId = accepted ? int.Parse(choiceParts[0]) : int.Parse(choiceParts[1]);
                            }
                            if (nextId > 0)
                            {
                                WindowStackManager.PopWindow();
                                ShowDialogue(nextId);
                                return;
                            }
                            else
                                dialogueUI?.Close();
                        }
                        else
                        {
                            // Choice가 없으면 NextID로 진행
                            if (dialogueInfo != null && dialogueInfo.NextID > 0)
                            {
                                WindowStackManager.PopWindow();
                                ShowDialogue(dialogueInfo.NextID);
                                return;
                            }
                            else
                                dialogueUI?.Close();
                        }
                    };
                    break;
            }

            return;
        }

        // 선택지 처리
        List<(string text, int nextId)> choices = new List<(string, int)>();

        if (!string.IsNullOrEmpty(main.Choice) && !string.IsNullOrEmpty(table.Get(main.TalkID, DialogueType.Choice, locale)))
        {
            var choiceParts = main.Choice.Split('|');
            var localeParts = table.Get(main.TalkID, DialogueType.Choice, locale).Split('|');
            int count = choiceParts.Length;

            for (int i = 0; i < count; i++)
            {
                var nextID = int.Parse(choiceParts[i]);
                if (nextID >= 0 && !string.IsNullOrEmpty(localeParts[i]))
                {
                    choices.Add((localeParts[i], nextID));
                }
            }
        }

        if (choices.Count > 0)
        {
            // 선택지 UI 표시 및 선택 시 분기
            dialogueUI.ShowChoice(choices, (selectedIdx) =>
            {
                if (choices[selectedIdx].nextId == 0) dialogueUI?.Close();
                ShowDialogue(choices[selectedIdx].nextId);
            });
            return;
        }

        if (main.NextID > 0)
            dialogueUI?.OnNext(() => ShowDialogue(main.NextID));
        else
            dialogueUI?.OnNext(() => { dialogueUI?.Close(); currentNPC.SetTalking(false); });
    }
}
