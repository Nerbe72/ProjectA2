using UnityEngine;

using GameStuff;

[RequireComponent(typeof(ProjectileHandler))]
public class RangedWeapon : Weapon
{
    private ProjectileHandler projectileHandler;
    [SerializeField] private bool ShowShockwave;

    protected override void Awake()
    {
        base.Awake();

        projectileHandler = GetComponent<ProjectileHandler>();
    }

    public override void HandlerAnimation(AttackEvent _event)
    {
        switch (_event)
        {
            case AttackEvent.Projectile:
                {
                    if (ShowShockwave) Singleton.Get<ShockWaveController>().StartShock(projectileHandler.transform.position);
                    projectileHandler?.FireProjectile(owner, WeaponID);
                }
                break;
        }
    }
}
