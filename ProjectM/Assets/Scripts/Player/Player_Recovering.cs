using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    //해당 아이템이 사용 가능한지 체크하고 사용 함수 호출
    public void CheckUsingItem(int slotIndex)
    {
        if (_info.IsNullRecoverSlot(slotIndex) == true)
        {
            Debug.Log($"Don't have RecoverItem[{slotIndex}]");
            return;
        }

        if (CanUsingRecoveryItem(_info._recoverSlot[slotIndex]) == false)
        {
            Debug.Log($"Can't Using RecoverItem[{_info._recoverSlot[slotIndex]}]");
            return;
        }

        StartCoroutine("RecoverCooltime", slotIndex);
    }

    //아이템이 사용 가능한지 체크
    public bool CanUsingRecoveryItem(ItemRecovery recovery)
    {
        if (_state == State.RECOVERYING)
        {
            Debug.Log("State.RECOVERYING");
            return false;
        }

        if (recovery.itemCount > 0)
        {
            if (recovery.recoverType == RecoverType.BAND || recovery.recoverType == RecoverType.KIT)
            {
                if (_info.IsMaxHP() == false)
                {
                    return true;
                }
                Debug.Log("Your HP is full");
            }
            else if (recovery.recoverType == RecoverType.SODA || recovery.recoverType == RecoverType.DRUG)
            {
                return true;
            }
        }
        return false;
    }

    //사용 시작하고 x초 뒤에 회복 실행
    private IEnumerator RecoverCooltime(int slotIndex)
    {
        StopAction();
        _state = State.RECOVERYING;
        _info.HUD.SetActiveRecoveringView(true);
        yield return new WaitForSeconds(_info._recoverSlot[slotIndex].usingLatency);

        _info.UsingRecoveryItem(slotIndex);
        _state = State.LIVE;
    }
}
