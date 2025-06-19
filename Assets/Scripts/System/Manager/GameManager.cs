using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    //todo: 제거
    public int InitializationPriority => 0;

    public GameObject playerPrefab;

    /// <summary>
    /// todo: playerPrefs로 불러오기
    /// </summary>
    public static Locale CurrentLocale = Locale.Korean;
    public event Action OnLocaleChanged;

    public static bool BeforeLoaded = false;
    public static bool PlayerLoaded = false;
    public static bool AfterLoaded = false;

    public static bool PhotonReady = false;

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
        if (playerPrefab == null)
        {
            Debug.LogError("<color=red><a> Missing</a></color> playerPrefab Reference.Please set it up in GameObject 'Game Manager'", this);
        }
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
        // 로그인 완료 후 Photon 서버에 연결
        PhotonNetwork.ConnectUsingSettings();
    }

    public async void LoadInventoryPlayer()
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

    /// <summary>
    /// Photon 서버 접속 후 마스터 서버에 연결된 시점 콜백
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // 룸 생성 또는 입장
        PhotonNetwork.JoinOrCreateRoom("DefaultRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    /// <summary>
    /// 룸 입장 후 호출되는 콜백
    /// </summary>
    public override void OnJoinedRoom()
    {
        // 룸 입장 후 플레이어 프리팹 로드
        if (playerPrefab == null)
        {
            Debug.LogError("<color=red><a> Missing</a></color> playerPrefab Reference.Please set it up in GameObject 'Game Manager'", this);
            return;
        }
        //PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        // 룸 입장 후 게임 시작
        //StartGame();
    }
}
