using System;
using UnityEngine;

using GameStuff;

public partial class Player : Character
{
    public event Action<WeaponItemInstance> OnWeaponChanged;

    [SerializeField] private AudioClip swapsound;

    public void EquipWeapon(WeaponItemInstance _instance, bool _broadcast = true)
    {
        EquipWeaponInternal(_instance, _broadcast);
    }

    private void EquipWeaponInternal(WeaponItemInstance _instance, bool _broadcast)
    {
        if (_instance == null || _instance.InstancedPrefab == null) return;

        if (weaponPrefab != null)
        {
            if (weaponPrefab.WeaponID == _instance.ItemID)
            {
                goto ChangeEquip;
            }

            Destroy(weaponPrefab.gameObject);
            weaponPrefab = null;
            weaponInstance = null;
            weaponInstanceId = Guid.Empty;
        }

        var obj = _instance.InstantiateWeapon();
        if (obj != null)
        {
            Weapon newWeapon = obj.GetComponent<Weapon>();
            if (newWeapon != null)
            {
                newWeapon.SetOwner(this);
                weaponPrefab = newWeapon;
                weaponInstance = _instance;
                weaponInstanceId = _instance.InventoryID;
            }

            obj.transform.SetParent(WeaponHandle, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            if (photonView != null && !photonView.IsMine)
            {
                foreach (var col in obj.GetComponentsInChildren<Collider>(true))
                {
                    Destroy(col);
                }
            }
        }
        else
        {
            Debug.LogError("Weapon Prefab Not Loaded : Player_Equipment - EquipmentWeaponInternal");
        }

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
        WeaponType equippedType = (WeaponType)weapon_selected.WeaponType;

        for (int i = 0; i < (int)WeaponType.Count; i++)
        {
            animator.SetBool(AnimationHash.GetHash((WeaponType)i), i == weapon_selected.WeaponType);
        }

    ChangeEquip:

        if (_broadcast && photonView != null && photonView.IsMine)
            ApplyEquipWeapon(_instance.ItemID);

        // Change Color
        var adapter = Singleton.Inventory.GetWeaponAdapter(_instance);
        if (adapter != null)
            weaponPrefab.SetOutlineColor(adapter.enhancedLevel);

        OnWeaponChanged?.Invoke(_instance);

        PlayEquipSound();
    }

    private void PlayEquipSound()
    {
        if (swapsound != null)
        {
            var soundManager = Singleton.Get<SoundManager>();
            if (soundManager != null)
            {
                soundManager.PlayEffectOneShot(swapsound);
            }
        }
    }

    public void EquipWeapon(WeaponItemInstance _instance)
    {
        EquipWeaponInternal(_instance, true);
    }

    public override (AttackType type, int damage) CalculateAttack()
    {
        if (weaponPrefab == null) return (AttackType.Physical, stats.Damage);

        var currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null) return (AttackType.Physical, stats.Damage);

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(currentWeapon.ItemID);

        int damage = stats.Damage +
            currentWeapon.Damage +
            weapon_selected.DamageGrowth_STR * GetCurrentLevel(LevelType.Strength) +
            weapon_selected.DamageGrowth_DEX * GetCurrentLevel(LevelType.Dexterity) +
            weapon_selected.DamageGrowth_INT * GetCurrentLevel(LevelType.Intelligent);

        return ((AttackType)weapon_selected.AttackType, damage);
    }

    public override int GetCalculatedDamage(WeaponItemInstance _instance = null)
    {
        var currentWeapon = GetCurrentWeapon();
        if (_instance == null && currentWeapon == null) return CurrentStatus(StatType.Damage);

        TableWeapon.Info weapon_selected;
        WeaponItemInstance weapon_instance;

        if (_instance != null)
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
            weapon_instance = _instance;
        }
        else
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(currentWeapon.ItemID);
            weapon_instance = currentWeapon;
        }

        var adapter = Singleton.Inventory.GetWeaponAdapter(weapon_instance);

        return CurrentStatus(StatType.Damage) +
            weapon_instance.Damage +
            (int)(adapter?.GetGrowth(LevelType.Strength) ?? weapon_selected.DamageGrowth_STR) * GetCurrentLevel(LevelType.Strength) +
            (int)(adapter?.GetGrowth(LevelType.Dexterity) ?? weapon_selected.DamageGrowth_DEX) * GetCurrentLevel(LevelType.Dexterity) +
            (int)(adapter?.GetGrowth(LevelType.Intelligent) ?? weapon_selected.DamageGrowth_INT) * GetCurrentLevel(LevelType.Intelligent);
    }

    public int GetCalculatedDamage(Levels _tempLevels)
    {
        var currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null) return TempStatus(StatType.Damage, _tempLevels);

        TableWeapon.Info weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(currentWeapon.ItemID);
        var adapter = Singleton.Inventory.GetWeaponAdapter(currentWeapon);

        return CurrentStatus(StatType.Damage) +
            currentWeapon.Damage +
            (int)(adapter?.GetGrowth(LevelType.Strength) ?? weapon_selected.DamageGrowth_STR) * (_tempLevels.Data[LevelType.Strength]) +
            (int)(adapter?.GetGrowth(LevelType.Dexterity) ?? weapon_selected.DamageGrowth_DEX) * (_tempLevels.Data[LevelType.Dexterity]) +
            (int)(adapter?.GetGrowth(LevelType.Intelligent) ?? weapon_selected.DamageGrowth_INT) * (_tempLevels.Data[LevelType.Intelligent]);
    }

    public override int GetCalculatedDefense(WeaponItemInstance _instance = null)
    {
        var currentWeapon = GetCurrentWeapon();
        if (_instance == null && currentWeapon == null) return CurrentStatus(StatType.Damage);

        TableWeapon.Info weapon_selected;
        WeaponItemInstance weapon_instance;

        if (_instance != null)
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
            weapon_instance = _instance;
        }
        else
        {
            weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(currentWeapon.ItemID);
            weapon_instance = currentWeapon;
        }

        var adapter = Singleton.Inventory.GetWeaponAdapter(weapon_instance);

        return CurrentStatus(StatType.Defense) +
            weapon_instance.Defense +
            (int)((adapter?.GetGrowth(LevelType.Strength) ?? 1f) * GetCurrentLevel(LevelType.Strength)) +
            (int)((adapter?.GetGrowth(LevelType.Dexterity) ?? 2f) * GetCurrentLevel(LevelType.Dexterity));
    }

    public int GetCalculatedDefense(Levels _tempLevels)
    {
        var currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null) return TempStatus(StatType.Damage, _tempLevels);

        var adapter = Singleton.Inventory.GetWeaponAdapter(currentWeapon);

        return CurrentStatus(StatType.Defense) +
            currentWeapon.Defense +
            (int)((adapter?.GetGrowth(LevelType.Strength) ?? 1f) * (_tempLevels.Data[LevelType.Strength])) +
            (int)((adapter?.GetGrowth(LevelType.Dexterity) ?? 2f) * (_tempLevels.Data[LevelType.Dexterity]));
    }

    public int GetCalculatedDamageWithGrowth(WeaponItemInstance _weapon, int _enhancementLevel)
    {
        if (_weapon == null) return CurrentStatus(StatType.Damage);

        var weaponData = Singleton.Get<TableDataManager>().Table.Weapon.Get(_weapon.ItemID);
        if (weaponData == null) return CurrentStatus(StatType.Damage);

        var enhancementTable = Singleton.Get<TableDataManager>().Table.Enhancement;
        var enhancementInfo = enhancementTable.Get(_weapon.ItemID, _enhancementLevel);

        float strGrowth = weaponData.DamageGrowth_STR;
        float dexGrowth = weaponData.DamageGrowth_DEX;
        float intGrowth = weaponData.DamageGrowth_INT;

        if (enhancementInfo != null)
        {
            strGrowth += enhancementInfo.AddGrowthSTR;
            dexGrowth += enhancementInfo.AddGrowthDEX;
            intGrowth += enhancementInfo.AddGrowthINT;
        }

        return CurrentStatus(StatType.Damage) +
            _weapon.Damage +
            (int)(strGrowth * GetCurrentLevel(LevelType.Strength)) +
            (int)(dexGrowth * GetCurrentLevel(LevelType.Dexterity)) +
            (int)(intGrowth * GetCurrentLevel(LevelType.Intelligent));
    }

    public int GetCalculatedDefenseWithGrowth(WeaponItemInstance _weapon, int _enhancementLevel)
    {
        if (_weapon == null) return CurrentStatus(StatType.Defense);

        var enhancementTable = Singleton.Get<TableDataManager>().Table.Enhancement;
        var enhancementInfo = enhancementTable.Get(_weapon.ItemID, _enhancementLevel);

        float strGrowth = 1f;
        float dexGrowth = 2f;

        if (enhancementInfo != null)
        {
            strGrowth += enhancementInfo.AddGrowthSTR;
            dexGrowth += enhancementInfo.AddGrowthDEX;
        }

        return CurrentStatus(StatType.Defense) +
            _weapon.Defense +
            (int)(strGrowth * GetCurrentLevel(LevelType.Strength)) +
            (int)(dexGrowth * GetCurrentLevel(LevelType.Dexterity));
    }

    private void SetWeapon(Guid _uniqueID)
    {
        if (inventory == null) inventory = Singleton.Inventory;
        var weaponInstance = inventory.GetWeaponByInventoryID(_uniqueID);

        if (weaponInstance == null) return;

        weaponInstanceId = _uniqueID;

        EquipWeapon(weaponInstance, true);
        inventory.SetIndicatorEquipped(_uniqueID);
    }

    public override WeaponItemInstance GetCurrentWeapon()
    {
        if (weaponInstanceId == System.Guid.Empty) return null;
        return Singleton.Inventory?.GetWeaponByInventoryID(weaponInstanceId);
    }
}