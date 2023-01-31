using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    private LobbyNetwork _lobbyNetwork = null;
    [SerializeField]
    private UIController _uiController = null;

    #region UI OBJECT

    public GameObject[] _panels = new GameObject[ConstNums.maxPanelCount];
    public AudioSource _audioSource = null;

    [Header("Login Panel")]
    public TMP_InputField _playerNameInput = null;

    [Header("Selection Panel")]
    public GameObject _roomListContent = null;

    [Header("Create Room Panel")]
    public TMP_InputField _roomNameInput = null;
    public TMP_InputField _maxPlayersInput = null;

    [Header("InRoom Panel")]
    public GameObject _LeftPanel = null;
    public GameObject _RightPanel = null;
    public GameObject _textView = null;
    public TMP_InputField _chatInput = null;
    public Button _startButton = null;
    public GameObject _notiImage = null;

    [Header("Entry")]
    public GameObject _roomListEntryPrefab = null;
    public GameObject _playerListEntryPrefab = null;
    public GameObject _chatEntryPrefab = null;

    #endregion

    #region BUTTON CALLBACKS

    public void OnLoginButtonClicked() =>
        _lobbyNetwork.Connect();

    public void OnOfflineButtonClicked() =>
        _lobbyNetwork.SetOfflineMode();

    public void OnCreateRoomButtonClicked() =>
        _uiController.ActivatePanel(Panel.CREATEROOM);

    public void OnJoinRandomRoomButtonClicked() =>
        _lobbyNetwork.JoinRandomRoom();

    public void OnBackButtonClicked() =>
        _lobbyNetwork.Disconnect();

    public void OnCreateButtonClicked() =>
        _lobbyNetwork.CreateRoom();

    public void OnCancelButtonClicked() =>
        _uiController.ActivatePanel(Panel.SELECTION);

    public void OnStartButtonClicked() =>
        _lobbyNetwork.LoadByMaster();

    public void OnLeaveButtonClicked() =>
        _lobbyNetwork.LeaveRoom();

    public void OnEnterButtonClicked() =>
        _lobbyNetwork.SendMessage();

    #endregion
}
