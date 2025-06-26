using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class GameManager : MonoBehaviourPunCallbacks
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

    // 네트워크 이벤트
    public static event Action<Photon.Realtime.Player> OnPlayerJoinedRoomEvent;
    public static event Action<Photon.Realtime.Player> OnPlayerLeftRoomEvent;

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

    public async void StartGame()
    {
        const string roomName = "RoomOne";
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log($"[PhotonManager] 룸 '{roomName}' 입장 시도");
            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 8 }, TypedLobby.Default);
        }

        while (!PhotonNetwork.InRoom)
        {
            await Task.Yield();
        }

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
        
        SceneLoadManager.NextPosition = loadedData.Position;
        SceneLoadManager.NextRotation = loadedData.Rotation;
        SceneLoadManager.NextScene = (Map)loadedData.Scene;

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
        Singleton.Player.SavePlayerData();

        Debug.Log("게임 데이터 저장 완료");
    }

    public void ChangeLocale(Locale _locale)
    {
        CurrentLocale = _locale;
        PlayerPrefs.SetInt(LocaleKey, (int)CurrentLocale);
        OnLocaleChanged?.Invoke();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
        DisconnectFromPhoton();
    }

    #region 게임 상태 관리

    /// <summary>
    /// 게임 시작 시 호출 (마스터 클라이언트만 호출 가능)
    /// </summary>
    public void StartGameSession()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        IsGameStarted = true;
        photonView.RPC(nameof(RPC_StartGameSession), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartGameSession()
    {
        IsGameStarted = true;
        OnGameStarted?.Invoke();
        Debug.Log("게임 세션이 시작되었습니다.");

        // 게임 시작 시 필요한 초기화 작업 수행
        InitializeGame();
    }

    /// <summary>
    /// 게임 종료 시 호출 (마스터 클라이언트만 호출 가능)
    /// </summary>
    public void EndGameSession()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        IsGameStarted = false;
        photonView.RPC(nameof(RPC_EndGameSession), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_EndGameSession()
    {
        IsGameStarted = false;
        OnGameEnded?.Invoke();
        Debug.Log("게임 세션이 종료되었습니다.");
    }

    private void InitializeGame()
    {
        // 게임 시작 시 필요한 초기화 작업을 여기에 추가
        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터 클라이언트만 실행할 초기화 코드
        }

        // 모든 클라이언트가 실행할 초기화 코드
    }

    #endregion

    #region 네트워크 이벤트 핸들러

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} 플레이어가 방에 입장했습니다. (현재 {PhotonNetwork.CurrentRoom.PlayerCount}명)");
        OnPlayerJoinedRoomEvent?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} 플레이어가 방을 나갔습니다. (현재 {PhotonNetwork.CurrentRoom.PlayerCount}명)");
        OnPlayerLeftRoomEvent?.Invoke(otherPlayer);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log($"네트워크 연결이 끊어졌습니다: {cause}");
        // 연결 끊김 처리 (예: 연결 오류 메시지 표시 후 메인 메뉴로 이동)
        SceneManager.LoadScene("MainMenu");
    }

    #endregion

    #region 유틸리티 함수

    /// <summary>
    /// 모든 클라이언트에서 씬을 로드합니다 (마스터 클라이언트만 호출 가능).
    /// </summary>
    public void LoadLevelForAll(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_LoadLevel), RpcTarget.All, sceneName);
    }

    [PunRPC]
    private void RPC_LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 포톤 네트워크에서 연결을 해제합니다.
    /// </summary>
    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("포톤 네트워크에서 연결을 해제했습니다.");
        }
    }

    /// <summary>
    /// 현재 방의 모든 플레이어 정보를 반환합니다.
    /// </summary>
    public static Dictionary<int, Photon.Realtime.Player> GetAllPlayers()
    {
        return PhotonNetwork.CurrentRoom?.Players;
    }

    #endregion
}
