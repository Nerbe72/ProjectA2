using System;
using UnityEngine;

public class QuestUI : WindowBase
{
    public int InitializationPriority => 6;

    private QuestList questList;
    private QuestDetail questDetail;

    public event Action<int> OnToggleChanged;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        questList = GetComponentInChildren<QuestList>(true);
        questDetail = GetComponentInChildren<QuestDetail>(true);
        questList.OnSelectQuest += questDetail.SetData;

        questList.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
