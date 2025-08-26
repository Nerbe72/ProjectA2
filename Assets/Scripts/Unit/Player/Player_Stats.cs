using System;
using System.Collections.Generic;
using System.Linq;

using GameStuff;
using UnityEngine;

public partial class Player : Character
{
    public int CurrentMaxHp => (stats.Health + (CurrentLevels.Data[LevelType.Health] * 4) + (CurrentLevels.Data[LevelType.Strength] * 2) + (CurrentLevels.Data[LevelType.Dexterity] * 2) + (CurrentLevels.Data[LevelType.Intelligent]));
    public int CurrentDamage => (stats.Damage + CurrentLevels.Data[LevelType.Strength] * 3);
    public int CurrentDefense => (stats.Defense + CurrentLevels.Data[LevelType.Dexterity] * 1 + CurrentLevels.Data[LevelType.Strength] * 2);

    public Levels CurrentLevels { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnLevelChanged;

    public int GetCurrentLevel(LevelType _type)
    {
        if (_type == LevelType.Total) return CurrentLevels.GetTotal();

        return CurrentLevels.Data[_type];
    }

    public int CurrentStatus(StatType _type)
    {
        switch (_type)
        {
            case StatType.Health:
                return CurrentMaxHp;
            case StatType.Damage:
                return CurrentDamage;
            case StatType.Defense:
                return CurrentDefense;
            default:
                return 0;
        }
    }

    public int TempStatus(StatType _type, Levels _tempLevels)
    {
        switch (_type)
        {
            case StatType.Health:
                return (stats.Health + (_tempLevels.Data[LevelType.Health] * 4) + (_tempLevels.Data[LevelType.Strength] * 2) + (_tempLevels.Data[LevelType.Dexterity] * 2) + (_tempLevels.Data[LevelType.Intelligent]));
            case StatType.Damage:
                return (stats.Damage + _tempLevels.Data[LevelType.Strength] * 3);
            case StatType.Defense:
                return (stats.Defense + _tempLevels.Data[LevelType.Dexterity] * 1 + _tempLevels.Data[LevelType.Strength] * 2);
            default:
                return 0;
        }
    }

    public int BaseStatus(StatType _type)
    {
        switch (_type)
        {
            case StatType.Health:
                return stats.Health;
            case StatType.Damage:
                return stats.Damage;
            case StatType.Defense:
                return stats.Defense;
            default:
                return 0;
        }
    }

    public void SetBaseStat(StatType _type, int _value)
    {
        if (stats == null)
        {
            stats = new StatData();
        }

        switch (_type)
        {
            case StatType.Health:
                stats.Health = _value;
                break;
            case StatType.Damage:
                stats.Damage = _value;
                break;
            case StatType.Defense:
                stats.Defense = _value;
                break;
        }
    }

    public void LoadLevelFromDB(int _healthLevel, int _strengthLevel, int _dexterityLevel, int _intelligentLevel)
    {
        CurrentLevels = new Levels(_healthLevel, _strengthLevel, _dexterityLevel, _intelligentLevel);
    }

    public void LevelUp(LevelType _type, int _increaseAmount)
    {
        // 총합 상한 300 유지: 남은 포인트만큼만 증가
        int remain = 300 - CurrentLevels.GetTotal();
        int finalIncrease = Mathf.Min(_increaseAmount, Mathf.Max(remain, 0));

        if (finalIncrease <= 0)
            return;

        CurrentLevels.Data[_type] += finalIncrease;

        OnLevelChanged?.Invoke();
    }

    public void LevelUp(Levels _tempLevels)
    {
        CurrentLevels.Data[LevelType.Health] = _tempLevels.Data[LevelType.Health];
        CurrentLevels.Data[LevelType.Strength] = _tempLevels.Data[LevelType.Strength];
        CurrentLevels.Data[LevelType.Dexterity] = _tempLevels.Data[LevelType.Dexterity];
        CurrentLevels.Data[LevelType.Intelligent] = _tempLevels.Data[LevelType.Intelligent];
        OnLevelChanged?.Invoke();

        SavePlayerDataWithoutPosition();
    }
}

public class Levels
{
    public Dictionary<LevelType, int> Data;

    public Levels()
    {
        Data = new Dictionary<LevelType, int>();

        for (int i = 1; i < (int)LevelType.Count; i++)
        {
            Data.Add((LevelType)i, 0);
        }
    }

    public Levels(int health, int strength, int dexterity, int intelligent)
    {
        Data = new Dictionary<LevelType, int>();
        List<int> temp = new List<int> { health, strength, dexterity, intelligent };

        for (int i = 1; i < (int)LevelType.Count; i++)
        {
            Data.Add((LevelType)i, temp[i - 1]);
        }
    }

    public int GetTotal()
    {
        return Data.Values.Sum();
    }
}
