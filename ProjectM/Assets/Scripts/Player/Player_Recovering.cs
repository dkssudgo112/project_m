using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    //해당 아이템이 사용 가능한지 체크하고 사용 함수 호출
    public void CheckUsingItem(int slotIndex)
    {
        if (_info.IsNullRecoverySlot(slotIndex) == true)
        {
            Debug.Log($"Don't have RecoverItem[{slotIndex}]");
            return;
        }

        if (CanUsingRecoveryItem(_info._recoverySlot[slotIndex]) == false)
        {
            Debug.Log($"Can't Using RecoverItem[{_info._recoverySlot[slotIndex]}]");
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

        if (recovery.itemData.itemCount > 0)
        {
            if (recovery.itemData.recoverType == RecoverType.BAND || recovery.itemData.recoverType == RecoverType.KIT)
            {
                if (_info.IsMaxHP() == false)
                {
                    return true;
                }
                _info.HUD.CreateInformText(_info.GetCantRecoveryMessage());
            }
            else if (recovery.itemData.recoverType == RecoverType.SODA || recovery.itemData.recoverType == RecoverType.DRUG)
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
        _info.HUD.PlayActionViewOn("회복 중...", _info._recoverySlot[slotIndex].itemData.usingLatency);
        yield return new WaitForSeconds(_info._recoverySlot[slotIndex].itemData.usingLatency);

        _info.UseRecoveryItem(slotIndex);
        _state = State.LIVE;
    }
}
