using UnityEngine;

public class Dummy : Enemy, IHurtable
{
    protected override void Awake()
    {
        Character = this;
        stats.UnitType = UnitType.Enemy;
    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        Debug.Log(_damage);
    }

    protected override Node CreateBehaviourTree()
    {
        throw new System.NotImplementedException();
    }
}
