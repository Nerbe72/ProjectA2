using System;
using System.Collections.Generic;
using UnityEngine;

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
    public string SceneName;

    // 무기 관련 데이터
    public Guid EquippedInventoryID;
    public string EquippedInventoryIDString;
    
    public PlayerSaveData()
    {
        // 레벨 초기화
        Level_Health = 0;
        Level_Strength = 0;
        Level_Dexterity = 0;
        Level_Intelligent = 0;
        
        // 기본 스탯
        MaxHealth = 100;
        Damage = 100;
        Defense = 100;
        
        // 상태 초기화
        CurrentHealth = 100;
        Position = new Vector3(4.5f, 0, 5.2f);
        Rotation = Quaternion.Euler(0, 55, 0);
        SceneName = "Village";

        // 무기 초기화
        EquippedInventoryID = Guid.Empty;
    }
    
    public static PlayerSaveData FromPlayer(Player player)
    {
        var currentWeapon = player.GetCurrentWeapon();

        return new PlayerSaveData
        {
            // 레벨 데이터
            Level_Health = player.GetCurrentLevel(LevelType.Health),
            Level_Strength = player.GetCurrentLevel(LevelType.Strength),
            Level_Dexterity = player.GetCurrentLevel(LevelType.Dexterity),
            Level_Intelligent = player.GetCurrentLevel(LevelType.Intelligent),

            MaxHealth = player.BaseStatus(StatType.Health),
            Damage = player.BaseStatus(StatType.Damage),
            Defense = player.BaseStatus(StatType.Defense),

            CurrentHealth = player.BaseStatus(StatType.Health),
            //Position = player.transform.position,
            //Rotation = player.transform.rotation,
            //SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,

            EquippedInventoryID = (currentWeapon != null && currentWeapon != null)
                ? currentWeapon.InventoryID
                : Guid.Empty,
            EquippedInventoryIDString = (currentWeapon != null && currentWeapon != null)
                ? currentWeapon.InventoryID.ToString()
                : Guid.Empty.ToString(),
        };
    }
} 