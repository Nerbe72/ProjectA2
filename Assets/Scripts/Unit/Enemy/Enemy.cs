using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

using GameStuff;

[RequireComponent(typeof(NavMeshAgent))]
public abstract partial class Enemy : Character, IHurtable
{
    protected NavMeshAgent agent;
    protected Animator animator;

    protected Player currentTarget;

    public EnemyData EnemyData => stats as EnemyData;
    public Vector3 SpawnPoint_Enemy;
    public Quaternion SpawnRotation_Enemy;

    public bool SightVisual;

    [SerializeField] protected Transform eyeTransform;

    protected StateFlags isState = StateFlags.None;
    protected bool isDead = false;
    protected bool isDying = false; // 사망 판정 후 RPC가 처리되기까지의 레이턴시 동안 AI를 멈추기 위한 플래그
    protected bool isHit = false;
    protected bool isAttack = false;
    protected bool isAttacking = false;
    
    // 넉백 관련 변수들
    protected bool isKnockbacked = false;
    protected Vector3 knockbackDirection;
    protected float knockbackDistance;
    protected float originalSpeed;
    protected Coroutine knockbackCoroutine;
    
    protected int networkEnemyID;
    protected int networkSpawnID;
    protected int networkMapID;

    public Character Character { get; set; }
    public AudioClip HurtSound { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action<int, int> OnHealthChanged;

    protected new PhotonView photonView;

    protected Node rootNode;
    protected abstract Node CreateBehaviourTree();

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

        rootNode = CreateBehaviourTree();
    }

    [PunRPC]
    public void InitializeEnemyData(int _enemyID, int _spawnID, int _mapID)
    {
        Debug.Log($"[Enemy] InitializeEnemyData 호출됨: EnemyID={_enemyID}, SpawnID={_spawnID}, MapID={_mapID}");

        if (agent != null)
        {
            if (EnemyData != null)
            {
                agent.speed = EnemyData.Speed;
                agent.angularSpeed = 120f;
                agent.acceleration = EnemyData.Speed * 2.0f;
                agent.stoppingDistance = EnemyData.AttackDistance * 0.8f;
            }
        }

        networkEnemyID = _enemyID;
        networkSpawnID = _spawnID;
        networkMapID = _mapID;

        // 스폰 위치 및 회전 설정 (NavMeshAgent와의 충돌 방지)
        var spawnData = Singleton.Get<TableDataManager>().Table.Enemy.Get(_spawnID);
        if (spawnData != null)
        {
            SpawnPoint_Enemy = new Vector3(spawnData.SpawnPositionX, spawnData.SpawnPositionY, spawnData.SpawnPositionZ);
            SpawnRotation_Enemy = Quaternion.Euler(0, spawnData.SpawnRotationY, 0);

            // Warp는 NavMesh 위에 있을 때만 사용 가능
            if (agent.isOnNavMesh)
            {
                agent.Warp(SpawnPoint_Enemy);
            }
            else
            {
                transform.position = SpawnPoint_Enemy;
            }
            transform.rotation = SpawnRotation_Enemy;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isDead || isDying) return;
        
        // 넉백 중일 때는 AI 동작 중단
        if (isKnockbacked) return;

        UpdateTarget();
        rootNode?.Evaluate();
    }

    protected virtual void UpdateTarget()
    {
        float closestDistSqr = Mathf.Infinity;
        Player closestPlayer = null;
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            float distSqr = (player.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestPlayer = player;
            }
        }
        currentTarget = closestPlayer;
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }
    }

    public bool CheckPlayerInSight()
    {
        if (currentTarget == null) return false;

        Transform sightSource = eyeTransform != null ? eyeTransform : transform;
        Vector3 eyePosition = sightSource.position;
        Vector3 targetPosition = currentTarget.transform.position;
        Vector3 directionToPlayer = (targetPosition - eyePosition).normalized;
        float distanceToPlayer = Vector3.Distance(eyePosition, targetPosition);

        if (isHit && distanceToPlayer >= EnemyData.SightDistance)
            return true;

        if (distanceToPlayer > EnemyData.SightDistance)
            return false;

        if (isAttacking && distanceToPlayer <= EnemyData.SightDistance)
            return true;

        float angle = EnemyData.SightAngle;
        float offset = EnemyData.SightOffset;
        Vector3 offsetDirection = sightSource.rotation * Quaternion.Euler(0, offset, 0) * Vector3.forward;
        float angleToPlayerFromSightCenter = Vector3.Angle(offsetDirection, directionToPlayer);

        if (angleToPlayerFromSightCenter > angle * 0.5f)
            return false;

        if (Physics.Raycast(eyePosition, directionToPlayer, distanceToPlayer * 1.01f, LayerMask.GetMask("Wall")))
            return false;

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

    public void EquipWeapon(bool _broadcast = true)
    {
        EquipWeaponInternal(_broadcast);
    }

    private void EquipWeaponInternal(bool _broadcast)
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
            if (_broadcast && photonView != null && photonView.IsMine)
                ApplyEquipWeapon(instance.ItemID);
        }
    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        if (isDead || isDying) return;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError($"서버와의 접속이 끊어졌습니다. {nameof(TakeDamage)} in {name}");
            return;
        }

        photonView.RPC(nameof(TakeDamageOnMaster), RpcTarget.MasterClient, _type, _damage, PhotonNetwork.LocalPlayer.ActorNumber);

        base.TakeDamage(_type, _damage);
    }

    public override int CalculateTakenDamage(AttackType _type, int _damage)
    {
        int takenDamage = 0;
        switch (_type)
        {
            default:
            case AttackType.Physical:
                takenDamage = Mathf.Max(1, Mathf.FloorToInt(_damage - (EnemyData.Defense * 0.5f)));
                break;
            case AttackType.Fixed:
                takenDamage = _damage;
                break;
        }

        return takenDamage;
    }

    public override void Dead()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Dead), isDead);

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;
    }

    private async void CreateItemDrops()
    {
        if (EnemyData != null)
        {
            Vector3 dropPosition = transform.position + (Vector3.up * 0.05f);

            var droppedItems = await Singleton.Get<DropFactory>().CreateDrops(EnemyData, dropPosition);
            Debug.Log($"Enemy {EnemyData.ID} dropped {droppedItems.Count} items");
        }
    }

    public virtual void Respawn()
    {
        gameObject.SetActive(true);

        isDead = false;
        isDying = false;
        isHit = false;
        isAttack = false;
        isAttacking = false;

        currentHealth = EnemyData.Health;
        OnHealthChanged?.Invoke(currentHealth, EnemyData.Health);

        if (agent != null)
        {
            agent.enabled = false;
            transform.position = SpawnPoint_Enemy;
            transform.rotation = SpawnRotation_Enemy;
            agent.enabled = true;

            agent.isStopped = false;
            agent.ResetPath();
        }

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
        // 이미 넉백 중이면 무시
        if (isKnockbacked) return;
        
        // 넉백 거리 계산 (강도에 비례)
        knockbackDistance = _force * 2.0f;
        
        // 공격자 방향의 반대 방향으로 넉백
        if (currentTarget != null)
        {
            knockbackDirection = (transform.position - currentTarget.transform.position).normalized;
        }
        else
        {
            // 타겟이 없으면 랜덤 방향
            knockbackDirection = Random.insideUnitSphere.normalized;
            knockbackDirection.y = 0; // Y축은 제거
        }
        
        // 넉백 코루틴 시작
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        knockbackCoroutine = StartCoroutine(KnockbackCoroutine());
    }

    public virtual void CreateFollowedProjectile(AttackType _type, int _amount, float _damagePercent, GameObject _prefab)
    {

    }

    public override (AttackType type, int damage) CalculateAttack()
    {
        if (weaponInstance != null)
        {
            var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
            int damage = weaponInstance.Damage;
            return ((AttackType)weapon_selected.AttackType, damage);
        }
        if (stats is EnemyData enemyData)
        {
            return (enemyData.AttackType, stats.Damage);
        }
        return (AttackType.Physical, stats.Damage);
    }

    public override int GetCalculatedDamage(WeaponItemInstance _instance = null)
    {
        return stats.Damage;
    }

    public override int GetCalculatedDefense(WeaponItemInstance _instance = null)
    {
        return stats.Defense;
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
    
    /// <summary>
    /// 넉백 중인지 확인
    /// </summary>
    public bool IsKnockbacked()
    {
        return isKnockbacked;
    }
    
    /// <summary>
    /// 넉백 강제 중단
    /// </summary>
    public void StopKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }
        
        if (isKnockbacked)
        {
            agent.ResetPath();
            agent.speed = originalSpeed;
            isKnockbacked = false;
        }
    }

    protected float DistanceFromPlayer()
    {
        if (currentTarget == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, currentTarget.transform.position);
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
    
    /// <summary>
    /// NavMeshAgent를 사용한 넉백 코루틴
    /// </summary>
    protected IEnumerator KnockbackCoroutine()
    {
        // 넉백 상태 시작
        isKnockbacked = true;
        
        // 원래 속도 저장
        originalSpeed = agent.speed;
        
        // 넉백 속도 설정 (빠른 이동)
        agent.speed = originalSpeed * 3.0f;
        
        // 넉백 목표 위치 계산
        Vector3 knockbackTarget = transform.position + (knockbackDirection * knockbackDistance);
        
        // NavMesh 경로 계산
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(knockbackTarget, path))
        {
            // 경로가 유효하면 이동
            agent.SetDestination(knockbackTarget);
            
            // 넉백 이동 완료까지 대기
            while (agent.remainingDistance > 0.1f && agent.hasPath)
            {
                yield return null;
            }
        }
        else
        {
            // 경로가 유효하지 않으면 최대한 이동
            Vector3 fallbackTarget = transform.position + (knockbackDirection * (knockbackDistance * 0.5f));
            agent.SetDestination(fallbackTarget);
            
            // 짧은 시간 동안 이동
            yield return new WaitForSeconds(0.5f);
        }
        
        // 넉백 완료 후 상태 복구
        agent.ResetPath();
        agent.speed = originalSpeed;
        isKnockbacked = false;
        knockbackCoroutine = null;
        
        Debug.Log($"[Enemy] 넉백 완료: {gameObject.name}");
    }
}
