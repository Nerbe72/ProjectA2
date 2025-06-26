using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class WeaponItemInstance : ItemInstance
{
    public int Damage;
    public int Defense;

    [NonSerialized] public GameObject InstancedPrefab;

    public async Task LoadPrefabAsync()
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(ItemID);
        var prefab = await ResourceLoader.LoadAsync<GameObject>(item_selected.Prefab, LoadType.ItemPrefab);

        InstancedPrefab = prefab;

        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(item_selected.ID);

        if ((WeaponType)weapon_selected.WeaponType == WeaponType.Melee) return;
        if (InstancedPrefab == null) return;

        var projectile_selected = Singleton.Get<TableDataManager>().Table.Projectile.Get(weapon_selected.ProjectileID);

        var projectile = await ResourceLoader.LoadAsync<GameObject>(projectile_selected.Prefab, LoadType.ProjectilePrefab);

        InstancedPrefab.GetComponent<ProjectileHandler>().projectilePrefab = projectile;

        if (weapon_selected.Abilities == null || weapon_selected.Abilities.Length < 0) return;

        int count = weapon_selected.Abilities.Length;
        for (int i = 0; i < count; i++)
        {
            var ability_selected = Singleton.Get<TableDataManager>().Table.WeaponAbility.Get(weapon_selected.Abilities[i]);
            if (ability_selected == null) continue;
            if (ability_selected.ProjectileID == 0) continue;

            var projectile_ability_selected = Singleton.Get<TableDataManager>().Table.Projectile.Get(ability_selected.ProjectileID);
            await ResourceLoader.LoadAsync<GameObject>(projectile_ability_selected.Prefab, LoadType.ProjectilePrefab);
        }
    }

    public GameObject InstantiateWeapon()
    {
        GameObject obj = GameObject.Instantiate(InstancedPrefab);
        Weapon weapon = obj.GetComponent<Weapon>();
        if (weapon != null)
        {
            var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(ItemID);
            weapon.WeaponID = item_selected.ID;
        }
        return obj;
    }
}
