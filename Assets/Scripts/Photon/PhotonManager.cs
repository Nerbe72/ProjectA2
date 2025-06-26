using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public partial class PhotonManager : MonoBehaviourPunCallbacks
{
    private PhotonView photonView;
    [SerializeField] private GameObject playerPrefab;
    public byte CurrentGroup = 0;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        photonView = GetComponent<PhotonView>();

        PhotonNetwork.GameVersion = "1.0.0";
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 10;

        if (playerPrefab == null)
        {
            Debug.LogError("<color=red><a> Missing</a></color> 플레이어 프리팹이 없습니다.", this);
        }
    }

    [PunRPC]
    private void OnApplyAttack(IHurtable _target, AttackType _attackType, int _damage)
    {
        _target.TakeDamage(_attackType, _damage);
    }

    public void ApplyAttack(IHurtable _target, AttackType _attackType, int _damage)
    {
        photonView.RPC("OnApplyAttack", RpcTarget.OthersBuffered, _target, _attackType, _damage);
    }

    public void OnApplyEnemyDead(IHurtable _target)
    {
        if (_target == null) return;
        _target.Dead();
    }

    public void ApplyEnemyDead(IHurtable _target)
    {
        photonView.RPC("OnApplyEnemyDead", RpcTarget.Others, _target);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
    }

    /// <summary>
    /// Photon 서버 접속 후 마스터 서버에 연결된 시점 콜백
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // 룸 생성 또는 입장
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("로비 입장중..");
    }

    /// <summary>
    /// 룸 입장 후 호출되는 콜백
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("룸 입장함");
    }

    public override void OnJoinedLobby()
    {
        OnLobby();
        Debug.Log("[PhotonManager] 로비 입장 완료");
    }

    public void OnLobby()
    {
        PhotonNetwork.IsMessageQueueRunning = true;
        Debug.Log("공용 로비에 접속했습니다. (포톤 연결 테스트용)");
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }

    public Player InstantiatePlayer(PlayerSaveData _data)
    {
        // 모든 클라이언트가 자신의 플레이어를 생성해야 하므로
        // photonView.IsMine 여부를 확인하지 않는다.
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
}
