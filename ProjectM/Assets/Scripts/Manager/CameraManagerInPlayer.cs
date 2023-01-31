using UnityEngine;
using Photon.Pun;

public class CameraManagerInPlayer : MonoBehaviourPun
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
        if (photonView.IsMine == true &&  PlayerManager.Instance.IsDead() == false)
        {
            _mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
    }
}
