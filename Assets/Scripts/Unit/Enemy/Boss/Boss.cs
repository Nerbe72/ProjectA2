using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public abstract class Boss : Character, IHurtable
{
    protected NavMeshAgent agent;
    protected Animator animator;
    public BossData BossData => stats as BossData;
    public Character Character { get; set; }
    protected Player player;
    public event Action<int, int> OnHealthChanged;


    [SerializeField] protected bool isAIActive = false;

    protected bool isDead = false;
    protected bool isAttacking = false;
    protected bool isDying = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        Character = this;
        currentHealth = BossData.Health;
    }

    protected virtual void Start()
    {
        player = Singleton.Player;
    }

    /// <summary>
    /// 조우 트리거시 호출
    /// </summary>
    public virtual void SetFaced()
    {

    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        if (isDead) return; // 죽은 상태만 체크

        int taken = 0;
        switch (_type)
        {
            default:
            case AttackType.Physical:
            case AttackType.Magical:
                taken = Mathf.Max(1, Mathf.FloorToInt(_damage - (BossData.Defense * 0.5f)));
                break;
            case AttackType.Fixed:
                taken = _damage;
                break;
        }

        Singleton.Get<DamageIndicatorManager>().CreateIndicator(transform.position + Vector3.up, _type, taken);

        taken = Mathf.Max(1, taken); // 최소 데미지 1
        currentHealth = Math.Clamp(currentHealth - taken, 0, BossData.Health);

        OnHealthChanged?.Invoke(currentHealth, BossData.Health);

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
        isAIActive = false;

        // 맵 개방

        // 플레이어에게 재화 지급
        int reward = BossData != null ? (int)Random.Range(BossData.RewardCurrency * 0.8f, BossData.RewardCurrency + 1) : 10;
        Singleton.Inventory.AddCurrency((uint)reward);
        Singleton.Player.KillCount.AddKillCount(BossData.ID);
    }

    public void Knockback(float _force)
    {
        // 보스에게는 넉백이 불가능함
    }

    public void TakeContinuousDamage(AttackType _attack, int _time, int _damage)
    {

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
