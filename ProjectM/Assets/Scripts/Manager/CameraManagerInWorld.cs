using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManagerInWorld : MonoBehaviour
{
    GameObject _mainCamera = null;
    GameObject _otherPlayer = null;

    private bool _haveToChange = false;
    private byte _aliveP = 0;



    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {

        _aliveP = NetworkManager._currentAlive;

        if (_haveToChange == true && PlayerManager._isMyPlayerDead == true)
        {
            _otherPlayer = GameObject.FindGameObjectWithTag("Player");
            _haveToChange = false;

        }

        if(_otherPlayer == null && PlayerManager._isMyPlayerDead == true)
        {
            if (_aliveP > 0)
            {
                _haveToChange = true;
            }
        }
        else if(PlayerManager._isMyPlayerDead == true)
        {
            _mainCamera.transform.position = new Vector3(_otherPlayer.transform.position.x, _otherPlayer.transform.position.y, -10);
        }




        
    }
}
