using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    [HideInInspector] public int WeaponID;
    protected IAttackStrategy attackStrategy;
    protected Character owner;

    public void SetOwner(Character _owner)
    {
        owner = _owner;
    }

    public virtual void UseWeapon()
    {
        if (attackStrategy == null) return;
        attackStrategy.ExecuteAttack(this);
    }

    public void SetAttackStrategy(IAttackStrategy _strategy)
    {
        attackStrategy = _strategy;
    }

    public virtual void HandlerAnimation(AttackEvent _event) { }
}
