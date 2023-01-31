using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPun
{
    // 08.03
    // 임시 설정
    public Transform[] _startPoints = new Transform[32];
    public GameObject _playerPrefab = null;

    private PhotonView _photonView = null;

    private int _idx = 0;
    private int _seed = 0;
    private int _actorNumber = 0;

    private void Awake()
    {
        _photonView = this.gameObject.GetPhotonView();
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _seed = Random.Range(0, 66536);
            _photonView.RPC("RPC_RandomizeState", RpcTarget.AllBufferedViaServer, _seed);
        }

        _actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        _idx = (_actorNumber + Random.Range(0, 66536)) % ConstNums.maxPlayers;
        _playerPrefab = PhotonNetwork.Instantiate("player_1", Vector3.zero, Quaternion.identity);
        selectPosition(_idx);
    }

    private void selectPosition(int index)
    {
        _playerPrefab.transform.position = _startPoints[index].position;
    }

    [PunRPC]
    void RPC_RandomizeState(int seed)
    {
        Random.InitState(seed);
    }
}
