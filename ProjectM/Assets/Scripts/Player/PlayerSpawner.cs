using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPun
{
    // 08.03
    // 임시 설정
    public Transform[] _startPoints = new Transform[32];
    public GameObject _playerPrefab = null;

    private PhotonView _photonView = null;

    private int _idx = 0;
    public int _rand = 0;
    private int _actorNumber = 0;

    private void Awake()
    {
        _photonView = this.gameObject.GetPhotonView();
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            _rand = Random.Range(0, 66536);
            _photonView.RPC("RPC_RandomizeState", RpcTarget.AllBufferedViaServer, _rand);
        }
    }
    
    private void SpawnPlayer()
    {
        _actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        _idx = (_actorNumber + _rand) % ConstNums.maxPlayers;
        _playerPrefab = PhotonNetwork.Instantiate("player_1", Vector3.zero, Quaternion.identity);
        _playerPrefab.transform.position = _startPoints[_idx].position;
    }

    [PunRPC]
    public void RPC_RandomizeState(int rand)
    {
        _rand = rand;

        SpawnPlayer();
    }
}
