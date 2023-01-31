using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public GameObject _loading = null;
    public GameObject _lobbyPanel = null;

    [SerializeField]
    private Image _pixelLogo = null;
    [SerializeField]
    private Image _teamLogo = null;

    private void Start()
    {
        PlayPixelLogo();
    }

    private void PlayPixelLogo()
    {
        _pixelLogo.gameObject.SetActive(true);
        Invoke("PlayTeamLogo", 2.7f);
    }

    private void PlayTeamLogo()
    {
        _teamLogo.gameObject.SetActive(true);
        Invoke("Finish", 2.7f);
    }

    private void Finish()
    {
        Destroy(this.gameObject);
    }
}
