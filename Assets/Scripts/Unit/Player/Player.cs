using Photon.Pun;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Player : Character, IHurtable, IPunObservable
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

    public static PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            Destroy(GetComponentInChildren<TargetDetector>().gameObject);
            return; 
        }

        Singleton.Player = this;
        //if (Singleton.Player != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        //Singleton.Player = this;
        DontDestroyOnLoad(gameObject);

        Character = this;

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        PlayerRadius = GetComponent<CapsuleCollider>().radius;

        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        InitInput();
        TransitionTo(new IdleState());

        IsInstantiated = false;
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
        CheckGround();
        UpdateAnimationParameters();

        if (CurrentState != null)
            CurrentState.Update(this);

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        CheckNormalInputs();
        CheckUIInput();
        SetTargeted();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

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
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }
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
        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        var saveData = PlayerSaveData.FromPlayer(this);
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadPlayerData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
        
        //파일이 없을경우 초기값으로 저장
        if (!File.Exists(savePath))
        {
            var defaultSaveData = new PlayerSaveData();
            string defaultJson = JsonUtility.ToJson(defaultSaveData, true);
            File.WriteAllText(savePath, defaultJson);
        }

        string json = File.ReadAllText(savePath);
        var loadedData = JsonUtility.FromJson<PlayerSaveData>(json);

        loadedData.EquippedInventoryID = new Guid(loadedData.EquippedInventoryIDString);

        LoadLevelFromDB(
            loadedData.Level_Health,
            loadedData.Level_Strength,
            loadedData.Level_Dexterity,
            loadedData.Level_Intelligent);
        SetBaseStat(StatType.Health, loadedData.MaxHealth);
        SetBaseStat(StatType.Damage, loadedData.Damage);
        SetBaseStat(StatType.Defense, loadedData.Defense);
        SetCurrentHealth(loadedData.CurrentHealth);
        SetWeapon(loadedData.EquippedInventoryID);

        currentHealth = CurrentMaxHp;

        //transform.position = loadedData.Position;
        //transform.rotation = loadedData.Rotation;

        //씬
        //string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //if (currentScene != loadedData.SceneName)
        //{
        //    StartCoroutine(LoadSceneAndSetPosition(loadedData));
        //}
        //else
        //{
        //    transform.position = loadedData.Position;
        //    transform.rotation = loadedData.Rotation;
        //}

        IsInstantiated = true;
    }

    private IEnumerator LoadSceneAndSetPosition(PlayerSaveData saveData)
    {
        // 비동기로 씬 로드
        var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(saveData.SceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        // 씬 로드 완료 후 플레이어 위치 설정
        transform.position = saveData.Position;
        transform.rotation = saveData.Rotation;
    }

    private void SetWeapon(Guid _uniqueID)
    {
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 다른 필요한 동기화 값 처리
    }
}
