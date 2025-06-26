using Photon.Pun;
using UnityEngine;

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
            _stream.SendNext(currentHealth);
        }
        else
        {
            networkPosition = (Vector3)_stream.ReceiveNext();
            networkRotation = (Quaternion)_stream.ReceiveNext();
            rigidbody.linearVelocity = (Vector3)_stream.ReceiveNext();
            currentHealth = (int)_stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - _info.SentServerTimestamp));
            networkPosition += (rigidbody.linearVelocity * lag);
        }
    }

    [PunRPC]
    void RPCOnEquipWeapon(int _weaponID)
    {
        // weaponID 로 WeaponItemInstance 를 생성 또는 테이블 조회
        var weaponInfo = Singleton.Get<TableDataManager>().Table.Item.Get(_weaponID);
        var prefab = ResourceLoader.Load<GameObject>(weaponInfo.Prefab, LoadType.ItemPrefab);
        var instance = new WeaponItemInstance();
        instance.ItemID = _weaponID;
        instance.InstancedPrefab = prefab;

        EquipWeapon(instance);   // 기존 Player.EquipWeapon 호출
    }

    public void ApplyEquipWeapon(int _weaponID)
    {
        photonView.RPC(nameof(RPCOnEquipWeapon), RpcTarget.OthersBuffered, _weaponID);
    }
}
