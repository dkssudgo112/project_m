using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static NetworkManager Instance;

    private PhotonView _photonView = null;

    [Header("Game Info")]
    [SerializeField]
    private bool _gameEnd = false;

    [Header("Network Info")]
    [Header("Player Data")]
    [SerializeField] 
    private byte _playerCount = 0;
    [SerializeField] 
    private int _maxPlayers = 0;
    [SerializeField] 
    private byte _currentAlive = 0;

    [Header("Time Data")]
    [SerializeField] 
    private int _networkTime = 0;
    //private double _delay = 0;

    [Header("Random Data")]
    [SerializeField] 
    private int _seed = 0;

    [Header("Magnetic Data")]
    [SerializeField] 
    private int _currentPhase = 0;

    #region UNITY

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log($"Fail : instantiate NetworkManager.");
        }

        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            _playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            _maxPlayers = PhotonNetwork.CurrentRoom.PlayerCount + ConstNums.aiCount;

            Sync();

            StartGame();
        }
    }



    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {/*
            if (PhotonNetwork.CurrentRoom.PlayerCount < _playerCount)
            {
                Debug.Log($"{_currentAlive} , {_playerCount} , {PhotonNetwork.CurrentRoom.PlayerCount}");
                DecreaseAlive(true);
                Debug.Log($"{_currentAlive} , {_playerCount} , {PhotonNetwork.CurrentRoom.PlayerCount}");
            }
            */
            _networkTime = PhotonNetwork.ServerTimestamp;
        }
    }

    #endregion

    #region IPunObservable

    // 네트워크 시간 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(this._networkTime);
        }
        else if (stream.IsReading && !PhotonNetwork.IsMasterClient)
        {
            _networkTime = (int)stream.ReceiveNext();

            // 네트워크 딜레이 계산법
            //_delay = Mathf.Abs((float)(_networkTime - info.SentServerTime));
        }
    }

    #endregion

    #region PUBLIC

    public int GetTime() =>
        _networkTime;

    public int GetMaxPlayers() =>
        _maxPlayers;

    public byte GetAlive() =>
        _currentAlive;

    public int GetPhase() =>
        _currentPhase;

    public bool IsEnd() =>
        _gameEnd;

    public void NextPhase()
    {
        _currentPhase++;

        if (_currentPhase <= ConstNums.lastPhase)
        {
            UIManager.Instance.CreateGuide(_currentPhase, ConstNums.PhaseTime[_currentPhase]);
            MagnetTimer.Instance.Run(_currentPhase);
            MagnetActivator.MoveToNextPhaze(_currentPhase);
        }
        else
        {
            Debug.Log("Reach the Last Phase");
        }
    }

    public void DecreaseAlive(bool isPlayer)
    {
        if (_gameEnd == true)
        {
            return;
        }

        if (isPlayer == true)
        {
            //_playerCount -= 1;
            //_photonView.RPC("RPC_UpdatePlayerCount", RpcTarget.OthersBuffered, _playerCount);
            _photonView.RPC("RPC_DecreasePlayerCount", RpcTarget.AllBufferedViaServer);
        }

        //_currentAlive -= 1;
        //byte newAliveCount = _currentAlive;
        //_photonView.RPC("RPC_UpdateAlive", RpcTarget.AllBufferedViaServer, newAliveCount);
        _photonView.RPC("RPC_DecreaseAlive", RpcTarget.AllBufferedViaServer);
    }

    public void RequestDropItemToMaster(string name, Vector3 spawnPos, Vector2 spreadDir)
    {
        _photonView.RPC("RPC_CallDropItemByMaster", RpcTarget.MasterClient, name, ItemManager.GetItemByName(name).itemData.itemCount, spawnPos, spreadDir);
    }

    public void RequestDropItemToMaster(string name, int itemCount, Vector3 spawnPos, Vector2 spreadDir)
    {
        _photonView.RPC("RPC_CallDropItemByMaster", RpcTarget.MasterClient, name, itemCount, spawnPos, spreadDir);
    }

    public void DisableItemByViewID(int viewID)
    {
        _photonView.RPC("RPC_DisableItemByViewID", RpcTarget.AllBuffered, viewID);
    }

    public void ExitGame() =>
        PhotonNetwork.LeaveRoom();

    #endregion

    #region PUN CALLBACKS

    public override void OnLeftRoom()
    {
        UIManager.Instance.ChangeActiveCursor(false);
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause) =>
        PhotonNetwork.LoadLevel("Lobby");

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient == true && (PhotonNetwork.CurrentRoom.PlayerCount < _playerCount))
        {
            DecreaseAlive(true);
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient == true && (PhotonNetwork.CurrentRoom.PlayerCount < _playerCount))
        {
            DecreaseAlive(true);
        }
    }

    #endregion

    #region PRIVATE

    private void Sync()
    {
        SyncAlive();
        SyncSeed();
    }

    private void SyncAlive()
    {
        byte aliveCount = (byte)_maxPlayers;
        _photonView.RPC("RPC_UpdatePlayerCount", RpcTarget.AllBufferedViaServer, _playerCount);
        _photonView.RPC("RPC_UpdateMaxPlayer", RpcTarget.AllBufferedViaServer, aliveCount);
        _photonView.RPC("RPC_UpdateAlive", RpcTarget.AllBufferedViaServer, aliveCount);
    }

    [PunRPC]
    private void RPC_UpdatePlayerCount(byte newPlayerCount)
    {
        _playerCount = newPlayerCount;
    }

    [PunRPC]
    private void RPC_UpdateMaxPlayer(byte maxPlayers)
    {
        _maxPlayers = maxPlayers;
    }

    [PunRPC]
    private void RPC_UpdateAlive(byte aliveCount)
    {
        _currentAlive = aliveCount;
        UIManager.Instance.RefreshAlive(_currentAlive);

        // 승리 알림
        if (_currentAlive == 1)
        {
            _gameEnd = true;
            PlayerManager.Instance.GameOver();
            UIManager.Instance._leaveButton.SetActive(true);
        }
    }

    [PunRPC]
    private void RPC_DecreasePlayerCount()
    {
        _playerCount -= 1;
    }

    [PunRPC]
    private void RPC_DecreaseAlive()
    {
        _currentAlive -= 1;
        UIManager.Instance.RefreshAlive(_currentAlive);

        if (_currentAlive == 1)
        {
            _gameEnd = true;
            PlayerManager.Instance.GameOver();
        }
    }

    private void SyncSeed()
    {
        _seed = Random.Range(1, 1000);
        _photonView.RPC("RPC_RandomizeState", RpcTarget.OthersBuffered, _seed);
    }

    [PunRPC]
    private void RPC_RandomizeState(int masterSeed)
    {
        _seed = masterSeed;

        Random.InitState(_seed);
    }

    private void StartGame()
    {
        _photonView.RPC("RPC_NotifyPhase", RpcTarget.AllBufferedViaServer, _currentPhase);
    }

    [PunRPC]
    private void RPC_NotifyPhase(int phase)
    {
        float time = ConstNums.PhaseTime[phase];
        UIManager.Instance.CreateGuide(phase, ConstNums.PhaseTime[phase]);
        MagnetTimer.Instance.Run(phase);
    }

    [PunRPC]
    private void RPC_CallDropItemByMaster(string name, int itemCount, Vector3 spawnPos, Vector2 spreadDir)
    {
        ObjectPoolManager.RaiseViewIDByOne();

        _photonView.RPC("RPC_DropItem", RpcTarget.AllBuffered, name, ObjectPoolManager.GetCurViewID(), itemCount, spawnPos, spreadDir);
    }

    [PunRPC]
    private void RPC_DropItem(string name, int viewID, int itemCount, Vector3 spawnPos, Vector2 spreadDir)
    {
        Item item = ObjectPoolManager.AllocItem<Item>(name, viewID, spawnPos);
        item.itemData.itemCount = itemCount;

        item.GetComponent<Rigidbody2D>().AddForce(spreadDir * 8f, ForceMode2D.Impulse);
    }

    [PunRPC]
    private void RPC_DisableItemByViewID(int viewID)
    {
        ObjectPoolManager.GetItemByViewID(viewID).SetActive(false);
    }

    #endregion
}