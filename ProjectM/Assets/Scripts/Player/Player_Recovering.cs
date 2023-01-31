using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    //�ش� �������� ��� �������� üũ�ϰ� ��� �Լ� ȣ��
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

    //�������� ��� �������� üũ
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

    //��� �����ϰ� x�� �ڿ� ȸ�� ����
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
