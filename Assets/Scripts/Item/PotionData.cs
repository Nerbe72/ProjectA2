using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/PotionData")]
public class PotionData : ItemData
{
    public int ItemAmount;
    public int HealAmount;
    public int ManaAmount;
    public string Description;
}

