using System;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public StatData stats;

    public Transform WeaponHandle;
    public Transform projectileHandle;
    protected Weapon weaponPrefab;
    protected WeaponItemInstance weaponInstance;

    protected int currentHealth;

    public virtual StatData GetStats()
    {
        return stats;
    }

    public virtual (AttackType type, int damage) CalculateAttack()
    {
        if (weaponPrefab == null) return (AttackType.Physical, stats.Damage);

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
        int damage = stats.Damage + weaponInstance.Damage;
        
        return ((AttackType)weapon_selected.AttackType, damage);
    }

    public virtual (AttackType type, int damage) CalculateAttack(int _abilityID)
    {
        if (_abilityID == 0) return (0, 0);

        var ability_selected = Singleton.Get<TableDataManager>().Table.WeaponAbility.Get(_abilityID);
        if (ability_selected == null) return (0, 0);

        (AttackType type, int damage) result = (0 ,0);

        if (weaponInstance == null)
        {
            //Fixed Damage
            result.type = (AttackType)ability_selected.AttackType;
            result.damage = Mathf.FloorToInt(ability_selected.Damage);
        }
        else
        {
            //Percentage Damage
            result.type = (AttackType)ability_selected.AttackType;
            result.damage = Mathf.FloorToInt((ability_selected.Damage * 0.01f) * CalculateAttack().damage);
        }

        return result;
    }

    public virtual void TakeDamage(AttackType _type, int _damage)
    {
        int taken = 0;

        switch (_type)
        {
            default:
            case AttackType.Physical:
                taken = Mathf.FloorToInt(_damage - (stats.Defense * 0.5f));
                break;
            case AttackType.Fixed:
                taken = _damage;
                break;
        }

        taken = Mathf.Max(1, taken); // 최소 데미지 1
        currentHealth = Math.Clamp(currentHealth - taken, 0, stats.Health);

        if (currentHealth == 0)
        {
            Dead();
        }
    }

    public virtual void Dead()
    {

    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public UnitType GetSelfUnitType()
    {
        return stats.UnitType;
    }

    public WeaponItemInstance GetCurrentWeapon()
    {
        return weaponInstance;
    }
}
