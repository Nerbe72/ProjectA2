using Photon.Pun;
using System;
using UnityEngine;

using GameStuff;

public abstract class Character : MonoBehaviourPunCallbacks
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

        var weaponInstance = GetCurrentWeapon();
        if (weaponInstance == null) return (AttackType.Physical, stats.Damage);

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
        int damage = stats.Damage + weaponInstance.Damage;

        return ((AttackType)weapon_selected.AttackType, damage);
    }

    public abstract int GetCalculatedDamage(WeaponItemInstance _instance = null);

    public abstract int GetCalculatedDefense(WeaponItemInstance _instance = null);

    //public virtual (AttackType type, int damage) CalculateAttack(int _abilityID)
    //{
    //    if (_abilityID == 0) return (0, 0);

    //    var ability_selected = Singleton.Get<TableDataManager>().Table.Skill.Get(_abilityID);
    //    if (ability_selected == null) return (0, 0);

    //    (AttackType type, int damage) result = (0, 0);

    //    if (weaponInstance == null)
    //    {
    //        //Fixed Damage
    //        result.type = (AttackType)ability_selected.AttackType;
    //        result.damage = Mathf.FloorToInt(ability_selected.Power);
    //    }
    //    else
    //    {
    //        //Percentage Damage
    //        result.type = (AttackType)ability_selected.AttackType;
    //        result.damage = Mathf.FloorToInt((ability_selected.Power * 0.01f) * CalculateAttack().damage);
    //    }

    //    return result;
    //}

    public virtual void TakeDamage(AttackType _type, int _damage)
    {
        Singleton.Get<DamageIndicatorManager>().CreateIndicator(transform.position + Vector3.up, _type, CalculateTakenDamage(_type, _damage));
    }

    public virtual int CalculateTakenDamage(AttackType _type, int _damage)
    {
        return 0;
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

    public virtual WeaponItemInstance GetCurrentWeapon()
    {
        return weaponInstance;
    }

    public void PlayAttackSound()
    {
        if (weaponPrefab != null)
            weaponPrefab.PlayAttackSound();
    }

    protected virtual void PlayHitSound()
    {
        if (stats != null && stats.HurtSound != null)
        {
            Singleton.Get<SoundManager>()?.PlayEffectOneShot(stats.HurtSound);
        }
    }

    protected virtual void PlayDeathSound()
    {
        if (stats != null && stats.DeathSound != null)
        {
            Singleton.Get<SoundManager>()?.PlayEffectOneShot(stats.DeathSound);
        }
    }
}
