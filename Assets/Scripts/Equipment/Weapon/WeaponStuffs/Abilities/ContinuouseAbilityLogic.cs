using UnityEngine;

using GameStuff;

public class ContinuouseAbilityLogic : AbilityLogic
{
    [Header("Poison Data")]
    public AttackType AttackType;
    public PowerType PowerType;
    public int Duration;
    public int Damage;

    public override void ApplyAbility(Character _owner, IHurtable _target)
    {
        if (Singleton.Player == null)
        {
            _target.TakeContinuousDamage(AttackType, Duration, Damage);
            return;
        }

        _target.TakeContinuousDamage(AttackType, Duration, Damage * Singleton.Player.GetCalculatedDamage());
    }
}
