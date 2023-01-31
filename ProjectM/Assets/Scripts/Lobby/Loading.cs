using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public GameObject _loading = null;
    public GameObject _lobbyPanel = null;

    private void Start()
    {
        Invoke("StartLobby", 3.2f);
    }

    private void StartLobby()
    {
        Destroy(this.gameObject);
    }
}
