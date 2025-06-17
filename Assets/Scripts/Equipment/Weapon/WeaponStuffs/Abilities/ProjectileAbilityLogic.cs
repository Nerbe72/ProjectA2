using System.Net.Security;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectileAbilityLogic : AbilityLogic
{
    public int Amount = 0;

    public async override void ApplyAbility(Character _owner, IHurtable _target)
    {
        var ability_selected = Singleton.Get<TableDataManager>().Table.WeaponAbility.Get(AbilityId);
        var projectile_selected = Singleton.Get<TableDataManager>().Table.Projectile.Get(ability_selected.ProjectileID);

        for (int i = 0; i < Amount; i++)
        {
            var obj = GameObject.Instantiate(ResourceLoader.Load<GameObject>(projectile_selected.Prefab, LoadType.ProjectilePrefab));
            var projectile = obj.GetComponent<Projectile>();
            projectile.InitData(projectile_selected);
            projectile.SetData(_owner, _owner.projectileHandle, _target.Character.GetComponentInChildren<Target>(), AbilityId);
            await Task.Delay(80);
        }
    }
}