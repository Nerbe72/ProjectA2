using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public int ID;
    public string Name;
    public Sprite Icon;
    public ItemType Type;
    public Rare Rarity;
}

[Serializable]
public class ItemWrapper
{
    public List<ItemData> items;

    public ItemWrapper()
    {
        items = new List<ItemData>();
    }
}