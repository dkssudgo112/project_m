using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

public class LobbyNetwork : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private LobbyUI _lobbyUI = null;
    [SerializeField]
    private UIController _uiController = null;

    private string sceneName = "__MainScene";

    private byte maxPlayers = ConstNums.maxPlayers;
    private string _playerName = "";
    private string _roomName = "";
    private RoomOptions roomOptions = null;

    private Dictionary<string, RoomInfo> _cachedRoomList = null;
    private Dictionary<string, GameObject> _roomListEntries = null;
    private Dictionary<int, GameObject> _playerListEntries = null;

    private PhotonView _photonView = null;

    #region UNITY

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        _cachedRoomList = new Dictionary<string, RoomInfo>();
        _roomListEntries = new Dictionary<string, GameObject>();

        _photonView = this.gameObject.GetPhotonView();
    }

    private void Start()
    {
        _uiController.ActivatePanel(Panel.LOGIN);
        _uiController.ActivateInput(_lobbyUI._playerNameInput);
    }

    #endregion

    #region PUBLIC

    public void Connect()
    {
        // À¯´ÏÆ¼ ÇÑ±Û ±úÁü ¿¹¹æ
        _playerName = _lobbyUI._playerNameInput.text + " ";

        if (OutOfRangePlayerName() == true)
        {
            ResetNameInput();
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = _playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void SetOfflineMode() =>
        PhotonNetwork.OfflineMode = true;

    public void JoinLobby()
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

    public void Disconnect() =>
        PhotonNetwork.Disconnect();

    public void AssignDefaultRoomInfo()
    {
        _lobbyUI._roomNameInput.text = $"Room {UnityEngine.Random.Range(1000, 9999)}";
        _lobbyUI._maxPlayersInput.text = $"16";
    }

    public void CreateRoom()
    {
        if (OutOfRangeRoomName() == true)
        {
            _lobbyUI._roomNameInput.text = "";
            _lobbyUI._roomNameInput.placeholder.GetComponent<TMP_Text>().text = " Please Name(Length 1 ~ 20)";
        }
        else if (OutOfRangeMaxPlayers() == true)
        {
            _lobbyUI._maxPlayersInput.text = "";
            _lobbyUI._maxPlayersInput.placeholder.GetComponent<TMP_Text>().text = " Please Number(1 ~ 16)";
        }
        else
        {
            _roomName = _lobbyUI._roomNameInput.text;
            maxPlayers = Byte.Parse(_lobbyUI._maxPlayersInput.text);
            roomOptions = new RoomOptions { MaxPlayers = maxPlayers };

            PhotonNetwork.CreateRoom(_roomName, roomOptions);
            _uiController.ActivatePanel(Panel.INROOM);
        }
    }

    public void JoinRandomRoom() =>
        PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() =>
        PhotonNetwork.LeaveRoom();

    public void ClearChatList()
    {
        Transform[] chatList = _lobbyUI._textView.GetComponentsInChildren<Transform>();

        if (chatList != null)
        {
            for (int i = 1; i < chatList.Length; i++)
            {
                if (chatList[i] != null)
                {
                    Destroy(chatList[i].gameObject);
                }
            }
        }
    }

    public void SendMessage()
    {
        _uiController.ActivateInput(_lobbyUI._chatInput);

        if (OutOfRangeMessage() == true)
        {
            Debug.Log("Send Failed");
            return;
        }

        string name = PhotonNetwork.LocalPlayer.NickName;
        string msg = $"<b>{name}</b> : {_lobbyUI._chatInput.text}";
        _photonView.RPC("RPC_ReceiveMessage", RpcTarget.All, msg);
        _lobbyUI._chatInput.text = "";
    }

    [PunRPC]
    public void RPC_ReceiveMessage(string msg)
    {
        if (_lobbyUI._textView.transform.childCount > ConstNums.maxTextCount)
        {
            for (int i = 0; i < _lobbyUI._textView.transform.childCount - ConstNums.maxTextCount; i++)
            {
                Destroy(_lobbyUI._textView.transform.GetChild(i).gameObject);
            }
        }

        CreateChatEntry(msg);
    }

    public void LoadByMaster()
    {
        _uiController.ActivatePanel(Panel.LOADING);
        _photonView.RPC("RPC_Load", RpcTarget.OthersBuffered);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(sceneName);
    }

    #endregion

    #region EVENT

    public override void OnConnectedToMaster()
    {
        JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        _cachedRoomList.Clear();
        ClearRoomListView();
        _uiController.ActivatePanel(Panel.SELECTION);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnLeftLobby()
    {
        _cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            LoadByMaster();
        }
        else
        {
            _cachedRoomList.Clear();

            _uiController.ActivatePanel(Panel.INROOM);

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
                    string masterName = ChangeToMasterName(p.NickName);
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
            string masterName = ChangeToMasterName(newPlayer.NickName);
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
        _uiController.ActivatePanel(Panel.SELECTION);

        if (_playerListEntries.Count == 0)
            return;

        foreach (GameObject entry in _playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        _playerListEntries.Clear();
        _playerListEntries = null;
    }

    #endregion

    #region PRIVATE

    private void ResetNameInput()
    {
        _lobbyUI._playerNameInput.text = "";
        _lobbyUI._playerNameInput.placeholder.GetComponent<TMP_Text>().text = " Please Name(Length 1 ~ 20)";
        _uiController.ActivateInput(_lobbyUI._playerNameInput);
    }

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

    private string ChangeToMasterName(string name)
    {
        string options = $"<b><color=yellow>";
        string masterName = $"{name}     {options}Host";

        return masterName;
    }

    private void CreateChatEntry(string msg)
    {
        RebuildLayout(_lobbyUI._textView.GetComponent<RectTransform>());

        GameObject chat = Instantiate(_lobbyUI._chatEntryPrefab);
        chat.transform.SetParent(_lobbyUI._textView.transform);
        chat.transform.localScale = Vector3.one;
        chat.GetComponent<TMP_Text>().text = msg;
    }

    private void RebuildLayout(RectTransform obj)
    {
        StartCoroutine(CoRebuildLayout(obj));
    }

    private IEnumerator CoRebuildLayout(RectTransform obj)
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
    }

    [PunRPC]
    private void RPC_Load() =>
        _uiController.ActivatePanel(Panel.LOADING);

    #endregion

    #region EXCEPTION

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (PhotonNetwork.InLobby == true)
        {
            PhotonNetwork.LeaveLobby();
        }

        _uiController.ActivatePanel(Panel.LOGIN);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _uiController.ActivatePanel(Panel.SELECTION);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        _uiController.ActivatePanel(Panel.SELECTION);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = $"Room {UnityEngine.Random.Range(1000, 10000)}";
        RoomOptions options = new RoomOptions { MaxPlayers = ConstNums.maxPlayers };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            _lobbyUI._startButton.gameObject.SetActive(true);
        }
    }

    private bool OutOfRangePlayerName() =>
        (_playerName == " ") || (_playerName.Length > ConstNums.maxNameSize);

    private bool OutOfRangeRoomName() =>
        (_lobbyUI._roomNameInput.text == "") || (_lobbyUI._roomNameInput.text.Length > ConstNums.maxNameSize);

    private bool OutOfRangeMaxPlayers() =>
        (Byte.Parse(_lobbyUI._maxPlayersInput.text) <= 0)
        || (Byte.Parse(_lobbyUI._maxPlayersInput.text) > ConstNums.maxPlayers)
        || (Int32.TryParse(_lobbyUI._maxPlayersInput.text, out int result) == false);

    private bool OutOfRangeMessage() =>
        _lobbyUI._chatInput.text == "";
        //|| _chatInput.text.Length > ConstNums.maxTextLength;

    #endregion
}
