using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class CameraManagerInWorld : MonoBehaviour
{
    GameObject _mainCamera = null;
    GameObject _otherPlayer = null;
    TMP_Text _playerName = null;

    List<GameObject> _others = new List<GameObject>();

    private byte _aliveP = 0;
    private int _cursor = 0;

    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerName = GameObject.Find("PlayerNameTxt").GetComponent<TMP_Text>();

        CreateAliveList();
    }

    private void LateUpdate()
    {
        if (PlayerManager.Instance.IsDead() == false || NetworkManager.Instance.IsEnd() == true)
        {
            return;
        }
        else if (_aliveP > 0)
        {
            if (_otherPlayer == null || _otherPlayer.activeSelf == false)
            {
                FindNext();
            }
            else
            {
                _mainCamera.transform.position = new Vector3(_otherPlayer.transform.position.x, _otherPlayer.transform.position.y, -10);
            }
        }

        _aliveP = NetworkManager.Instance.GetAlive();
    }

    public void FindNext()
    {
        while (_cursor != -1)
        {
            _cursor = NextCursor();
            _otherPlayer = _others[_cursor];

            if (_otherPlayer == null || _otherPlayer.activeSelf == false)
            {
                continue;
            }
            else
            {
                if (_otherPlayer.CompareTag("Player"))
                {
                    UpdatePlayerName(_otherPlayer);
                }
                else if (_otherPlayer.CompareTag("AI"))
                {
                    UpdateAIName(_otherPlayer);
                }

                return;
            }
        }
    }

    public void FindPrev()
    {
        while (_cursor != -1)
        {
            _cursor = PrevCursor();
            _otherPlayer = _others[_cursor];

            if (_otherPlayer == null || _otherPlayer.activeSelf == false)
            {
                continue;
            }
            else
            {
                if (_otherPlayer.CompareTag("Player"))
                {
                    UpdatePlayerName(_otherPlayer);
                }
                else if (_otherPlayer.CompareTag("AI"))
                {
                    UpdateAIName(_otherPlayer);
                }

                return;
            }
        }
    }

    private void CreateAliveList()
    {
        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] otherAIs = GameObject.FindGameObjectsWithTag("AI");

        foreach (GameObject obj in otherPlayers)
        {
            _others.Add(obj);
        }

        foreach (GameObject obj in otherAIs)
        {
            _others.Add(obj);
        }
    }

    private int NextCursor()
    {
        int cursor = _cursor;
        int count = 0;

        while (count <= _others.Count)
        {
            cursor = (cursor + 1) % _others.Count;

            if (_others[cursor] != null)
            {
                return cursor;
            }

            count++;
        }

        return -1;
    }

    private int PrevCursor()
    {
        int cursor = _cursor;
        int count = 0;

        while (count <= _others.Count)
        {
            if (cursor - 1 < 0)
            {
                cursor = _others.Count - 1;
            }
            else
            {
                cursor = cursor - 1;
            }

            if (_others[cursor] != null)
            {
                return cursor;
            }

            count++;
        }

        return -1;
    }

    private void UpdatePlayerName(GameObject player)
    {
        string playerName = player.gameObject.GetPhotonView().Owner.NickName;
        _playerName.text = playerName;
    }

    private void UpdateAIName(GameObject ai)
    {
        string aiName = ai.GetComponent<AIAgent>()._AIname;
        _playerName.text = aiName;
    }
}
