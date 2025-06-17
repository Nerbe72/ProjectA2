using UnityEngine;

public class QuestDialogueWindow : WindowBase
{
    public int InitializationPriority => 4;
    [HideInInspector] public QuestDialogue QuestDialogue;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        WindowType = WindowType.DialogueWindow;
        QuestDialogue = GetComponentInChildren<QuestDialogue>();

        gameObject.SetActive(false);
    }
}
