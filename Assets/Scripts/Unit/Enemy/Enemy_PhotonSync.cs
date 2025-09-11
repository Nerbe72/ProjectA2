using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System;

using GameStuff;

public abstract partial class Enemy : Character, IPunObservable
{
    // 네트워크 동기화
    protected Vector3 networkPosition;
    protected Quaternion networkRotation;
    private bool isInitialized = false;

    public bool IsDead => isDead;
    public int CurrentHealth { get; private set; }

    public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _info)
    {
        if (_stream.IsWriting)
        {
            _stream.SendNext(transform.position);
            _stream.SendNext(transform.rotation);
        }
        else
        {
            this.networkPosition = (Vector3)_stream.ReceiveNext();
            this.networkRotation = (Quaternion)_stream.ReceiveNext();

            if (!isInitialized)
            {
                transform.position = this.networkPosition;
                transform.rotation = this.networkRotation;
                if(agent != null && agent.isOnNavMesh)
                {
                    agent.Warp(transform.position);
                }
                isInitialized = true;
            }
        }
    }

    protected void InitializePhotonSync()
    {
        if (photonView == null) photonView = GetComponent<PhotonView>();
    }
    
    protected new void OnEnable()
    {
        base.OnEnable();
        InitializePhotonSync();
    }

    [PunRPC]
    public void RPCOnEquipWeapon(int _weaponID)
    {
        var tableManager = Singleton.Get<TableDataManager>().Table;

        if (tableManager == null)
        {
            Debug.LogError($"테이블이 로드되지 않았습니다. Player_PhotonSync_RPCOnEquipWeapon");
            return;
        }

        var weaponInfo = tableManager.Item.Get(_weaponID);
        Debug.Log(weaponInfo.Prefab);
        var prefab = ResourceLoader.Load<GameObject>(weaponInfo.Prefab, LoadType.ItemPrefab);
        var instance = new WeaponItemInstance();
        instance.ItemID = _weaponID;
        instance.InstancedPrefab = prefab;
        EquipWeapon();
    }

    public void ApplyEquipWeapon(int _weaponID)
    {
        photonView.RPC(nameof(RPCOnEquipWeapon), RpcTarget.OthersBuffered, _weaponID);
    }

    [PunRPC]
    protected void TakeDamageOnMaster(AttackType _type, int _damage, int _attackerActorNumber)
    {
        if (isDead || isDying) return;

        int takenDamage = CalculateTakenDamage(_type, _damage);

        currentHealth = Math.Clamp(currentHealth - takenDamage, 0, stats.Health);
        OnHealthChanged?.Invoke(currentHealth, EnemyData.Health);
        Singleton.Get<DamageIndicatorManager>().CreateIndicator(transform.position + Vector3.up, _type, takenDamage);

        if (currentHealth > 0)
        {
            photonView.RPC(nameof(SyncHit), RpcTarget.All, currentHealth);
        }
        else
        {
            currentHealth = 0;
            isDying = true; // AI 정지
            photonView.RPC(nameof(SyncDead), RpcTarget.AllBuffered, _attackerActorNumber);
        }

        base.TakeDamage(_type, _damage);
    }

    [PunRPC]
    protected void SyncHit(int _newHealth)
    {
        if (isDead) return;

        int previousHealth = currentHealth;
        currentHealth = _newHealth;
        if (currentHealth < previousHealth)
        { 
            OnHealthChanged?.Invoke(currentHealth, EnemyData.Health);
        }

        isHit = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Hit), true);
    }

    [PunRPC]
    public void SyncDead(int killerActorNumber = -1)
    {
        if (isDead) return;

        Dead();

        if (PhotonNetwork.IsMasterClient)
        {
            int reward = EnemyData != null ? (int)UnityEngine.Random.Range(EnemyData.RewardCurrency * 0.85f, EnemyData.RewardCurrency) : 10;
            photonView.RPC(nameof(GiveReward), RpcTarget.AllBuffered, reward, killerActorNumber);
            Singleton.Get<EnemyManager>().SetDeadFlag(this);

            if (killerActorNumber >= 0)
            {
                photonView.RPC(nameof(SpawnDrop), RpcTarget.All, killerActorNumber);
            }
        }
    }
    
    [PunRPC]
    public void GiveReward(int _reward, int _killerActorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == _killerActorNumber)
        {
            Singleton.Inventory.AddCurrency((uint)_reward);
            Singleton.Player.KillCount.AddKillCount(EnemyData.ID);
        }
    }

    [PunRPC]
    public void SpawnDrop(int _killerActorNumber)
    {
        Debug.Log(_killerActorNumber);
        if (PhotonNetwork.LocalPlayer.ActorNumber == _killerActorNumber)
        {
            CreateItemDrops();
        }
    }
    
    [PunRPC]
    public void SyncRespawn()
    {
        Respawn();
        GetComponentInChildren<HeadlHealthIndicator>()?.UpdateHealth(currentHealth, EnemyData.Health);

        isDead = false;
        isHit = false;
        isAttack = false;
        isAttacking = false;
        isDying = false;

        currentHealth = EnemyData.Health;
        OnHealthChanged?.Invoke(currentHealth, EnemyData.Health);

        // 위치/회전 리셋은 이미 마스터에서 전송됨
        
        if (animator != null)
        {
            animator.SetBool(AnimationHash.GetHash(ActionType.Dead), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Hit), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Attack), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
        }
    }
}
