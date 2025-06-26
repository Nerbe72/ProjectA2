using System;
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
    public int Scene;

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
        Scene = 4;

        // 무기 초기화
        EquippedInventoryID = Guid.Empty;
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
            //Position = player.transform.position,
            //Rotation = player.transform.rotation,
            //SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,

            EquippedInventoryID = (currentWeapon != null && currentWeapon != null)
                ? currentWeapon.InventoryID
                : Guid.Empty,
            EquippedInventoryIDString = (currentWeapon != null && currentWeapon != null)
                ? currentWeapon.InventoryID.ToString()
                : Guid.Empty.ToString(),

            Position = _position,
            Rotation = _player.transform.rotation,

            Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        };
    }
}