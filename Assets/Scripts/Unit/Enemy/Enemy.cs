using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
    
    protected int networkEnemyID;
    protected int networkSpawnID;
    protected int networkMapID;

    public Character Character { get; set; }
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

        // 자식 클래스에서 구현된 행동 트리를 생성하여 rootNode에 할당합니다.
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
        // AI 로직은 마스터 클라이언트에서만 실행합니다.
        if (!PhotonNetwork.IsMasterClient) return;
        if (isDead || isDying) return;

        UpdateTarget();
        rootNode?.Evaluate();
    }

    protected virtual void UpdateTarget()
    {
        // 가장 가까운 플레이어를 찾습니다.
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
        // 마스터 클라이언트가 아니면(즉, 원격 클라이언트이면) 네트워크 데이터를 기반으로 위치를 보간합니다.
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
    }

    public override void Dead()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Dead), isDead);

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;
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
}
