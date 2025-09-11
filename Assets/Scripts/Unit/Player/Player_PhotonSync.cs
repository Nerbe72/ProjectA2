using Photon.Pun;
using UnityEngine;

using GameStuff;

public partial class Player : Character, IPunObservable
{
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _info)
    {
        if (_stream.IsWriting)
        {
            _stream.SendNext(rigidbody.position);
            _stream.SendNext(rigidbody.rotation);
            _stream.SendNext(rigidbody.linearVelocity);
        }
        else
        {
            networkPosition = (Vector3)_stream.ReceiveNext();
            networkRotation = (Quaternion)_stream.ReceiveNext();
            rigidbody.linearVelocity = (Vector3)_stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - _info.SentServerTimestamp));
            networkPosition += (rigidbody.linearVelocity * lag);
        }
    }

    [PunRPC]
    public void RPCOnEquipWeapon(int _weaponID)
    {
        var tableManager = Singleton.Get<TableDataManager>().Table;

        if (tableManager == null)
        {
            Debug.LogError($"테이블이 로드되지 않았습니다. Player_PhotonSync_RPCOnEquipWeapon");
            return;
        }

        var weaponInfo = tableManager.Item.Get(_weaponID);
        var prefab = ResourceLoader.Load<GameObject>(weaponInfo.Prefab, LoadType.ItemPrefab);
        var instance = new WeaponItemInstance();
        instance.ItemID = _weaponID;
        instance.InstancedPrefab = prefab;
        Debug.Log($"RPC 무기 장착, {photonView.ViewID} {_weaponID}");
        EquipWeapon(instance, false);
    }

    public void ApplyEquipWeapon(int _weaponID)
    {
        photonView.RPC(nameof(RPCOnEquipWeapon), RpcTarget.OthersBuffered, photonView.ViewID, _weaponID);
    }
}
