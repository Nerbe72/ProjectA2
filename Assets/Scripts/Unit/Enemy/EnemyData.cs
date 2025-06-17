using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EnemyStatData", order = 1)]
public class EnemyData : StatData
{
    public AttackType AttackType;

    public float Speed;

    public int SightOffset;
    public int SightAngle;
    public float SightDistance;
    public Vector3 SightHeight;
    public float DistanceLimit;

    public float AttackDistance;
    public float AttackCooldown;

    public int WeaponID;

    public int RewardCurrency;
}
