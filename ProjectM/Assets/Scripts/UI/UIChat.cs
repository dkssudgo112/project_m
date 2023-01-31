using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIChat : MonoBehaviourPun
{
    private const int maxTextCount = 12;
    private static readonly Color whiteOnColor = new(255, 255, 255, 1);
    private static readonly Color whiteOffColor = new(255, 255, 255, 0);
    private static readonly Color blackTransColor = new(0, 0, 0, 0.5f);
    private static readonly Color blackOffColor = new(0, 0, 0, 0);

    public static bool _enabledChat = false;

    public GameObject _chatView = null;
    public GameObject _textView = null;
    public TMP_InputField _chatInput = null;
    public GameObject _chatPrefab = null;
    public Button _enterButton = null;

    public PhotonView _photonView = null;

    private void Awake()
    {
        BindChat();
        _chatInput.interactable = false;
        _chatInput.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            InteractEnterKey(_chatInput.interactable);
        }
    }

    private void BindChat()
    {
        _chatView = this.gameObject;
        _photonView = _chatView.GetPhotonView();
        _textView = _chatView.transform.Find("TextView").gameObject;
        _chatInput = _chatView.GetComponentInChildren<TMP_InputField>();
        _enterButton = _chatInput.GetComponentInChildren<Button>();
        _chatPrefab = Resources.Load<GameObject>("TextResources/ChatPrefab");
    }

    public void CreateChatPrefab(string msg)
    {
        UIManager._Instance.RebuildLayout(_textView.GetComponent<RectTransform>());

        GameObject chat = Instantiate(_chatPrefab);
        chat.transform.SetParent(_textView.transform);
        chat.transform.localScale = Vector3.one;
        chat.GetComponent<TMP_Text>().text = msg;
    }

    public void Send()
    {
        if (_chatInput.text == "")
        {
            return;
        }

        string name = UIManager._Instance.MakeBold(PlayerManager.GetName());
        string msg = $"{name} : {_chatInput.text}";
        _photonView.RPC("RPC_Send", RpcTarget.AllBuffered, msg);
        _chatInput.text = "";
    }

    [PunRPC]
    public void RPC_Send(string msg)
    {
        if (_textView.transform.childCount > ConstNums.maxTextCount)
        {
            Destroy(_textView.transform.GetChild(0).gameObject);
        }

        CreateChatPrefab(msg);
    }

    public void InteractEnterKey(bool enabled)
    {
        _enabledChat = !enabled;

        if (enabled == true)
        {
            _textView.GetComponent<Image>().color = blackOffColor;

            Send();
            _chatInput.interactable = !enabled;
            _chatInput.gameObject.SetActive(!enabled);

            CancelInvoke("OffChat");
            Invoke("OffChat", 3f);
        }
        else
        {
            CancelInvoke("OffChat");
            OnChat();
            _textView.GetComponent<Image>().color = blackTransColor;

            _chatInput.gameObject.SetActive(!enabled);
            _chatInput.interactable = !enabled;
            _chatInput.ActivateInputField();
        }
    }

    public void OnEnterClicked() => InteractEnterKey(_chatInput.interactable);

    public void OnChat()
    {
        foreach (TMP_Text line in _textView.GetComponentsInChildren<TMP_Text>())
        {
            line.color = whiteOnColor;
        }
    }

    public void OffChat()
    {
        foreach (TMP_Text line in _textView.GetComponentsInChildren<TMP_Text>())
        {
            line.color = whiteOffColor;
        }
    }
}
