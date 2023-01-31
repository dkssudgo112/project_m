using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager Instance;

    private static PhotonView _photonView = null;

    private static int _playerID = 0;
    private static string _playerName = null;
    private static bool _isDead = false;
    private static int _killCount = 0;
    private static byte _rank = 0;

    public static bool _isMyPlayerDead = false;

    void Start()
    {
        Instance = this;

        _playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        _playerName = PhotonNetwork.LocalPlayer.NickName;
        _photonView = GetComponent<PhotonView>();

        Ending(!_isDead);
    }

    public static string GetName() => _playerName;

    public static void NotifyDeath(object[] enemyInfo)
    {
        if (_isDead == false)
        {
            _isDead = true;
            _rank = NetworkManager.GetAlive();
            _photonView.RPC("RPC_NotifyDeath", RpcTarget.AllBufferedViaServer, _playerName, enemyInfo);
            NetworkManager.DecreaseAlive();
            UIManager._Instance.CreateBottomKillLog(enemyInfo, _killCount, _isDead);

            OnDeath();
        }
    }

    public static void NotifyDeathAI(string AIName, object[] enemyInfo)
    {
        _photonView.RPC("RPC_NotifyDeathAI", RpcTarget.AllBufferedViaServer, AIName, enemyInfo);
        NetworkManager.DecreaseAlive();
    }

    public static void OnDeath()
    {
        if (NetworkManager.IsEnd())
        {
            return;
        }

        Ending(!_isDead);
    }

    public static bool IsDead() => _isDead;

    public static void CheckWinner() => Ending(!_isDead);

    public static void Ending(bool isWinner)
    {
        Cursor.visible = true;
        UIManager._Instance.ChangeActiveCursor(false);

        if (isWinner == true)
        {
            _rank = NetworkManager.GetAlive();
            UIManager._Instance.RefreshWinner();
        }
        else
        {
            UIManager._Instance.RefreshLoser();
        }

        UIManager._Instance.RefreshRanking(_rank, NetworkManager.GetMaxPlayers());
        UIManager._Instance.RefreshKills(_killCount);

        if (NetworkManager.GetAlive() == 1)
        {
            UIManager._Instance._watchingButton.SetActive(false);
        }

        Instance.PlayEnding();
    }

    public void PlayEnding()
    {
        float phase = 0;
        float startTime = 1f;
        float delay = 3f;
        GameObject obj;

        obj = UIManager._Instance._endingView;
        StartCoroutine(SetActiveObj(obj, true, startTime));

        while (phase < 5)
        {
            // 순차적으로 UI 활성화
            if (phase == 0)
            {
                obj = UIManager._Instance._endingTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager._Instance._leaveButton;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager._Instance._watchingButton;
                StartCoroutine(SetActiveObj(obj, !NetworkManager.IsEnd(), delay + phase));
            }
            else if (phase == 1)
            {
                obj = UIManager._Instance._myRankingTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));
            }
            else if (phase == 2)
            {
                obj = UIManager._Instance._ranking.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager._Instance._killCount.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));
            }
            else
            {
                obj = UIManager._Instance._rankingTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager._Instance._killCountTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));
            }

            phase++;
        }
    }

    private IEnumerator SetActiveObj(GameObject obj, bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(state);
    }

    #region PUN RPC

    // 킬로그 전체 알림
    [PunRPC]
    private void RPC_NotifyDeath(string playerName, object[] enemyInfo)
    {
        if (((string)enemyInfo[(int)InfoIdx.NAME]).Equals(_playerName))
        {
            object[] deadInfo = (object[])enemyInfo.Clone();
            deadInfo[(int)InfoIdx.NAME] = playerName;

            KillByYou(deadInfo);
        }
        UIManager._Instance.CreateTopKillLog(playerName, enemyInfo);
    }

    [PunRPC]
    private void RPC_NotifyDeathAI(string AIName, object[] enemyInfo)
    {
        if (((string)enemyInfo[(int)InfoIdx.NAME]).Equals(_playerName))
        {
            object[] deadInfo = (object[])enemyInfo.Clone();
            deadInfo[(int)InfoIdx.NAME] = AIName;

            KillByYou(deadInfo);
        }
        UIManager._Instance.CreateTopKillLog(AIName, enemyInfo);
    }

    // 하단 킬로그 킬한 사람에게 알림
    private void KillByYou(object[] deadInfo)
    {
        _killCount++;
        UIManager._Instance.CreateBottomKillLog(deadInfo, _killCount, _isDead);
    }

    #endregion
}
