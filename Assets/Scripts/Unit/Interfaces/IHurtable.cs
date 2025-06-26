using System;
using System.Collections;

public interface IHurtable
{
    public event Action<int, int> OnHealthChanged;

    public Character Character { get; set; }

    public void TakeDamage(AttackType _attack, int _damage);

    public void TakeContinuousDamage(AttackType _attack, int _time, int _damage);

    public void Dead();

    public IEnumerator ContinuouseDamageCoroutine(AttackType _type, int _time, int _damage);

    public void Knockback(float _force);
}
