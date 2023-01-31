using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerInput : MonoBehaviourPun
{
    public bool acquire { get; private set; }
    public bool attack { get; private set; }
    public bool reload { get; private set; }
    public Vector3 mouseVec { get; private set; }
    public Vector3 move { get; private set; }
    public bool[] slots = new bool[10] { false,false,false,false,false,false,false,false,false,false };    

    private void Update()
    {
        if(photonView.IsMine == false || UIChat._enabledChat == true)
        {
            return;
        }

        reload = Input.GetKeyDown(KeyCode.R);
        attack = Input.GetMouseButton(0);
        acquire = Input.GetKeyDown(KeyCode.F);

        slots[0] = Input.GetKeyDown(KeyCode.Alpha1);
        slots[1] = Input.GetKeyDown(KeyCode.Alpha2);
        slots[2] = Input.GetKeyDown(KeyCode.Alpha3);
        slots[3] = Input.GetKeyDown(KeyCode.Alpha4);
        slots[6] = Input.GetKeyDown(KeyCode.Alpha7);
        slots[7] = Input.GetKeyDown(KeyCode.Alpha8);
        slots[8] = Input.GetKeyDown(KeyCode.Alpha9);
        slots[9] = Input.GetKeyDown(KeyCode.Alpha0);

        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseVec = new Vector3(mousePos.x, mousePos.y, 0);
        move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
    }
}
