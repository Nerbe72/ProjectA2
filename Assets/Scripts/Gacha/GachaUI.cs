using System.Threading.Tasks;
using UnityEngine;

public class GachaUI : WindowBase
{
    public bool IsGachaRunning = false;
    public int InitializationPriority => 4;

    [SerializeField] private GachaBanner gachaBanner;
    [SerializeField] private MinigameController minigameController;
    [SerializeField] private GachaResult gachaResult;

    private GachaResultData resultData;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        gachaBanner.OnSelectRoll += RequestRoll;
        minigameController.OnEndMinigame += SetMinigameResult;
        gachaResult.OnResultEnd += () => { IsGachaRunning = false; ShowBanner(); };

        WindowType = WindowType.GachaWindow;

        gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InputManager.IgnoreInput = true;
        InputManager.IgnoreUIInput = true;
        UIManager.OffBasicUI();
        ShowBanner();
    }

    private void OnDisable()
    {
        InputManager.IgnoreInput = false;
        InputManager.IgnoreUIInput = false;
        UIManager.OnBasicUI();
    }

    public void ShowBanner()
    {
        gachaBanner.Init(Singleton.Inventory.GetCurrency());
        gachaBanner.gameObject.SetActive(true);
        minigameController.gameObject.SetActive(false);
        gachaResult.gameObject.SetActive(false);
    }

    public void ShowMinigame()
    {
        gachaBanner.gameObject.SetActive(false);
        minigameController.gameObject.SetActive(true);
        gachaResult.gameObject.SetActive(false);
        minigameController.StartMinigame();
    }

    public void ShowResult()
    {
        gachaBanner.gameObject.SetActive(false);
        minigameController.gameObject.SetActive(false);
        gachaResult.gameObject.SetActive(true);
        gachaResult.ShowResult(resultData);
    }

    private void RequestRoll(BannerData _banner, int _rollCount)
    {
        uint price = (uint)(_rollCount == 1 ? _banner.SinglePrice : _banner.TenPrice);
        if (!Singleton.Inventory.IsCurrencyEnough(price))
        {
            Debug.Log("재화가 부족합니다.");
            // TODO: UI 추가
            return;
        }
        Singleton.Inventory.MinusCurrency(price);

        IsGachaRunning = true;

        resultData = new GachaResultData(_banner, _rollCount);

        // 30% 확률로 미니게임 시작
        //if (_rollCount == 10 && Random.Range(0, 100) < 30)
        //    ShowMinigame();
        //else
        //    DoCalculateResult();

        //디버그용 미니게임 100% 발동
        if (_rollCount == 10)
            ShowMinigame();
    }

    //미니게임 완료 후 호출됨
    private void SetMinigameResult(bool _success)
    {
        resultData.MinigameSuccess = _success;

        DoCalculateResult();
    }

    //추후 서버측으로 전환
    private async void DoCalculateResult()
    {
        Singleton.Get<GachaManager>().StartGacha(ref resultData);

        while(resultData.Items.Count == 0)
        {
            await Task.Yield();
        }

        //결과가 도출되면 인벤토리 저장 후 result 실행
        //무기 인스턴스 로드 포함
        for(int i = 0; i < resultData.Items.Count; i++)
        {
            WeaponItemInstance weaponInstance = new WeaponItemInstance();
            var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(resultData.Items[i].ID);
            var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(resultData.Items[i].ID);

            weaponInstance.ItemID = resultData.Items[i].ID;
            weaponInstance.Damage = resultData.Items[i].Data.Damage;
            weaponInstance.Defense = resultData.Items[i].Data.Defense;

            weaponInstance.InstancedPrefab = await ResourceLoader.LoadAsync<GameObject>(item_selected.Prefab, LoadType.ItemPrefab);
            Singleton.Inventory.TakeItem(weaponInstance);

            if (weapon_selected.Abilities == null || weapon_selected.Abilities.Length < 0) return;

            int count = weapon_selected.Abilities.Length;
            for (int j = 0; j < count; j++)
            {
                var ability_selected = Singleton.Get<TableDataManager>().Table.WeaponAbility.Get(weapon_selected.Abilities[j]);
                if (ability_selected == null || ability_selected.ProjectileID == 0) continue;

                var projectile_ability_selected = Singleton.Get<TableDataManager>().Table.Projectile.Get(ability_selected.ProjectileID);
                await ResourceLoader.LoadAsync<GameObject>(projectile_ability_selected.Prefab, LoadType.ProjectilePrefab);
            }
        }

        Singleton.Inventory.InitInventoryFrame();
        Singleton.Inventory.SaveInventoryData();

        //결과 저장
        //Task task = SingletonManager.AuthManager.SetDataAsync(Request.writegachalog, new GachaResultWrapper(logs));
        //await task.ContinueWith(task =>
        //{
        //    Debug.LogWarning("가챠정보 저장 실패, 1회 재시도합니다.");
        //}, TaskContinuationOptions.OnlyOnFaulted);


        ShowResult();
    }
}
