using System.Threading.Tasks;
using UnityEngine;

using GameStuff;

public class ProjectileAbilityLogic : AbilityLogic
{
    public AttackType AttackType;
    public PowerType PowerType;
    public int Damage;
    public int Amount = 0;

    public async override void ApplyAbility(Character _owner, IHurtable _target)
    {
        var ability_selected = Singleton.Get<TableDataManager>().Table.Skill.Get(AbilityId);
        var projectile_selected = Singleton.Get<TableDataManager>().Table.Projectile.Get(ability_selected.ProjectileID);

        for (int i = 0; i < Amount; i++)
        {
            var obj = GameObject.Instantiate(ResourceLoader.Load<GameObject>(projectile_selected.Prefab, LoadType.ProjectilePrefab));
            var projectile = obj.GetComponent<Projectile>();
            projectile.InitData(projectile_selected);

            AttackType finalType = AttackType;
            int finalDamage = 0;

            if (Singleton.Player == null)
            {
                finalType = AttackType.Fixed;
                finalDamage = Damage;

                goto DoSkill;
            }

            switch (PowerType)
            {
                case PowerType.Fixed:
                    finalDamage = Damage;
                    break;
                case PowerType.Percentage:
                    finalDamage = (int)(Damage * _owner.GetCalculatedDamage());
                    break;
            }

            DoSkill:
            projectile.SetData(_owner, _owner.projectileHandle, _target.Character.GetComponentInChildren<Target>(), finalType, finalDamage);

            await Task.Delay(80);
        }
    }
}