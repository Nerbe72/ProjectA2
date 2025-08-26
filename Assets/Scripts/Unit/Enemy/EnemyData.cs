using UnityEngine;

using GameStuff;

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

    [Header("드롭 설정")]
    [Range(0f, 100f)]
    public float BaseDropChance = 100f;
    [Range(1, 5)]
    public int MaxDropsPerKill = 100;
    
    [Header("드롭 아이템")]
    public DropItemData[] DropItems = new DropItemData[0];
}
