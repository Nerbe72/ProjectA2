using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

using GameStuff;
using SoundStuff;

public abstract class Boss : Character, IHurtable
{
    protected NavMeshAgent agent;
    protected Animator animator;
    public BossData BossData => stats as BossData;
    public Character Character { get; set; }
    public AudioClip HurtSound { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        
        // 보스 등장 시 보스전 BGM 시작
        StartBossBGM();
    }
    
    private void StartBossBGM()
    {
        if (BossData != null && BossData.BossBGM != null)
        {
            var soundManager = Singleton.Get<SoundManager>();
            if (soundManager != null)
            {
                soundManager.PlayBossBGM(BossData.BossBGM);
            }
        }
    }

    public virtual void SetFaced()
    {

    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        if (isDead) return; //  ¸ üũ

        int takenDamage = CalculateTakenDamage(_type, _damage);

        takenDamage = Mathf.Max(1, takenDamage); // ּ  1
        currentHealth = Math.Clamp(currentHealth - takenDamage, 0, BossData.Health);

        OnHealthChanged?.Invoke(currentHealth, BossData.Health);

        base.TakeDamage(_type, _damage);

        if (currentHealth == 0)
        {
            Dead();
        }
    }

    public override int CalculateTakenDamage(AttackType _type, int _damage)
    {
        int takenDamage = 0;
        switch (_type)
        {
            default:
            case AttackType.Physical:
            case AttackType.Magical:
                takenDamage = Mathf.Max(1, Mathf.FloorToInt(_damage - (BossData.Defense * 0.5f)));
                break;
            case AttackType.Fixed:
                takenDamage = _damage;
                break;
        }

        return takenDamage;
    }

    public override void Dead()
    {
        isDead = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Dead), true);
        agent.isStopped = true;
        isAIActive = false;

        // 보스 사망 시 맵 BGM으로 복귀
        ReturnToMapBGM();

        // 보상 지급

        // 플레이어에게 보상 지급
        int reward = BossData != null ? (int)Random.Range(BossData.RewardCurrency * 0.8f, BossData.RewardCurrency + 1) : 10;
        Singleton.Inventory.AddCurrency((uint)reward);
        Singleton.Player.KillCount.AddKillCount(BossData.ID);
    }
    
    private void ReturnToMapBGM()
    {
        var soundManager = Singleton.Get<SoundManager>();
        if (soundManager != null)
        {
            soundManager.ReturnToMapBGM();
        }
    }
    
    /// <summary>
    /// 보스 공격 패턴별 사운드 재생
    /// </summary>
    protected void PlayBossAttackSound(BossAttackPattern _pattern)
    {
        var audioClip = BossData.GetAttackSound(_pattern);
        if (audioClip != null)
        {
            Singleton.Get<SoundManager>()?.PlayEffectOneShot(audioClip);
        }
    }

    public void Knockback(float _force)
    {
        // Դ ˹ Ұ
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

    public override int GetCalculatedDamage(WeaponItemInstance _instance = null)
    {
        return stats.Damage;
    }

    public override int GetCalculatedDefense(WeaponItemInstance _instance = null)
    {
        return stats.Defense;
    }
}
