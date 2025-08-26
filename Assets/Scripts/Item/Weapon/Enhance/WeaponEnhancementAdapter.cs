using UnityEngine;
using System.Collections.Generic;
using System;

using GameStuff;

public enum EnhancementResult
{
    Success,
    Failure,
    MaxLevel,
}

public enum EnchantResult
{
    Success,
    AlreadyEquipped,
    NoEnoughSlot,
    SkillNotFound,
}

[Serializable]
public class WeaponEnhancementAdapter : IWeaponEnhancement
{
    public string weaponInventoryIDString;
    public int enhancedLevel;
    public List<int> enchantedSkillIds = new List<int>();
    public Guid weaponInventoryID;

    public int EnhancedLevel
    {
        get => enhancedLevel;
    }

    public WeaponItemInstance WeaponItemInstance
    {
        get
        {
            if (weaponInventoryID == Guid.Empty) return null;
            return Singleton.Inventory.GetWeaponByInventoryID(weaponInventoryID);
        }
    }

    public void Init(WeaponItemInstance _instance)
    {
        if (_instance != null)
        {
            weaponInventoryID = _instance.InventoryID;
            weaponInventoryIDString = _instance.InventoryIDString;
        }
    }

    public int MaxEnhancementLevel
    {
        get
        {
            var weaponInstance = WeaponItemInstance;
            if (weaponInstance == null)
            {
                Debug.LogError("WeaponItemInstance is not initialized.");
                return 0;
            }

            var table = Singleton.Get<TableDataManager>().Table;

            return table.Weapon.Get(weaponInstance.ItemID).MaxEnchantmentCount;
        }
    }

    public bool IsMaxEnhanced => enhancedLevel == MaxEnhancementLevel;

    public EnhancementResult TryEnhance()
    {
        if (IsMaxEnhanced)
            return EnhancementResult.MaxLevel;

        // 강화 성공률 계산
        float _successRate = CalculateSuccessRate();
        bool _success = UnityEngine.Random.Range(0f, 100f) < _successRate;

        if (_success)
        {
            // 성공 시에만 재화 차감
            uint _enhancementCost = (uint)(enhancedLevel * 120);
            
            if (Singleton.Inventory.IsCurrencyEnough(_enhancementCost))
            {
                Singleton.Inventory.MinusCurrency(_enhancementCost);
                enhancedLevel++;
                Singleton.Get<Alert>().Show($"강화 성공! 새로운 레벨: {enhancedLevel}", Color.green);
                return EnhancementResult.Success;
            }
            else
            {
                Singleton.Get<Alert>().Show("강화 성공 후 재화 차감에 실패했습니다.", Color.red);
                return EnhancementResult.Failure;
            }
        }

        Singleton.Get<Alert>().Show($"강화 실패! 성공률: {_successRate}%", Color.red);
        return EnhancementResult.Failure;
    }

    private float CalculateSuccessRate()
    {
        // 강화 레벨별 성공률
        if (enhancedLevel <= 5) return 100f;      // 0~5레벨: 100%
        if (enhancedLevel <= 10) return 80f;      // 6~10레벨: 80%
        if (enhancedLevel <= 15) return 60f;      // 11~15레벨: 60%
        if (enhancedLevel <= 20) return 40f;      // 16~20레벨: 40%
        return 20f;                               // 21레벨 이상: 20%
    }

    public void EnhanceWeapon() => TryEnhance();

    public int GetTotalDamage()
    {
        var weaponInstance = WeaponItemInstance;
        if (weaponInstance == null) return 0;
        
        return weaponInstance.Damage + (enhancedLevel * 5);
    }

    public int GetTotalDefense()
    {
        var weaponInstance = WeaponItemInstance;
        if (weaponInstance == null) return 0;
        
        return weaponInstance.Defense + (enhancedLevel * 2);
    }

    //private float CalculateSuccessRate()
    //{
    //}
    
    public IReadOnlyList<int> GetEnchantedSkills()
    {
        return enchantedSkillIds.AsReadOnly();
    }
    
    public List<int> GetEnchantedSkillsCopy()
    {
        return new List<int>(enchantedSkillIds);
    }
    
    public int GetTotalSkillSlots()
    {
        var weaponInstance = WeaponItemInstance;
        if (weaponInstance == null) return 0;
        
        var weaponInfo = Singleton.Get<TableDataManager>()?.Table.Weapon.Get(weaponInstance.ItemID);
        return weaponInfo?.SkillSlotCount ?? 0;
    }
    
    public int GetUsedSkillSlots()
    {
        int totalUsed = 0;
        var skillTable = Singleton.Get<TableDataManager>()?.Table.Skill;
        
        if (skillTable != null)
        {
            foreach (int skillId in enchantedSkillIds)
            {
                var skillInfo = skillTable.Get(skillId);
                if (skillInfo != null)
                {
                    totalUsed += skillInfo.RequiredSlotCount;
                }
            }
        }
        return totalUsed;
    }
    
    public int GetAvailableSkillSlots()
    {
        return GetTotalSkillSlots() - GetUsedSkillSlots();
    }
    
    public EnchantResult TryEnchantSkill(int _skillId)
    {
        if (enchantedSkillIds.Contains(_skillId))
        {
            Singleton.Get<Alert>().Show("이미 장착된 스킬입니다.", Color.red);
            return EnchantResult.AlreadyEquipped;
        }
        
        var skillInfo = Singleton.Get<TableDataManager>()?.Table.Skill.Get(_skillId);
        var inventory = Singleton.Inventory;
        
        int requiredSlots = skillInfo.RequiredSlotCount;
        if (GetAvailableSkillSlots() < requiredSlots)
        {
            Singleton.Get<Alert>().Show($"스킬 슬롯이 부족합니다. 필요: {requiredSlots}, 사용 가능: {GetAvailableSkillSlots()}", Color.red);
            return EnchantResult.NoEnoughSlot;
        }
        
        int removedCount = inventory.RemoveItemByID(_skillId, 1);
        if (removedCount == 0)
        {
            Singleton.Get<Alert>().Show($"보유하지 않은 스킬(ID: {_skillId})입니다", Color.red);
            return EnchantResult.SkillNotFound;
        }
        
        enchantedSkillIds.Add(_skillId);
        Singleton.Get<Alert>().Show($"스킬 인챈트 성공! ID: {_skillId}, 슬롯 사용: {requiredSlots}", Color.green);
        
        return EnchantResult.Success;
    }
    
    public List<AbilityLogic> GetActiveSkills()
    {
        List<AbilityLogic> skills = new List<AbilityLogic>();
        var abilityManager = Singleton.Get<AbilityManager>();
        
        if (abilityManager != null)
        {
            foreach (int skillId in enchantedSkillIds)
            {
                var skill = abilityManager.GetAbility(skillId);
                if (skill != null)
                    skills.Add(skill);
            }
        }
        return skills;
    }
    
    public bool HasEnchantedSkill(int _skillId)
    {
        return enchantedSkillIds.Contains(_skillId);
    }
    
    public bool RemoveEnchantedSkill(int _skillId)
    {
        bool removed = enchantedSkillIds.Remove(_skillId);
        if (removed)
        {
            Singleton.Get<Alert>().Show($"스킬 인챈트 제거 완료! ID: {_skillId}", Color.yellow);
        }
        return removed;
    }
    
    public (int total, int used, int available) GetSlotInfo()
    {
        int total = GetTotalSkillSlots();
        int used = GetUsedSkillSlots();
        int available = total - used;
        
        return (total, used, available);
    }

    public float GetGrowth(LevelType _levelType)
    {
        var weaponInstance = WeaponItemInstance;
        if (weaponInstance == null) return 0f;

        var weaponData = Singleton.Get<TableDataManager>()?.Table.Weapon.Get(weaponInstance.ItemID);
        if (weaponData == null) return 0f;

        var enhancementTable = Singleton.Get<TableDataManager>()?.Table.Enhancement;
        var enhancementInfo = enhancementTable?.Get(weaponInstance.ItemID, enhancedLevel);

        switch (_levelType)
        {
            case LevelType.Strength:
                return weaponData.DamageGrowth_STR + (enhancementInfo?.AddGrowthSTR ?? 0f);
            case LevelType.Dexterity:
                return weaponData.DamageGrowth_DEX + (enhancementInfo?.AddGrowthDEX ?? 0f);
            case LevelType.Intelligent:
                return weaponData.DamageGrowth_INT + (enhancementInfo?.AddGrowthINT ?? 0f);
            default:
                return 0f;
        }
    }
}