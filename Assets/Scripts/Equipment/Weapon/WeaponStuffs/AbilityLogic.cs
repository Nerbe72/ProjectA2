public abstract class AbilityLogic : IAbility
{
    public int AbilityId { get; set; }

    public abstract void ApplyAbility(Character _owner, IHurtable _target);
}
