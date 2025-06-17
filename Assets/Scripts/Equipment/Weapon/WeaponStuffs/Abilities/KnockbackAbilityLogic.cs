using UnityEngine;

public class KnockbackAbilityLogic : AbilityLogic
{
    [Header("어빌리티 파라미터")]
    public float KnockbackForce;
    public float Growth;

    public override void ApplyAbility(Character _owner, IHurtable _target)
    {
        _target.Knockback(KnockbackForce);
    }
}
