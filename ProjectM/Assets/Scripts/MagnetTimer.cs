using System.Collections;
using UnityEngine;

public class MagnetTimer : MonoBehaviour
{
    public static MagnetTimer Instance;

    private int _phaseTime = 0;
    private int _outputTime = 0;

    private int _currTime = 0;
    private int _prevTime = 0;
    private int _timeDiff = 0;

    private bool _isRun = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Run(int phase)
    {
        int time = (int)ConstNums.PhaseTime[phase];
        _phaseTime = time * 1000;
        _prevTime = 0;
        _timeDiff = 0;

        _isRun = true;
        StartCoroutine("CoTimeFlow");
    }

    public void Stop()
    {
        UpdateHUDTime(0);
    }

    private void OnFinish()
    {
        NetworkManager.Instance.NextPhase();
    }

    private IEnumerator CoTimeFlow()
    {
        while (_isRun == true)
        {
            yield return null;

            UpdateHUDTime(_outputTime);

            _currTime = NetworkManager.Instance.GetTime();

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

                _isRun = false;
            }

            _prevTime = _currTime;
        }

        Stop();
        OnFinish();
    }

    private void UpdateHUDTime(int outputTime)
    {
        UIManager.Instance.RefreshTime(string.Format("{0:00 : ##}", _outputTime));
    }
}
