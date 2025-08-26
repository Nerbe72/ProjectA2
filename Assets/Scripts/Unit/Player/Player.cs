using Photon.Pun;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

using GameStuff;
using SoundStuff;
using CameraType = GameStuff.CameraType;

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
    public AudioClip HurtSound { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected System.Guid weaponInstanceId = System.Guid.Empty;
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        
        if (photonView.IsMine)
        {
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
            Destroy(GetComponentInChildren<TargetDetector>().gameObject);
        }

        DontDestroyOnLoad(gameObject);

        Character = this;

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        PlayerRadius = GetComponent<CapsuleCollider>().radius;
        
        rigidbody.sleepThreshold = 0.001f;
        
        CheckGround();
        ResetVertical();
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

            OnLevelChanged += SyncHealthOnLevelChanged;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        CheckNormalInputs();
        UpdateAnimationParameters();
        if (CurrentState != null)


            CurrentState.Update(this);
        CheckUIInput();
        SetTargeted();

        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    TakeDamage(AttackType.Fixed, 999999);
        //}
    }

    public void PlayActionSound(PlayerActionType _actionType)
    {
        Singleton.Get<SoundManager>()?.PlayPlayerActionSound(_actionType);
    }

    private void FixedUpdate()
    {
        CheckGround();

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

        OnLevelChanged -= SyncHealthOnLevelChanged;
    }

    //bt
    public void OnJumpInput()
    {
        Debug.Log($"<color=yellow>OnJumpInput called. Current state: {CurrentState}, IsGrounded: {IsFlagged(StateFlags.Grounded)}</color>");
        CurrentState?.OnJumpInput(this);
    }

    public void TransitionTo(IPlayerState _state)
    {
        if (CurrentState != null)
            CurrentState.Exit(this);

        CurrentState = _state;
        CurrentState.Enter(this);
    }

    public override void TakeDamage(AttackType _type, int _damage)
    {
        if (IsFlagged(StateFlags.DodgeIgnored))
            return;

        int actualDamage = (int)Mathf.Max(1, _damage - (CurrentStatus(StatType.Defense) * 0.5f));
        currentHealth = Math.Clamp(currentHealth - actualDamage, 0, CurrentMaxHp);

        OnHealthChanged?.Invoke(currentHealth, CurrentMaxHp);

        if (!IsFlagged(StateFlags.Hitting)) SetFlag(StateFlags.Hit);

        Singleton.Get<PostProcessingManager>().SetHurtEffect();
        Singleton.Get<DamageIndicatorManager>().CreateIndicator(transform.position + Vector3.up, _type, actualDamage);

        base.TakeDamage(_type, _damage);

        if (currentHealth <= 0)
            Dead();
    }

    public override void Dead()
    {
        SetFlag(StateFlags.Death);
        collider.enabled = false;
        UIManager.OffBasicUI();
        
        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(3f);
        Respawn();
        yield break;
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        yield return new WaitForSeconds(1f);
        yield return Singleton.Get<PostProcessingManager>().DeadFadeIn(1.5f);
        yield return new WaitForSeconds(0.5f);
        Respawn();
        Singleton.Get<PostProcessingManager>().ResetDeadEffectImmediate();
    }

    public void UnlockMovementAfterWarp()
    {
        StartCoroutine(UnlockMovementCoroutine());
    }

    private IEnumerator UnlockMovementCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        IsMovementLocked = false;
    }

    public void Respawn()
    {
        isState = StateFlags.None;
        collider.enabled = true;

        WindowStackManager.PopAllWindows();
        UIManager.OnBasicUI();

        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
            
            transform.position = loadedData.Position;
            transform.rotation = loadedData.Rotation;

            SceneLoadManager.NextPosition = loadedData.Position;
            SceneLoadManager.NextRotation = loadedData.Rotation;
            SceneLoadManager.NextScene = (GameStuff.Map)loadedData.Scene;

            SceneManager.LoadScene(1);
        }

        currentHealth = CurrentMaxHp;
        OnHealthChanged?.Invoke(currentHealth, CurrentMaxHp);

        uint currentCurrency = Singleton.Inventory.GetCurrency();
        uint lostCurrency = (uint)(currentCurrency * 0.1f);
        Singleton.Inventory.MinusCurrency(lostCurrency);
    }

    private void SyncHealthOnLevelChanged()
    {
        currentHealth = CurrentMaxHp;
        OnHealthChanged?.Invoke(currentHealth, CurrentMaxHp);
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

    /// <summary>
    /// 워프시에만 사용 - 위치 정보를 포함한 전체 플레이어 데이터 저장
    /// </summary>
    public void SavePlayerData()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex < 4)
            return;

        Vector3 savingPosition = transform.position;
        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        var saveData = PlayerSaveData.FromPlayer(Singleton.Player, savingPosition);
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("<color=green>플레이어 데이터 저장됨 (위치 포함)</color>");
    }

    /// <summary>
    /// 위치 정보 제외한 플레이어 데이터 저장
    /// </summary>
    public void SavePlayerDataWithoutPosition()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex < 4)
            return;

        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        
        Vector3 currentPosition = Vector3.zero;
        Quaternion currentRotation = Quaternion.identity;
        GameStuff.Map currentScene = GameStuff.Map.None;
        
        if (File.Exists(savePath))
        {
            string existingJson = File.ReadAllText(savePath);
            var existingData = JsonUtility.FromJson<PlayerSaveData>(existingJson);
            currentPosition = existingData.Position;
            currentRotation = existingData.Rotation;
            currentScene = (GameStuff.Map)existingData.Scene;
        }
        
        var saveData = PlayerSaveData.FromPlayer(Singleton.Player, currentPosition);
        saveData.Rotation = currentRotation;
        saveData.Scene = (int)currentScene;
        
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("<color=green>플레이어 데이터 저장됨 (위치 제외)</color>");

        OnQuestStateChanged?.Invoke();
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

        // 퀘스트 데이터 로드
        LoadQuestData(_data.QuestData, _data.TalkCountData, _data.KillCountData);
    }

    private void SetWeapon(Guid _uniqueID)
    {
        if (inventory == null) inventory = Singleton.Inventory;
        var weaponInstance = inventory.GetWeaponByInventoryID(_uniqueID);

        if (weaponInstance == null) return;

        weaponInstanceId = _uniqueID;
        inventory.SetIndicatorEquipped(_uniqueID);
    }

    public override WeaponItemInstance GetCurrentWeapon()
    {
        if (weaponInstanceId == System.Guid.Empty) return null;
        return Singleton.Inventory?.GetWeaponByInventoryID(weaponInstanceId);
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

            currentHealth = Mathf.Min(currentHealth + _amount, CurrentMaxHp);

            // 피격시 중단
            if (IsFlagged(StateFlags.Hit)) break;

            if (time >= HealTime) break;

            yield return null;
        }
    }
}
