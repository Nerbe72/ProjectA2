using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

using GameStuff;

public class GameManager : MonoBehaviour
{
    public static Locale CurrentLocale = Locale.Korean;
    private string LocaleKey = "Locale";

    public bool BeforeLoaded;
    public bool PlayerLoaded;
    public bool AfterLoaded;

    // 게임 상태 관리
    public static bool IsGameStarted { get; private set; }

    public event Action OnLocaleChanged;
    public static event Action OnGameStarted;
    public static event Action OnGameEnded;
    public static event Action<bool> OnGamePaused; // true: 일시정지, false: 재개

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 120;
        if (!PlayerPrefs.HasKey(LocaleKey))
        {
            PlayerPrefs.SetInt(LocaleKey, (int)Locale.Korean);
        }

        CurrentLocale = (Locale)PlayerPrefs.GetInt(LocaleKey);

        BeforeLoaded = false;
        PlayerLoaded = false;
        AfterLoaded = false;
    }

    private void OnDestroy()
    {
        OnLocaleChanged = null;
    }

    public async void StartGame()
    {
        await Singleton.Get<PhotonManager>().ConnectAndJoinRoom();
        
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        
        SceneManager.LoadScene(1);
    }

    public PlayerSaveData LoadPlayerData()
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

        if (!loadedData.EquippedInventoryIDString.IsNullOrEmpty())
        {
            loadedData.EquippedInventoryID = new Guid(loadedData.EquippedInventoryIDString);
        }

        Debug.Log("<color=green>플레이어 데이터 로드 완료</color>"); 
        return loadedData;
    }

    public async Task LoadInventoryData()
    {
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
    }

    private void SaveGameData()
    {
        if (Singleton.Inventory == null || Singleton.Player == null)
        {
            Debug.LogError("인벤토리 또는 플레이어 데이터가 초기화되지 않았습니다.");
            return;
        }

        Singleton.Inventory.SaveInventoryData();
        Singleton.Player.SavePlayerDataWithoutPosition();

        Debug.Log("게임 데이터 저장 완료");
    }

    public void ChangeLocale(Locale _locale)
    {
        CurrentLocale = _locale;
        PlayerPrefs.SetInt(LocaleKey, (int)CurrentLocale);
        OnLocaleChanged?.Invoke();
    }

    public static void ShowExit()
    {
        Singleton.exit.Show();
    }

    public static void ExitGame()
    {
        // 싱글톤 역순 제거

        Singleton.UnloadAllSingleton();

        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        //SaveGameData();
        Singleton.Get<PhotonManager>().DisconnectFromPhoton();
    }
}
