using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MagnetTimer : MonoBehaviourPun
{
    private static MagnetTimer Instance;

    private int _phaseTime = 0;
    private int _outputTime = 0;

    private int _currTime = 0;
    private int _prevTime = 0;
    private int _timeDiff = 0;

    private bool isRun = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public static void Run(int phase)
    {
        int time = (int)ConstNums.PhaseTime[phase];
        Instance._phaseTime = time * 1000;
        Instance._prevTime = 0;
        Instance._timeDiff = 0;

        Instance.isRun = true;
        Instance.StartCoroutine("CoTimeFlow");
    }

    private void OnFinish()
    {
        NetworkManager.NextPhase();
    }

    public static void Stop()
    {
        UIManager._Instance.RefreshTime($"00 : 00");
    }

    private IEnumerator CoTimeFlow()
    {
        while (isRun == true)
        {
            yield return null;

            UpdateHUDTime(_outputTime);

            _currTime = NetworkManager.GetTime();

            if (_prevTime == 0)
            {
                _prevTime = _currTime;
            }

            _timeDiff = _currTime - _prevTime;

            _phaseTime = _phaseTime - _timeDiff;
            _outputTime = _phaseTime / 1000;

            if (_phaseTime <= 0)
            {
                _phaseTime = 0;
                _outputTime = 0;

                isRun = false;
            }

            _prevTime = _currTime;
        }

        UIManager._Instance.RefreshTime($"00 : 00");
        OnFinish();
    }

    private void UpdateHUDTime(int outputTime)
    {
        if (_outputTime >= 10)
        {
            UIManager._Instance.RefreshTime($"00 : {_outputTime}");
        }
        else if (_outputTime > 0)
        {
            UIManager._Instance.RefreshTime($"00 : 0{_outputTime}");
        }
        else
        {
            UIManager._Instance.RefreshTime($"00 : 00");
        }
    }
}
