using GameStuff;

public interface IWeapon
{
    public void UseWeapon();
    public void PlayAttackSound();
    public void SetAttackStrategy(IAttackStrategy _attackStrategy);
    public void HandlerAnimation(AttackEvent _event);
    public void SetOutlineColor(int _enhancementCount);
}
