using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryWrapper
{
    public List<WeaponItemInstance> weapons;
    public List<PotionItemInstance> potions;
    public uint Currency;

    public InventoryWrapper()
    {
        weapons = new List<WeaponItemInstance>();
        potions = new List<PotionItemInstance>();
        Currency = 0;
    }
}
