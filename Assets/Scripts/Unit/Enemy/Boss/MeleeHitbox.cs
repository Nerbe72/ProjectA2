using UnityEngine;

public class MeleeHitbox : MonoBehaviour, IHitbox
{
    private AttackType attackType;
    private int damage;
    private Character owner;
    private Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        Deactivate();
    }

    public void Activate()
    {
        collider.enabled = true;
    }

    public void Configure(AttackType _attackType, int _damage, Character _owner)
    {
        attackType = _attackType;
        damage = _damage;
        owner = _owner;
    }

    public void Deactivate()
    {
        collider.enabled = false;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other == null) return;

        if (_other.gameObject == owner.gameObject) return;
        var hurtable = _other.GetComponent<IHurtable>();

        if (hurtable != null)
        {
            hurtable.TakeDamage(attackType, damage);
        }
    }
}
