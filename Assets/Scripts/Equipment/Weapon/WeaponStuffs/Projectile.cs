using UnityEngine;

using GameStuff;
using SoundStuff;

[RequireComponent(typeof(Animator))]
public abstract class Projectile : MonoBehaviour
{
    protected SphereCollider sphereCollider;
    protected Character owner;
    protected WeaponItemInstance weaponInstance;

    [SerializeField] protected ParticleSystem particle;

    protected AttackType attackType;
    public EffectColor projectileColor;
    public bool IsAbilityAttack;
    protected int damage;
    protected Vector3 direction;
    protected Transform target;

    protected float projectileSpeed;
    protected float curveHeight;
    protected float maxRange;

    protected Vector3 startPosition;

    protected virtual void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        //animator = GetComponent<Animator>();
    }

    protected virtual void Update() { }

    public virtual void InitData(TableProjectile.Info _bulletInfo)
    {
        projectileSpeed = _bulletInfo.Speed;
        curveHeight = _bulletInfo.Height;
        maxRange = _bulletInfo.Range;
    }

    public virtual void SetData(Character _owner, Transform _spawn, Target _target = null, AttackType _type = AttackType.Fixed, int _damage = 0)
    {
        startPosition = _spawn.position;
        transform.position = startPosition;
        direction = _spawn.forward;
        target = _target?.transform;
        owner = _owner;

        if (owner is Enemy)
        {
            var playerTransform = Singleton.Player.transform;
            direction = (playerTransform.position - startPosition).normalized;
            target = playerTransform;
        }

        IsAbilityAttack = _damage != 0;
        var calculated = IsAbilityAttack ? (_type, _damage) : _owner.CalculateAttack();

        attackType = calculated.Item1;
        damage = calculated.Item2;

        weaponInstance = _owner.GetCurrentWeapon();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other == null) goto DestroyEnd;

        IHurtable hurtable = other.GetComponentInParent<IHurtable>();

        if (hurtable == null || hurtable.Character == null)
            goto DestroyEnd;

        if (hurtable.Character.GetSelfUnitType() == owner.GetSelfUnitType())
            return;

        hurtable.TakeDamage(attackType, damage);

        if (weaponInstance != null && !IsAbilityAttack)
        {
            var weaponAdapter = Singleton.Inventory.GetWeaponAdapter(weaponInstance);
            if (weaponAdapter != null)
            {
                var activeSkills = weaponAdapter.GetActiveSkills();
                foreach (var skill in activeSkills)
                {
                    Singleton.Get<AbilityManager>().TryUseAbility(owner, skill.AbilityId, hurtable);
                }
            }
        }

    DestroyEnd:
        //Singleton.EffectManager.StartEffect(projectileColor, transform.position);
        particle.transform.SetParent(null);
        particle.transform.position = other.ClosestPoint(particle.transform.position);
        particle.Play();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
