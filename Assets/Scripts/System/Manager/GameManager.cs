using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int InitializationPriority => 0;

    /// <summary>
    /// todo: playerPrefs로 불러오기
    /// </summary>
    public static Locale CurrentLocale = Locale.Korean;
    public event Action OnLocaleChanged;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
        CurrentLocale = Locale.Korean;
    }

    private void Start()
    {
        // 게임 시작 시 데이터 로드
        StartGame();
    }
    
    private void StartGame()
    {
        //디버그용. 추후 타이밍을 최초 게임 로딩시로 변경
        //로그인시 로드될 대상을 지정하고 로드됨
        LoadGameData();
    }
    
    private async void LoadGameData()
    {
        Debug.Log("테스트: user1 로그인");
        await LoginManager.LoginAsync("user1", "1234aa!");

        var loadInventory = Singleton.Inventory.LoadInventoryData();
        
        while (!loadInventory.IsCompleted)
        {
            if (loadInventory.IsFaulted)
            {
                Debug.LogError("인벤토리 데이터 로드 실패");
                return;
            }

            await Task.Yield();
        }

        Singleton.Player.LoadPlayerData();
        
        Debug.Log("<color=green>플레이어 데이터 로드 완료</color>");
    }

    private void SaveGameData()
    {
        Singleton.Inventory.SaveInventoryData();
        Singleton.Player.SavePlayerData();
        
        Debug.Log("게임 데이터 저장 완료");
    }

    public void ChangeLocale(Locale _locale)
    {
        CurrentLocale = _locale;
        OnLocaleChanged?.Invoke();
    }
    
    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}
