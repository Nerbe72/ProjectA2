using System.Threading.Tasks;
using UnityEngine;

using GameStuff;

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
            Debug.Log("��ȭ�� �����մϴ�.");
            // TODO: UI �߰�
            return;
        }
        Singleton.Inventory.MinusCurrency(price);

        IsGachaRunning = true;

        resultData = new GachaResultData(_banner, _rollCount);

        // 30% Ȯ���� �̴ϰ��� ����
        //if (_rollCount == 10 && Random.Range(0, 100) < 30)
        //    ShowMinigame();
        //else
        //    DoCalculateResult();

        //����׿� �̴ϰ��� 100% �ߵ�
        if (_rollCount == 10)
            ShowMinigame();
    }

    //�̴ϰ��� �Ϸ� �� ȣ���
    private void SetMinigameResult(bool _success)
    {
        resultData.MinigameSuccess = _success;

        CalculateResult();
    }

    private async void CalculateResult()
    {
        Singleton.Get<GachaManager>().StartGacha(ref resultData);

        while (resultData.Items.Count == 0)
        {
            await Task.Yield();
        }

        for (int i = 0; i < resultData.Items.Count; i++)
        {
            bool success = await Singleton.Get<ItemFactory>().CreateAndAddToInventoryAsync(
                resultData.Items[i].ID,
                resultData.Items[i].Data.Damage,
                resultData.Items[i].Data.Defense
            );

            if (!success)
            {
                Debug.LogError($"GachaUI: 아이템 생성 및 인벤토리 추가에 실패했습니다. ItemID: {resultData.Items[i].ID}");
            }
        }
        
        Singleton.Inventory.SaveInventoryData();

        //재업로드
        //Task task = SingletonManager.AuthManager.SetDataAsync(Request.writegachalog, new GachaResultWrapper(logs));
        //await task.ContinueWith(task =>
        //{
        //    Debug.LogWarning("Upload Error. Try Again");
        //}, TaskContinuationOptions.OnlyOnFaulted);

        ShowResult();
    }
}
