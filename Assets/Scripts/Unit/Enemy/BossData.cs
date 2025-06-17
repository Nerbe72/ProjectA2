using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BossStatData", order = 1)]
public class BossData : StatData
{
    public AttackType AttackType;
    public int Speed;
    public int RewardCurrency;
}
