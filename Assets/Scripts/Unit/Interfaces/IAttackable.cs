
public interface IAttackable
{
    public void Attack(AttackType _type, IHurtable _target);

    public void CalculateDamage(Character _character);
}
