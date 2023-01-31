using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private byte aiCount = 16;

    public static NetworkManager Instance;

    private static PhotonView _photonView = null;

    [Header("Game Info")]
    public static bool _gameEnd = false;

    [Header("Network Info")]
    [Header("Player Param")]
    public byte _prevPlayerCount = 0;
    public static int _maxPlayers = 0;
    public static byte _currentAlive = 0;

    [Header("Time Param")]
    public int _networkTime = 0;
    //public double _delay = 0;

    [Header("Random Param")]
    public static float _seed = 0;

    [Header("Magnetic Param")]
    public static int _currentPhase = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log($"Failed to instantiate NetworkManager.");
        }

        _photonView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient == true)
        {
            MakeSeed();
            _maxPlayers = PhotonNetwork.CurrentRoom.PlayerCount + aiCount;
            int aliveCount = _maxPlayers;
            SetAlive((byte)aliveCount);
        }
        else
        {
            CheckSeed();
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        StartGame();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _networkTime = PhotonNetwork.ServerTimestamp;
        }
    }

    #region IPunObservable
    
    // send : 네트워크 시간
    // send : 현재 플레이어 수
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(this._networkTime);
        }
        else if (stream.IsReading && !PhotonNetwork.IsMasterClient)
        {
            _networkTime = (int)stream.ReceiveNext();

            // delay = 0.001
            //_delay = Mathf.Abs((float)(_networkTime - info.SentServerTime));
        }
    }

    #endregion

    #region METHOD

    public void MakeSeed()
    {
        _seed = UnityEngine.Random.Range(1f, 1000f);
        _photonView.RPC("RPC_RespondSeed", RpcTarget.OthersBuffered, _seed);
    }

    public void CheckSeed()
    {
        if (_seed == 0)
        {
            RequestSeed();
            Debug.Log("Request Seed by NetworkManager");
        }
    }

    public static float RequestSeed()
    {
        if (_seed == 0)
        {
            _photonView.RPC("RPC_RequestSeed", RpcTarget.MasterClient);
        }

        return _seed;
    }

    // phase 0   : Wait Time      : 7
    // phase 1   : FARM.ONE       : 40    레드존 전진 1분 20초
    // phase 2   : DECREASE.ONE   : 40    레드존 접근 중! 세이프 존으로 이동
    // phase 3   : FARM.TWO       : 20
    // phase 4   : DECREASE.TWO   : 20
    // phase 5   : FARM.THREE     : 10
    // phase 6   : DECREASE.THREE : 10
    // phase 7   : FARM.FOUR      : 10
    // phase 8   : DECREASE.FOUR  : 10
    // phase 9   : FARM.FIVE      : 5
    // phase 10  : DECREASE.FIVE  : 5

    // 인게임 접속 후 대기 시간
    public static void StartGame()
    {
        _photonView.RPC("RPC_NotifyPhase", RpcTarget.AllBufferedViaServer, _currentPhase);
    }

    public static void NextPhase()
    {
        _currentPhase++;

        if (_currentPhase <= ConstNums.lastPhase)
        {
            UIManager._Instance.CreateGuide(_currentPhase, ConstNums.PhaseTime[_currentPhase]);
            MagnetTimer.Run(_currentPhase);
            MagnetActivator.MoveToNextPhaze(_currentPhase);
        }
        else
        {
            Debug.Log("Reach the Last Phase");
        }
        
    }

    public static int GetTime()
    {
        return Instance._networkTime;
    }

    public static void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetAlive(byte aliveCount)
    {
        _photonView.RPC("RPC_UpdateMaxPlayer", RpcTarget.AllBufferedViaServer, aliveCount);
        _photonView.RPC("RPC_UpdateAlive", RpcTarget.AllBufferedViaServer, aliveCount);
    }

    public static byte GetAlive() => _currentAlive;

    public static int GetMaxPlayers() => _maxPlayers;

    public static bool IsEnd() => _gameEnd;

    public static void DecreaseAlive()
    {
        if (_gameEnd == true)
        {
            return;
        }

        _currentAlive -= 1;
        byte newAliveCount = _currentAlive;
        _photonView.RPC("RPC_UpdateAlive", RpcTarget.AllBufferedViaServer, newAliveCount);
    }

    public static void DropItem(string name, Vector3 spawnPos, Vector2 startVector)
    {
        _photonView.RPC("RPC_DropItem", RpcTarget.AllBuffered, name, ItemManager.GetItemByName(name).itemCount, spawnPos, startVector);
    }

    public static void DropItem(string name, int itemCount, Vector3 spawnPos, Vector2 startVector)
    {
        _photonView.RPC("RPC_DropItem", RpcTarget.AllBuffered, name, itemCount, spawnPos, startVector);
    }

    #endregion

    #region PUN RPC

    [PunRPC]
    public void RPC_RequestSeed()
    {
        _photonView.RPC("RPC_RespondSeed", _photonView.Owner, _seed);
    }

    [PunRPC]
    public void RPC_RespondSeed(float masterSeed)
    {
        _seed = masterSeed;
    }

    [PunRPC]
    public void RPC_UpdateAlive(byte aliveCount)
    {
        _currentAlive = aliveCount;
        UIManager._Instance.RefreshAlive(_currentAlive);

        // 승리 알림
        if (_currentAlive == 1)
        {
            _gameEnd = true;
            PlayerManager.CheckWinner();
        }
    }

    [PunRPC]
    public void RPC_UpdateMaxPlayer(byte maxPlayers)
    {
        _maxPlayers = maxPlayers;
    }

    [PunRPC]
    public void RPC_NotifyPhase(int phase)
    {
        float time = ConstNums.PhaseTime[phase];
        UIManager._Instance.CreateGuide(phase, ConstNums.PhaseTime[phase]);
        MagnetTimer.Run(phase);
    }

    [PunRPC]
    public void RPC_DropItem(string name, int itemCount, Vector3 spawnPos, Vector2 spreadDir)
    {
        Item item = ObjectPoolManager.AllocItem<Item>(name, spawnPos);
        item.itemCount = itemCount;

        item.GetComponent<Rigidbody2D>().AddForce(spreadDir * 8f, ForceMode2D.Impulse);
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

    #endregion
}