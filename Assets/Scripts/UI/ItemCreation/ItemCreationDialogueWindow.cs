using System;
using UnityEngine;

public class ItemCreationDialogueWindow : WindowBase
{
    private RecipeList list;
    private Creation creation;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        gameObject.SetActive(false);

        list = GetComponentInChildren<RecipeList>(true);
        creation = GetComponentInChildren<Creation>(true);

        list.OnRecipeSelected += DetectItemSelection;
    }

    private void DetectItemSelection(ItemInstance _instance)
    {
        creation.SetData(_instance.ItemID);
    }
}
