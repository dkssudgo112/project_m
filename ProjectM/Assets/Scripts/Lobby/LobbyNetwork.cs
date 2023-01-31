using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

public class LobbyNetwork : MonoBehaviourPunCallbacks
{
    #region LOBBY PARAM

    public static string s_playerName = "";

    public static string s_roomName = "";
    public static byte s_maxPlayers = ConstNums.maxPlayers;
    public static RoomOptions s_roomOptions = null;
    private static string s_sceneName = "__MainScene";

    private Dictionary<string, RoomInfo> _cachedRoomList = null;
    private Dictionary<string, GameObject> _roomListEntries = null;
    private Dictionary<int, GameObject> _playerListEntries = null;

    private static PhotonView _photonView = null;
    private static LobbyUI _lobbyUI = null;

    #endregion

    #region UNITY
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        _cachedRoomList = new Dictionary<string, RoomInfo>();
        _roomListEntries = new Dictionary<string, GameObject>();

        _photonView = this.gameObject.GetPhotonView();
        _lobbyUI = LobbyUI.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (_lobbyUI._loginPanel.activeSelf == true)
            {
                OnConnect();
            }

            if (_lobbyUI._inRoomPanel.activeSelf == true)
            {
                SendInRoom();
                _lobbyUI._chatInput.ActivateInputField();
            }
        }
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            PhotonNetwork.JoinRandomOrCreateRoom();
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (PhotonNetwork.InLobby == true)
        {
            PhotonNetwork.LeaveLobby();
        }

        _lobbyUI.SetActivePanel(_lobbyUI._loginPanel.name);
    }

    public override void OnJoinedLobby()
    {
        _cachedRoomList.Clear();
        ClearRoomListView();
        _lobbyUI.SetActivePanel(_lobbyUI._selectionPanel.name);
    }

    public override void OnLeftLobby()
    {
        _cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _lobbyUI.SetActivePanel(_lobbyUI._selectionPanel.name);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        _lobbyUI.SetActivePanel(_lobbyUI._selectionPanel.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = $"Room {UnityEngine.Random.Range(1000, 10000)}";
        RoomOptions options = new RoomOptions { MaxPlayers = ConstNums.maxPlayers };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            OnLoad();
        }
        else
        {
            _cachedRoomList.Clear();

            _lobbyUI.SetActivePanel(_lobbyUI._inRoomPanel.name);

            if (_playerListEntries == null)
            {
                _playerListEntries = new Dictionary<int, GameObject>();
            }

            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(_lobbyUI._playerListEntryPrefab);

                if (_lobbyUI._LeftPanel.transform.childCount <= (ConstNums.maxPlayers / 2) - 1)
                {
                    entry.transform.SetParent(_lobbyUI._LeftPanel.transform);
                }
                else
                {
                    entry.transform.SetParent(_lobbyUI._RightPanel.transform);
                }
                entry.transform.localScale = Vector3.one;

                if (p.IsMasterClient == true)
                {
                    string options = $"<b><color=yellow>";
                    string masterName = $"{p.NickName}     {options}Host";
                    entry.GetComponent<PlayerListEntry>().SetPlayerListEntry(p.ActorNumber, masterName);
                }
                else
                {
                    entry.GetComponent<PlayerListEntry>().SetPlayerListEntry(p.ActorNumber, p.NickName);
                }

                _playerListEntries.Add(p.ActorNumber, entry);
            }

            if (PhotonNetwork.IsMasterClient == true)
            {
                _lobbyUI._startButton.gameObject.SetActive(true);
            }
            else
            {
                _lobbyUI._startButton.gameObject.SetActive(false);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameObject entry = Instantiate(_lobbyUI._playerListEntryPrefab);

        if (_lobbyUI._LeftPanel.transform.childCount <= (ConstNums.maxPlayers / 2) - 1)
        {
            entry.transform.SetParent(_lobbyUI._LeftPanel.transform);
        }
        else
        {
            entry.transform.SetParent(_lobbyUI._RightPanel.transform);
        }
        entry.transform.localScale = Vector3.one;

        if (newPlayer.IsMasterClient == true)
        {
            string options = $"<b><color=yellow>";
            string masterName = $"{newPlayer.NickName}     {options}Host";
            entry.GetComponent<PlayerListEntry>().SetPlayerListEntry(newPlayer.ActorNumber, masterName);
        }
        else
        {
            entry.GetComponent<PlayerListEntry>().SetPlayerListEntry(newPlayer.ActorNumber, newPlayer.NickName);
        }

        _playerListEntries.Add(newPlayer.ActorNumber, entry);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Destroy(_playerListEntries[otherPlayer.ActorNumber].gameObject);
        _playerListEntries.Remove(otherPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {
        _lobbyUI.SetActivePanel(_lobbyUI._selectionPanel.name);

        if (_playerListEntries.Count == 0)
            return;

        foreach (GameObject entry in _playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        _playerListEntries.Clear();
        _playerListEntries = null;
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            _lobbyUI._startButton.gameObject.SetActive(true);
        }
    }

    #endregion

    #region STATIC METHOD

    public static void SetOfflineMode() => PhotonNetwork.OfflineMode = true;

    public static void JoinRoom() => PhotonNetwork.JoinRandomRoom();

    public static void Disconnect() => PhotonNetwork.Disconnect();

    public static void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public static void OnConnect()
    {
        s_playerName = _lobbyUI._playerNameInput.text + " ";

        if ((s_playerName == " ") || (s_playerName.Length > ConstNums.maxNameSize))
        {
            _lobbyUI._playerNameInput.text = "";
            _lobbyUI._playerNameInput.placeholder.GetComponent<TMP_Text>().text = " Please Name(Length 1 ~ 20)";
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = s_playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    public static void OnCreateRoom()
    {
        if ((_lobbyUI._roomNameInput.text == "") || (_lobbyUI._roomNameInput.text.Length > ConstNums.maxNameSize))
        {
            _lobbyUI._roomNameInput.text = "";
            _lobbyUI._roomNameInput.placeholder.GetComponent<TMP_Text>().text = " Please Name(Length 1 ~ 20)";
        }
        else if ((Byte.Parse(_lobbyUI._maxPlayersInput.text) <= 0)
                    || (Byte.Parse(_lobbyUI._maxPlayersInput.text) > ConstNums.maxPlayers)
                    || (Int32.TryParse(_lobbyUI._maxPlayersInput.text, out int result) == false))
        {
            _lobbyUI._maxPlayersInput.text = "";
            _lobbyUI._maxPlayersInput.placeholder.GetComponent<TMP_Text>().text = " Please Number(1 ~ 16)";
        }
        else
        {
            s_roomName = _lobbyUI._roomNameInput.text;
            s_maxPlayers = Byte.Parse(_lobbyUI._maxPlayersInput.text);
            s_roomOptions = new RoomOptions { MaxPlayers = s_maxPlayers };

            PhotonNetwork.CreateRoom(s_roomName, s_roomOptions);
            _lobbyUI.SetActivePanel(_lobbyUI._inRoomPanel.name);
        }
    }

    public static void OnLoad()
    {
        _lobbyUI.SetActivePanel(_lobbyUI._loadingPanel.name);
        _photonView.RPC("OnLoadingPanel", RpcTarget.OthersBuffered);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(s_sceneName);
    }

    #endregion

    #region METHOD

    private void ClearRoomListView()
    {
        foreach (GameObject entry in _roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        _roomListEntries.Clear();
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in _cachedRoomList.Values)
        {
            GameObject entry = Instantiate(_lobbyUI._roomListEntryPrefab);
            entry.transform.SetParent(_lobbyUI._roomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().SetRoomListEntry(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            _roomListEntries.Add(info.Name, entry);
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (_cachedRoomList.ContainsKey(info.Name))
                {
                    _cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            if (_cachedRoomList.ContainsKey(info.Name))
            {
                _cachedRoomList[info.Name] = info;
            }
            else
            {
                _cachedRoomList.Add(info.Name, info);
            }
        }
    }

    public static void SendInRoom()
    {
        if (_lobbyUI._chatInput.text == "")
        {
            Debug.Log("Send Failed");
            return;
        }

        Debug.Log("Send Suc");
        string name = PhotonNetwork.LocalPlayer.NickName;
        string msg = $"{name} : {_lobbyUI._chatInput.text}";
        _photonView.RPC("RPC_SendInRoom", RpcTarget.All, msg);
        _lobbyUI._chatInput.text = "";
    }

    [PunRPC]
    public void RPC_SendInRoom(string msg)
    {
        Debug.Log("RPC Send");
        if (_lobbyUI._textView.transform.childCount > ConstNums.maxTextCount)
        {
            Destroy(_lobbyUI._textView.transform.GetChild(0).gameObject);
        }

        CreateChatEntry(msg);
    }

    public void CreateChatEntry(string msg)
    {
        _lobbyUI.RebuildLayout(_lobbyUI._textView.GetComponent<RectTransform>());

        GameObject chat = Instantiate(_lobbyUI._chatEntryPrefab);
        chat.transform.SetParent(_lobbyUI._textView.transform);
        chat.transform.localScale = Vector3.one;
        chat.GetComponent<TMP_Text>().text = msg;
    }

    [PunRPC]
    private void OnLoadingPanel() => _lobbyUI.SetActivePanel(_lobbyUI._loadingPanel.name);

    #endregion
}
