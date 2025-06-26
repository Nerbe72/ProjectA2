using Photon.Pun;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using WebSocketSharp;

public partial class Player : Character, IHurtable
{
    public int InitializationPriority => 2;
    [HideInInspector] public bool IsInstantiated = false;
    public IPlayerState CurrentState { get; private set; }
    private new Rigidbody rigidbody;
    private CapsuleCollider collider;
    private Animator animator;
    private Inventory inventory;
    public float HealTime;
    public Character Character { get; set; }
    public float PlayerRadius = 0.4f;
    public bool IsMovementLocked { get; set; } = false;
    public bool IsLoadingScene { get; set; } = false;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        
        if (photonView.IsMine)
        {
            // 이미 로컬 플레이어가 존재하면 중복 생성 방지
            if (Singleton.Player != null)
            {
                Destroy(gameObject);
                return;
            }

            Singleton.Player = this;
            

            InitInput();
            TransitionTo(new IdleState());

            IsInstantiated = false;
        }
        else
        {
            // 원격 플레이어의 불필요한 컴포넌트 제거
            Destroy(GetComponentInChildren<TargetDetector>().gameObject);
        }

        DontDestroyOnLoad(gameObject);

        Character = this;

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        PlayerRadius = GetComponent<CapsuleCollider>().radius;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            targetManager = Singleton.Get<TargetManager>();
            cameraManager = Singleton.Get<CameraManager>();
            inventory = Singleton.Inventory;
            inventory.OnWeaponEquipped += EquipWeapon;
            cameraManager.SetCameraTo(CameraType.Main, transform);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        CheckGround();
        UpdateAnimationParameters();

        if (CurrentState != null)
            CurrentState.Update(this);


        CheckNormalInputs();
        CheckUIInput();
        SetTargeted();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            rigidbody.position = Vector3.MoveTowards(rigidbody.position, networkPosition, Time.fixedDeltaTime);
            rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, networkRotation, Time.fixedDeltaTime);
        }

        if (CurrentState != null)
            CurrentState.FixedUpdate(this);
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnWeaponEquipped -= EquipWeapon;
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }

    void OnLevelWasLoaded(int level)
    {
        CalledOnLevelWasLoaded(level);
    }

    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        //if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        //{
        //    transform.position = new Vector3(0f, 5f, 0f);
        //}
    }

    //bt
    public void TransitionTo(IPlayerState _state)
    {
        if (CurrentState != null)
            CurrentState.Exit(this);

        CurrentState = _state;
        CurrentState.Enter(this);
    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        int actualDamage = (int)Mathf.Max(1, _damage - (CurrentStatus(StatType.Defense) * 0.5f));
        currentHealth = Math.Clamp(currentHealth - actualDamage, 0, CurrentMaxHp);

        OnHealthChanged?.Invoke(currentHealth, CurrentMaxHp);

        if (!IsFlagged(StateFlags.Hitting)) SetFlag(StateFlags.Hit);

        Singleton.Get<DamageIndicatorManager>().CreateIndicator(transform.position + Vector3.up, _type, actualDamage);

        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    public override void Dead()
    {
        SetFlag(StateFlags.Death);
        collider.enabled = false;
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(3f);
        Respawn();
        yield break;
    }

    public void UnlockMovementAfterWarp()
    {
        StartCoroutine(UnlockMovementCoroutine());
    }

    private IEnumerator UnlockMovementCoroutine()
    {
        // 물리 프레임이 한 번 지난 후에 이동 제한을 해제하여 안정성을 확보합니다.
        yield return new WaitForFixedUpdate();
        IsMovementLocked = false;
    }

    public void Respawn()
    {
        isState = StateFlags.None;
        collider.enabled = true;

        WindowStackManager.ResetWindowStack();
        UIManager.OffBasicUI();

        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
            transform.position = loadedData.Position;
            transform.rotation = loadedData.Rotation;
        }
        currentHealth = CurrentMaxHp;

        InputManager.IgnoreInput = false;

        uint currentCurrency = Singleton.Inventory.GetCurrency();
        uint lostCurrency = (uint)(currentCurrency * 0.1f);
        Singleton.Inventory.MinusCurrency(lostCurrency);
        // todo: 사망 표시 부활 애니메이션
    }

    public void HandlerAnimation(AttackEvent _event)
    {
        if (weaponPrefab == null) return;

        weaponPrefab.HandlerAnimation(_event);
    }

    public void HealCoroutine(int _amount)
    {
        StartCoroutine(HealEnumerator(_amount));
    }

    public void TakeContinuousDamage(AttackType _type, int _time, int _damage)
    {
        StartCoroutine(ContinuouseDamageCoroutine(_type, _time, _damage));
    }

    public IEnumerator ContinuouseDamageCoroutine(AttackType _type, int _time, int _damage)
    {
        for (int i = 0; i < _time; i++)
        {
            yield return new WaitForSeconds(1f);
            TakeDamage(_type, _damage);
        }
    }

    public void Knockback(float _force)
    {

    }

    public void CreateFollowedProjectile(AttackType _type, int _amount, float _damagePercent, GameObject _prefab)
    {

    }

    public void SavePlayerData()
    {
        Vector3 savingPosition = transform.position;
        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        var saveData = PlayerSaveData.FromPlayer(Singleton.Player, savingPosition);
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public void SetPlayerDataFromLoaded(PlayerSaveData _data)
    {
        LoadLevelFromDB(
            _data.Level_Health,
            _data.Level_Strength,
            _data.Level_Dexterity,
            _data.Level_Intelligent);
        SetBaseStat(StatType.Health, _data.MaxHealth);
        SetBaseStat(StatType.Damage, _data.Damage);
        SetBaseStat(StatType.Defense, _data.Defense);
        SetCurrentHealth(_data.CurrentHealth);
        
        if (!_data.EquippedInventoryIDString.IsNullOrEmpty())
        {
            _data.EquippedInventoryID = new Guid(_data.EquippedInventoryIDString);
            SetWeapon(_data.EquippedInventoryID);
        }

        currentHealth = CurrentMaxHp;
        IsInstantiated = true;
    }

    private void SetWeapon(Guid _uniqueID)
    {
        if (inventory == null) inventory = Singleton.Inventory;
        var weaponInstance = inventory.GetWeaponByInventoryID(_uniqueID);

        if (weaponInstance == null) return;

        inventory.SetIndicatorEquipped(_uniqueID);
    }

    public override StatData GetStats()
    {
        return stats;
    }

    public void SetCurrentHealth(int _health)
    {
        currentHealth = Mathf.Clamp(_health, 0, CurrentMaxHp);
    }

    public IEnumerator HealEnumerator(int _amount)
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;

            // 체력 회복
            currentHealth = Mathf.Min(currentHealth + _amount, CurrentMaxHp);

            // 피격시 중단
            if (IsFlagged(StateFlags.Hit)) break;

            if (time >= HealTime) break;

            yield return null;
        }
    }
}
