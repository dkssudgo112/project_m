using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private LobbyUI _lobbyUI = null;
    [SerializeField]
    private LobbyNetwork _lobbyNetwork = null;

    #region UNITY

    private void Awake()
    {
        _lobbyUI._audioSource = GetComponent<AudioSource>();
        _lobbyUI._audioSource.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleNoti(_lobbyUI._notiImage);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (IsActivedPanel(Panel.LOGIN) == true)
            {
                _lobbyNetwork.Connect();
            }
            else if (IsActivedPanel(Panel.CREATEROOM) == true)
            {
                _lobbyNetwork.CreateRoom();
            }
            else if (IsActivedPanel(Panel.INROOM) == true)
            {
                _lobbyNetwork.SendMessage();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {

        }
    }

    #endregion

    public void ActivateInput(TMP_InputField inputField)
    {
        inputField.interactable = true;
        inputField.ActivateInputField();
    }

    public void ActivatePanel(Panel panel)
    {
        int panelIdx = (int)panel;

        for (int i = 0; i < ConstNums.maxPanelCount; i++)
        {
            if (i == panelIdx)
            {
                _lobbyUI._panels[i].SetActive(true);
            }
            else
            {
                _lobbyUI._panels[i].SetActive(false);
            }
        }

        ActiveCallback(panel);
    }

    private void ActiveCallback(Panel panel)
    {
        switch (panel)
        {
            case Panel.LOGIN:
                {
                    break;
                }
            case Panel.SELECTION:
                {
                    break;
                }
            case Panel.CREATEROOM:
                {
                    _lobbyNetwork.AssignDefaultRoomInfo();
                    break;
                }
            case Panel.INROOM:
                {
                    _lobbyNetwork.ClearChatList();
                    ActivateInput(_lobbyUI._chatInput);
                    break;
                }
            case Panel.LOADING:
                {
                    break;
                }
            default:
                break;
        }
    }

    private void ToggleNoti(GameObject noti) =>
        noti.SetActive(!noti.activeSelf);

    private bool IsActivedPanel(Panel panel) =>
        _lobbyUI._panels[(int)panel].activeSelf;
}