using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    public byte CurrentGroup = 0;

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

        PhotonNetwork.GameVersion = "1.0.0";
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 10;

        if (playerPrefab == null)
        {
            Debug.LogError("<color=red><a> Missing</a></color> 플레이어 프리팹이 없습니다.", this);
        }
    }

    public async Task ConnectAndJoinRoom()
    {
        const string roomName = "RoomOne";

        // 1) Photon 서버 접속 보장
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.ConnectUsingSettings();
            while (!PhotonNetwork.IsConnectedAndReady)
            {
                await Task.Yield();
            }
            Debug.Log("[PhotonManager] Photon 서버 연결 완료");
        }

        // 2) 룸 입장 / 생성
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log($"[PhotonManager] 룸 '{roomName}' 입장 시도");
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 8,
                EmptyRoomTtl = 0,
                PlayerTtl = 0,
                CleanupCacheOnLeave = true
            };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        // 3) 입장 완료 대기
        while (!PhotonNetwork.InRoom)
        {
            await Task.Yield();
        }
    }

    public override void OnConnectedToMaster()
    {
        // 룸 생성 또는 입장
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("로비 입장중..");
    }

    public override void OnJoinedRoom()
    {
        // 메시지 큐 일시정지 (씬 로드 완료까지 모든 네트워크 메시지 버퍼링)
        PhotonNetwork.IsMessageQueueRunning = false;
        
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            bool result = PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            Debug.Log(result ? "첫 접속 플레이어를 마스터 클라이언트로 설정 성공" : "SetMasterClient 호출 불필요 혹은 실패");
        }
        
        Debug.Log($"[Photon] 룸 입장 완료. 메시지 큐 일시정지. (PlayerCount: {PhotonNetwork.CurrentRoom.PlayerCount})");
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"마스터 클라이언트 변경됨. {newMasterClient.NickName}");
    }

    public override void OnJoinedLobby()
    {
        OnLobby();
        Debug.Log("[PhotonManager] 로비 입장 완료");
    }

    public void OnLobby()
    {
        Debug.Log("공용 로비에 접속했습니다. (포톤 연결 테스트용)");
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }

    public Player InstantiatePlayer(PlayerSaveData _data)
    {
        var gameObject = PhotonNetwork.Instantiate(playerPrefab.name, _data.Position, _data.Rotation, 0);
        return gameObject.GetComponent<Player>();
    }

    public void ChangeInterestGroup(byte _interest)
    {
        byte before = CurrentGroup;
        if (CurrentGroup != 0)
        {
            PhotonNetwork.SetInterestGroups(CurrentGroup, false);
        }

        CurrentGroup = _interest;
        PhotonNetwork.SetInterestGroups(CurrentGroup, true);

        Debug.Log($"포톤 관심 그룹 변경됨 {before} -> {CurrentGroup}");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} 플레이어가 방에 입장했습니다. (현재 {PhotonNetwork.CurrentRoom.PlayerCount}명)");
        
        // 새 플레이어의 메시지 큐도 일시정지
        if (PhotonNetwork.LocalPlayer.ActorNumber == newPlayer.ActorNumber)
        {
            PhotonNetwork.IsMessageQueueRunning = false;
        }
        
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

    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("포톤 네트워크에서 연결을 해제했습니다.");
        }
    }

    public static Dictionary<int, Photon.Realtime.Player> GetAllPlayers()
    {
        return PhotonNetwork.CurrentRoom?.Players;
    }
}
