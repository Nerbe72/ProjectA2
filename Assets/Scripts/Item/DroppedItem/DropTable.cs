using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItemData
{
    public int ItemID;
    [Range(0f, 100f)]
    public float DropRate; // 0-100% 확률
    [Range(1, 10)]
    public int MinQuantity = 1;
    [Range(1, 10)]
    public int MaxQuantity = 1;
}

[CreateAssetMenu(fileName = "DropTable", menuName = "Game/Drop Table")]
public class DropTable : ScriptableObject
{
    [Header("드롭 테이블 설정")]
    public int EnemyID;
    public string EnemyName;
    
    [Header("드롭 아이템 목록")]
    public List<DropItemData> DropItems = new List<DropItemData>();
    
    [Header("드롭 설정")]
    [Range(0f, 100f)]
    public float BaseDropChance = 50f; // 기본 드롭 확률
    [Range(1, 5)]
    public int MaxDropsPerKill = 3; // 한 번에 최대 드롭 개수
} 