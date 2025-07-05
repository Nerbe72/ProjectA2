using UnityEngine;

[RequireComponent(typeof(MeleeColliderHandler))]
public class MeleeWeapon : Weapon
{
    private MeleeColliderHandler colliderHandler;

    private void Awake()
    {
        colliderHandler = GetComponent<MeleeColliderHandler>();
    }

    public override void HandlerAnimation(AttackEvent _event)
    {
        switch (_event)
        {
            case AttackEvent.MeleeStart:
                colliderHandler.OnAttackStart();
                break;
            case AttackEvent.MeleeEnd:
                colliderHandler.OnAttackEnd();
                break;
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other == null) return;

        IHurtable hurtable = _other.GetComponent<IHurtable>();
        if (hurtable == null) return;

        if (owner == null) return;

        Debug.Log($"MeleeWeapon OnTriggerEnter to: {hurtable.Character.name}");
        if (hurtable.Character.GetSelfUnitType() == owner.GetSelfUnitType()) return;

        var calculated = owner.CalculateAttack();

        this.UseWeapon();
        hurtable.TakeDamage(calculated.type, calculated.damage);
    }
}
