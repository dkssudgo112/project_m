using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance = null;

    #region UI PARAM

    [Header("Login Panel")]
    public GameObject _loginPanel = null;

    public TMP_InputField _playerNameInput = null;

    [Header("Selection Panel")]
    public GameObject _selectionPanel = null;

    public GameObject _roomListScrollView = null;
    public GameObject _roomListContent = null;
    public GameObject _roomListEntryPrefab = null;

    [Header("Create Room Panel")]
    public GameObject _createRoomPanel = null;

    public TMP_InputField _roomNameInput = null;
    public TMP_InputField _maxPlayersInput = null;

    [Header("InRoom Panel")]
    public GameObject _inRoomPanel = null;

    public GameObject _playerListPanel = null;
    public GameObject _LeftPanel = null;
    public GameObject _RightPanel = null;
    public GameObject _playerListEntryPrefab = null;
    public GameObject _chatPanel = null;
    public GameObject _textView = null;
    public TMP_InputField _chatInput = null;
    public GameObject _chatEntryPrefab = null;
    public GameObject _statePanel = null;
    public Image _mapImage = null;
    public Button _startButton = null;

    public AudioSource _audioSource = null;

    [Header("Loading Panel")]
    public GameObject _loadingPanel = null;

    private bool _notiOn = false;
    public GameObject _notiImage = null;

    #endregion

    #region UNITY

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        SetActivePanel(_loginPanel.name);
        _audioSource = GetComponent<AudioSource>();
        _audioSource.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (_notiOn == true)
            {
                _notiImage.SetActive(false);
                _notiOn = false;
            }
            else
            {
                _notiImage.SetActive(true);
                _notiOn = true;
            }
        }
    }

    #endregion

    #region UI CALLBACKS

    public void OnLoginButtonClicked() => LobbyNetwork.OnConnect();

    public void OnOfflineButtonClicked() => LobbyNetwork.SetOfflineMode();

    public void OnCreateRoomButtonClicked() => SetActivePanel(_createRoomPanel.name);

    public void OnJoinRandomRoomButtonClicked() => LobbyNetwork.JoinRoom();

    public void OnBackButtonClicked() => LobbyNetwork.Disconnect();

    public void OnCreateButtonClicked() => LobbyNetwork.OnCreateRoom();

    public void OnCancelButtonClicked() => SetActivePanel(_selectionPanel.name);

    public void OnStartButtonClicked() => LobbyNetwork.OnLoad();

    public void OnLeaveButtonClicked() => LobbyNetwork.LeaveRoom();

    public void OnEnterClicked() => LobbyNetwork.SendInRoom();

    #endregion

    #region METHOD

    public void SetActivePanel(string activePanel)
    {
        _loginPanel.SetActive(activePanel.Equals(_loginPanel.name));
        _selectionPanel.SetActive(activePanel.Equals(_selectionPanel.name));
        _createRoomPanel.SetActive(activePanel.Equals(_createRoomPanel.name));
        _inRoomPanel.SetActive(activePanel.Equals(_inRoomPanel.name));
        _loadingPanel.SetActive(activePanel.Equals(_loadingPanel.name));

        CheckOnJoinInRoom(activePanel.Equals(_inRoomPanel.name));
    }

    private void CheckOnJoinInRoom(bool inRoom)
    {
        if (inRoom == true)
        {
            _chatInput.interactable = true;
            _chatInput.ActivateInputField();
        }
    }

    public void RebuildLayout(RectTransform obj)
    {
        StartCoroutine(CoRebuildLayout(obj));
    }

    public IEnumerator CoRebuildLayout(RectTransform obj)
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
    }

    #endregion
}
