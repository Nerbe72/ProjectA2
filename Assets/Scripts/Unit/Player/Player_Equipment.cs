using System;
using UnityEngine;

public partial class Player : Character
{
    public event Action<WeaponItemInstance> OnWeaponChanged;

    //private void SwapWeapon(Weapon _weapon)
    //{
    //    var weaponTable = SingletonManager.TableDataManager.Table.Weapon;
    //    for (int i = 0; i < (int)WeaponType.Count; i++)
    //    {
    //        animator.SetBool(AnimationHash.GetActionHash((WeaponType)i), i == weaponTable.Get(_weapon.WeaponInstance.ID).WeaponType);
    //    }
    //}

    public void EquipWeapon(WeaponItemInstance _instance, bool _broadcast = true)
    {
        EquipWeaponInternal(_instance, _broadcast);
    }

    // Original logic moved here to allow overload wrapper
    private void EquipWeaponInternal(WeaponItemInstance _instance, bool _broadcast)
    {
        if (_instance == null || _instance.InstancedPrefab == null) return;

        // 기존 장착 무기 삭제
        if (weaponPrefab != null)
        {
            // 기존 무기와 형태가 같다면 삭제하지 않음
            if (weaponPrefab.WeaponID == _instance.ItemID) return;
            Destroy(weaponPrefab.gameObject);
            weaponPrefab = null;
            weaponInstance = null;
        }

        // 새 무기 생성 및 장착
        var obj = _instance.InstantiateWeapon();
        if (obj != null)
        {
            Weapon newWeapon = obj.GetComponent<Weapon>();
            if (newWeapon != null)
            {
                // 소유자 설정
                newWeapon.SetOwner(this);
                // Player.weapon 필드에 할당
                weaponPrefab = newWeapon;
                weaponInstance = _instance;
            }

            // 트랜스폼 설정
            obj.transform.SetParent(WeaponHandle, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("무기가 사전로드되지 않음");
        }

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
        WeaponType equippedType = (WeaponType)weapon_selected.WeaponType;

        for (int i = 0; i < (int)WeaponType.Count; i++)
        {
            animator.SetBool(AnimationHash.GetHash((WeaponType)i), i == weapon_selected.WeaponType);
        }
        if (_broadcast && photonView != null && photonView.IsMine)
            ApplyEquipWeapon(_instance.ItemID);
        OnWeaponChanged?.Invoke(_instance);
    }

    // Legacy overload for delegates (broadcast=true by default)
    public void EquipWeapon(WeaponItemInstance _instance)
    {
        EquipWeaponInternal(_instance, true);
    }

    /// <summary>
    /// 피격판정시 호출
    /// </summary>
    public override (AttackType type, int damage) CalculateAttack()
    {
        if (weaponPrefab == null) return (AttackType.Physical, stats.Damage);

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);

        //레벨 성장 데미지 포함
        int damage = stats.Damage +
            weaponInstance.Damage +
            weapon_selected.DamageGrowth_STR * GetCurrentLevel(LevelType.Strength) +
            weapon_selected.DamageGrowth_DEX * GetCurrentLevel(LevelType.Dexterity) +
            weapon_selected.DamageGrowth_INT * GetCurrentLevel(LevelType.Intelligent);

        return ((AttackType)weapon_selected.AttackType, damage);
    }

    public int GetCalculatedDamage(WeaponItemInstance _instance = null)
    {
        if (_instance == null && weaponInstance == null) return CurrentStatus(StatType.Damage);

        TableWeapon.Info weapon_selected;
        WeaponItemInstance weapon_instance;

        if (_instance != null)
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
            weapon_instance = _instance;
        }
        else
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
            weapon_instance = weaponInstance;
        }

        return CurrentStatus(StatType.Damage) +
            weapon_instance.Damage +
            weapon_selected.DamageGrowth_STR * GetCurrentLevel(LevelType.Strength) +
            weapon_selected.DamageGrowth_DEX * GetCurrentLevel(LevelType.Dexterity) +
            weapon_selected.DamageGrowth_INT * GetCurrentLevel(LevelType.Intelligent);
    }

    public int GetCalculatedDamage(Levels _tempLevels)
    {
        if (weaponInstance == null) return TempStatus(StatType.Damage, _tempLevels);

        TableWeapon.Info weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
        return CurrentStatus(StatType.Damage) +
            weaponInstance.Damage +
            weapon_selected.DamageGrowth_STR * (_tempLevels.Data[LevelType.Strength]) +
            weapon_selected.DamageGrowth_DEX * (_tempLevels.Data[LevelType.Dexterity]) +
            weapon_selected.DamageGrowth_INT * (_tempLevels.Data[LevelType.Intelligent]);
    }

    public int GetCalculatedDefense(WeaponItemInstance _instance = null)
    {
        if (_instance == null && weaponInstance == null) return CurrentStatus(StatType.Damage);

        TableWeapon.Info weapon_selected;
        WeaponItemInstance weapon_instance;

        if (_instance != null)
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
            weapon_instance = _instance;
        }
        else
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
            weapon_instance = weaponInstance;
        }

        return CurrentStatus(StatType.Defense) +
            weapon_instance.Defense +
            (GetCurrentLevel(LevelType.Strength) * 1) +
            (GetCurrentLevel(LevelType.Dexterity) * 2);
    }

    public int GetCalculatedDefense(Levels _tempLevels)
    {
        if (weaponInstance == null) return TempStatus(StatType.Damage, _tempLevels);

        return CurrentStatus(StatType.Defense) +
            weaponInstance.Defense +
            (_tempLevels.Data[LevelType.Strength] * 1) +
            (_tempLevels.Data[LevelType.Dexterity] * 2);
    }
}
