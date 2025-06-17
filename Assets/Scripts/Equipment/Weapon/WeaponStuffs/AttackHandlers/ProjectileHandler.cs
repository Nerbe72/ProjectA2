using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform spawnPoint;

    public void FireProjectile(Character _owner, int _weaponID)
    {
        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_weaponID);
        var projectile_selected = Singleton.Get<TableDataManager>().Table.Projectile.Get(weapon_selected.ProjectileID);
        var prefab = ResourceLoader.Load<GameObject>(projectile_selected.Prefab, LoadType.ProjectilePrefab) ?? projectilePrefab;
        GameObject obj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        Projectile projectile = obj.GetComponent<Projectile>();
        projectile.InitData(projectile_selected);
        projectile.SetData(_owner, spawnPoint, Singleton.Get<TargetManager>().CurrentTarget);
    }
}
