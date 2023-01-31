using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraManagerInPlayer : MonoBehaviourPunCallbacks
{
    GameObject _mainCamera = null;

    

    void Start()
    {
        if (photonView.IsMine == true)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            
        }
    }

    void Update()
    {
        if (photonView.IsMine == true &&  PlayerManager._isMyPlayerDead == false)
        {
            _mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
    }
}
