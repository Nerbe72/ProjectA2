public interface IWeapon
{
    public void UseWeapon();
    public void SetAttackStrategy(IAttackStrategy _attackStrategy);
    public void HandlerAnimation(AttackEvent _event);
}
