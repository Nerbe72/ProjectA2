using Photon.Pun;
using UnityEngine;

public abstract partial class Enemy : Character, IPunObservable
{
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _info)
    {
        if (_stream.IsWriting)
        {
            _stream.SendNext(agent.destination);
            _stream.SendNext(transform.position);
            _stream.SendNext(transform.rotation);
            _stream.SendNext(agent.velocity);
            _stream.SendNext(currentHealth);
        }
        else
        {
            Vector3 destination = (Vector3)_stream.ReceiveNext();
            Vector3 position = (Vector3)_stream.ReceiveNext();
            Quaternion rotation = (Quaternion)_stream.ReceiveNext();
            Vector3 velocity = (Vector3)_stream.ReceiveNext();
            int health = (int)_stream.ReceiveNext();

            currentHealth = health;
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - _info.SentServerTimestamp));
            networkPosition += (agent.velocity * lag);
        }
    }

    
}
