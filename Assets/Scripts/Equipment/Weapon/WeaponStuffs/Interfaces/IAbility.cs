using UnityEngine;

public interface IAbility
{
    public int AbilityId { get; set; }

    public void ApplyAbility(Character _owner, IHurtable _target);
}
