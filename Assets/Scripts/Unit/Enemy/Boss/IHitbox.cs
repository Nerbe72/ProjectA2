public interface IHitbox
{
    public void Configure(AttackType _attackType, int _damage, Character _owner);
    public void Activate();
    public void Deactivate();
}
