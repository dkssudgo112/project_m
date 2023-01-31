using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;

public class PlayerListEntry : MonoBehaviour
{
    public TMP_Text _playerNameText = null;

    private int _ownerId = 0;

    public void SetPlayerListEntry(int playerId, string playerName)
    {
        _ownerId = playerId;
        _playerNameText.text = playerName;
    }

    public int GetOwnerId() => _ownerId;
}
