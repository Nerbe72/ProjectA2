using Photon.Pun;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public abstract partial class Enemy : Character, IHurtable
{
    protected NavMeshAgent agent;
    protected Animator animator;

    protected Player player;

    public EnemyData EnemyData => stats as EnemyData;
    public Vector3 SpawnPoint_Enemy;
    public Quaternion SpawnRotation_Enemy;

    public bool SightVisual;

    [SerializeField] protected Transform eyeTransform;

    protected bool isDead = false;
    protected bool isHit = false;
    protected bool isAttack = false;
    protected bool isAttacking = false;
    protected bool isDying = false;

    public Character Character { get; set; }
    public event Action<int, int> OnHealthChanged;

    private PhotonView photonView;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();

        Character = this;
        currentHealth = EnemyData.Health;

        SpawnPoint_Enemy = transform.position;
        SpawnRotation_Enemy = transform.rotation;

        EquipWeapon();
    }

    protected virtual void Start()
    {
        player = Singleton.Player;
    }

    public bool CheckPlayerInSight()
    {
        if (player == null) return false;
        Transform sightSource = eyeTransform != null ? eyeTransform : transform;
        Vector3 eyePosition = sightSource.position;
        Vector3 targetPosition = player.transform.position;
        Vector3 directionToPlayer = (targetPosition - eyePosition).normalized;
        float distanceToPlayer = Vector3.Distance(eyePosition, targetPosition);
        if (isHit && distanceToPlayer >= EnemyData.SightDistance) return true;
        if (distanceToPlayer > EnemyData.SightDistance) return false;
        if (isAttacking && distanceToPlayer <= EnemyData.SightDistance) return true;

        float angle = EnemyData.SightAngle;
        float offset = EnemyData.SightOffset;
        Vector3 offsetDirection = sightSource.rotation * Quaternion.Euler(0, offset, 0) * Vector3.forward;
        float angleToPlayerFromSightCenter = Vector3.Angle(offsetDirection, directionToPlayer);

        if (angleToPlayerFromSightCenter > angle * 0.5f) return false;

        if (Physics.Raycast(eyePosition, directionToPlayer, distanceToPlayer * 1.01f, LayerMask.GetMask("Wall"))) return false;
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (SightVisual == false) return;

        Handles.color = Color.red;

        float angle = EnemyData.SightAngle;
        float offset = EnemyData.SightOffset;
        Transform sightSource = eyeTransform != null ? eyeTransform : transform;

        Quaternion rotation = sightSource.rotation * Quaternion.Euler(0, offset, 0);
        Vector3 startDirection = rotation * Quaternion.Euler(0, -angle * 0.5f, 0) * Vector3.forward;

        Handles.DrawSolidArc(sightSource.position, Vector3.up, startDirection, angle, EnemyData.SightDistance);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Singleton.Player.transform.position - transform.position));

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + agent.destination);
        }
        else
        {
            SpawnPoint_Enemy = transform.position;
            SpawnRotation_Enemy = transform.rotation;
        }
    }
#endif

    private void EquipWeapon()
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(EnemyData.WeaponID);
        var prefab = ResourceLoader.Load<GameObject>(item_selected.Prefab, LoadType.ItemPrefab);
        var obj = Instantiate(prefab);
        obj.transform.parent = WeaponHandle;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        var instance = new WeaponItemInstance { ItemID = EnemyData.WeaponID, Damage = EnemyData.Damage, Defense = EnemyData.Defense };

        weaponPrefab = obj.GetComponent<Weapon>();
        weaponInstance = instance;

        if (weaponPrefab != null)
        {
            weaponPrefab.SetOwner(this);
            weaponPrefab.WeaponID = instance.ItemID;
        }
    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        if (isDead) return; // 죽은 상태만 체크

        int taken = 0;
        switch (_type)
        {
            default:
            case AttackType.Physical:
                taken = Mathf.Max(1, Mathf.FloorToInt(_damage - (EnemyData.Defense * 0.5f)));
                break;
            case AttackType.Fixed:
                taken = _damage;
                break;
        }

        isHit = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Hit), true);
        Singleton.Get<DamageIndicatorManager>().CreateIndicator(transform.position + Vector3.up, _type, taken);

        taken = Mathf.Max(1, taken); // 최소 데미지 1
        currentHealth = Math.Clamp(currentHealth - taken, 0, EnemyData.Health);

        OnHealthChanged?.Invoke(currentHealth, EnemyData.Health);

        if (currentHealth == 0)
        {
            Dead();
        }
    }

    public override void Dead()
    {
        isDead = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Dead), true);
        agent.isStopped = true;

        int reward = EnemyData != null ? (int)Random.Range(EnemyData.RewardCurrency * 0.85f, EnemyData.RewardCurrency) : 10;

        Singleton.Inventory.AddCurrency((uint)reward);
        Singleton.Player.KillCount.AddKillCount(EnemyData.ID);
        Singleton.Get<EnemyManager>().SetDeadFlag(this);
    }

    public virtual void Respawn()
    {
        isDead = false;
        isHit = false;
        isAttack = false;
        isAttacking = false;
        isDying = false;

        // 체력 초기화
        currentHealth = EnemyData.Health;
        OnHealthChanged?.Invoke(currentHealth, EnemyData.Health);

        transform.position = SpawnPoint_Enemy;
        transform.rotation = SpawnRotation_Enemy;

        agent.isStopped = false;
        agent.ResetPath();

        if (animator != null)
        {
            animator.SetBool(AnimationHash.GetHash(ActionType.Dead), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Hit), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Attack), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
        }
    }

    public virtual void TakeContinuousDamage(AttackType _type, int _time, int _damage)
    {
        StartCoroutine(ContinuouseDamageCoroutine(_type, _time, _damage));
    }

    public virtual void Knockback(float _force)
    {

    }

    public virtual void CreateFollowedProjectile(AttackType _type, int _amount, float _damagePercent, GameObject _prefab)
    {

    }

    public void SetDying()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Dead), false);
        isDying = true;
    }

    public void ResetDying()
    {
        isDying = false;
    }

    protected float DistanceFromPlayer()
    {
        return Vector3.Distance(transform.position, Singleton.Player.transform.position);
    }

    protected bool CheckSpawnDistanceOut(float _dist)
    {
        return (_dist * _dist) <= (SpawnPoint_Enemy - transform.position).sqrMagnitude;
    }

    //Animation Event
    private void ReleaseBoolAction(ActionType _action)
    {
        animator.SetBool(AnimationHash.GetHash(_action), false);
    }

    public void SetAttacking()
    {
        isAttacking = true;
    }

    public void ReleaseAttacking()
    {
        isAttacking = false;
    }

    public void HandlerAnimation(AttackEvent _event)
    {
        if (weaponPrefab == null) return;
        weaponPrefab.HandlerAnimation(_event);
    }

    public void AnimationDeadEvent()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator ContinuouseDamageCoroutine(AttackType _type, int _time, int _damage)
    {
        for (int i = 0; i < _time; i++)
        {
            yield return new WaitForSeconds(1f);
            TakeDamage(_type, _damage);
        }
    }
}
