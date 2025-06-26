using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    private Dictionary<int, AbilityLogic> abilities;

    public int InitializationPriority => 4;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        abilities = new Dictionary<int, AbilityLogic>();
    }

    public AbilityLogic GetAbility(int _id)
    {
        if (abilities.ContainsKey(_id)) return abilities[_id];

        var ability_selected = Singleton.Get<TableDataManager>().Table.WeaponAbility.Get(_id);

        //투사체 생성
        if (ability_selected.ProjectileID != 0)
        {
            var projectile = Singleton.Get<TableDataManager>().Table.Projectile.Get(ability_selected.ProjectileID);

            ProjectileAbilityLogic abilityData = new ProjectileAbilityLogic();
            abilityData.AbilityId = ability_selected.ID;
            abilityData.Amount = ability_selected.Projectile_Amount;

            abilities.Add(_id, abilityData);
            return abilityData;
        }

        //지속피해
        if (ability_selected.ContinuouseTime != 0)
        {
            PoisonAbilityLogic abilityData = new PoisonAbilityLogic();
            abilityData.AbilityId = ability_selected.ID;
            abilityData.AttackType = (AttackType)ability_selected.AttackType;
            abilityData.PoisonDamage = ability_selected.Damage;
            abilityData.PoisonTime = ability_selected.ContinuouseTime;

            abilities.Add(_id, abilityData);
            return abilityData;
        }

        if (ability_selected.KnockbackForce != 0)
        {
            KnockbackAbilityLogic abilityData = new KnockbackAbilityLogic();
            abilityData.AbilityId = ability_selected.ID;
            abilityData.KnockbackForce = ability_selected.KnockbackForce;

            abilities.Add(_id, abilityData);
            return abilityData;
        }

        return null;
    }
}
