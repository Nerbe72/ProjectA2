using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameStuff;

[Serializable]
public class QuestObjectiveInstanceData
{
    public int ObjectiveIndex;
    public ObjectiveType Type;
    public int TargetID;
    public bool Completed;
    
    // KillObjectiveInstance용
    public int Current;
    public int Required;
    
    // CollectObjectiveInstance용 (위와 동일한 필드 사용)
}

[Serializable]
public class QuestInstanceData
{
    public int QuestID;
    public QuestState State;
    public List<QuestObjectiveInstanceData> ObjectivesList;
}

[Serializable]
public class PlayerSaveData
{
    // 레벨 데이터
    public int Level_Health;
    public int Level_Strength;
    public int Level_Dexterity;
    public int Level_Intelligent;

    // 기본 스탯
    public int MaxHealth;
    public int Damage;
    public int Defense;

    // 상태 데이터
    public int CurrentHealth;
    public Vector3 Position;
    public Quaternion Rotation;
    public int Scene;

    // 무기 관련 데이터
    public Guid EquippedInventoryID;
    public string EquippedInventoryIDString;

    // 퀘스트 관련 데이터
    public List<QuestInstanceData> QuestData;
    public List<TalkCountData> TalkCountData;
    public List<KillCountData> KillCountData;

    public PlayerSaveData()
    {
        // 레벨 초기화
        Level_Health = 1;
        Level_Strength = 1;
        Level_Dexterity = 1;
        Level_Intelligent = 1;

        // 기본 스탯
        MaxHealth = 100;
        Damage = 100;
        Defense = 100;

        // 상태 초기화
        CurrentHealth = 100;
        Position = new Vector3(4.5f, 0, 5.2f);
        Rotation = Quaternion.Euler(0, 55, 0);
        Scene = 4;

        // 무기 초기화
        EquippedInventoryID = Guid.Empty;

        // 퀘스트 초기화
        QuestData = new List<QuestInstanceData>();
        TalkCountData = new List<TalkCountData>();
        KillCountData = new List<KillCountData>();
    }

    public static PlayerSaveData FromPlayer(Player _player, Vector3 _position)
    {
        var currentWeapon = _player.GetCurrentWeapon();

        return new PlayerSaveData
        {
            // 레벨 데이터
            Level_Health = _player.GetCurrentLevel(LevelType.Health),
            Level_Strength = _player.GetCurrentLevel(LevelType.Strength),
            Level_Dexterity = _player.GetCurrentLevel(LevelType.Dexterity),
            Level_Intelligent = _player.GetCurrentLevel(LevelType.Intelligent),

            MaxHealth = _player.BaseStatus(StatType.Health),
            Damage = _player.BaseStatus(StatType.Damage),
            Defense = _player.BaseStatus(StatType.Defense),

            CurrentHealth = _player.BaseStatus(StatType.Health),

            EquippedInventoryID = (currentWeapon != null && currentWeapon != null)
                ? currentWeapon.InventoryID
                : Guid.Empty,
            EquippedInventoryIDString = (currentWeapon != null && currentWeapon != null)
                ? currentWeapon.InventoryID.ToString()
                : Guid.Empty.ToString(),

            Position = _position,
            Rotation = _player.transform.rotation,

            Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex,

            // 퀘스트 데이터 저장
            QuestData = _player.QuestStateInstance.QuestStates.Values.Select(qsi => new QuestInstanceData
            {
                QuestID = qsi.QuestID,
                State = qsi.State,
                ObjectivesList = qsi.Objectives.Values.Select(obj => new QuestObjectiveInstanceData
                {
                    ObjectiveIndex = obj.ObjectiveIndex,
                    Type = obj.Type,
                    TargetID = obj.TargetID,
                    Completed = obj.Completed,
                    Current = obj is KillObjectiveInstance kill ? kill.Current : 0,
                    Required = obj is KillObjectiveInstance killObj ? killObj.Required : 
                              obj is CollectObjectiveInstance collect ? collect.Required : 0
                }).ToList()
            }).ToList(),
            TalkCountData = _player.TalkCount.TalkCount.Select(kvp => new TalkCountData { NPCID = kvp.Key, Count = kvp.Value }).ToList(),
            KillCountData = _player.KillCount.KillCount.Select(kvp => new KillCountData { EnemyID = kvp.Key, Count = kvp.Value }).ToList()
        };
    }
}

[Serializable]
public class TalkCountData
{
    public int NPCID;
    public int Count;
}

[Serializable]
public class KillCountData
{
    public int EnemyID;
    public int Count;
}

