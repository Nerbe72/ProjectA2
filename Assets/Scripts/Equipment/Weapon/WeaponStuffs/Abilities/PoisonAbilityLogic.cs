using UnityEngine;

public class PoisonAbilityLogic : AbilityLogic
{
    [Header("Poison Data")]
    public AttackType AttackType;
    public int PoisonTime;
    public int PoisonDamage;
    public int GrowthTime;
    public int GrowthDamage;

    public override void ApplyAbility(Character _owner, IHurtable _target)
    {
        _target.TakeContinuousDamage(AttackType, PoisonTime, PoisonDamage);
    }
}
