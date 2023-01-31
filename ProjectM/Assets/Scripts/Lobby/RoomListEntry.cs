using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class RoomListEntry : MonoBehaviour
{
    public TMP_Text _roomNameText = null;
    public TMP_Text _roomPlayersText = null;
    public Button _joinRoomButton = null;

    private string _roomName = "";

    public void Start()
    {
        _joinRoomButton.onClick.AddListener(() =>
        {
            PhotonNetwork.JoinRoom(_roomName);
        });
    }

    public void SetRoomListEntry(string name, byte currentPlayers, byte maxPlayers)
    {
        _roomName = name;
        _roomNameText.text = name;
        _roomPlayersText.text = $"{currentPlayers} / {maxPlayers}";
    }
}