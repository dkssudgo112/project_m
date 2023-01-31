using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPun
{
    public static PlayerManager Instance;

    public GameObject _otherCamera = null;
    private PhotonView _photonView = null;

    private bool _isDead = false;
    private byte _rank = 0;
    private int _killCount = 0;
    private int _playerID = 0;
    private string _playerName = "";

    #region UNITY

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log($"Fail : instantiate PlayerManager.");
        }

        _photonView = GetComponent<PhotonView>();
        _otherCamera = Resources.Load<GameObject>("CameraManager");
    }

    void Start()
    {
        _playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        _playerName = PhotonNetwork.LocalPlayer.NickName;   
    }

    #endregion

    #region PUBLIC

    public void SetDead(bool isDead) =>
        _isDead = isDead;

    public bool IsDead() =>
        _isDead;

    public int GetID() =>
        _playerID;

    public string GetName() =>
        _playerName;

    // 킬로그 본인 알림
    public void NotifyDeath(object[] enemyInfo)
    {
        if (_isDead == false)
        {
            _isDead = true;
            _rank = NetworkManager.Instance.GetAlive();
            _photonView.RPC("RPC_NotifyDeath", RpcTarget.AllBufferedViaServer, _playerName, enemyInfo);
            NetworkManager.Instance.DecreaseAlive(true);
            UIManager.Instance.CreateBottomKillLog(enemyInfo, _killCount, _isDead);

            OnDeath();
        }
    }

    // 킬로그 전체 알림
    [PunRPC]
    private void RPC_NotifyDeath(string playerName, object[] enemyInfo)
    {
        int enemyID = (int)enemyInfo[(int)InfoIdx.ID];
        if (enemyID == _playerID)
        {
            object[] deadInfo = (object[])enemyInfo.Clone();
            deadInfo[(int)InfoIdx.NAME] = playerName;

            NotifyKilling(deadInfo);
        }
        UIManager.Instance.CreateTopKillLog(playerName, enemyInfo);
    }

    public void NotifyDeathAI(string AIName, object[] enemyInfo)
    {
        _photonView.RPC("RPC_NotifyDeathAI", RpcTarget.AllBufferedViaServer, AIName, enemyInfo);
        NetworkManager.Instance.DecreaseAlive(false);
    }

    [PunRPC]
    private void RPC_NotifyDeathAI(string AIName, object[] enemyInfo)
    {
        int enemyID = (int)enemyInfo[(int)InfoIdx.ID];
        if (enemyID == _playerID)
        {
            object[] deadInfo = (object[])enemyInfo.Clone();
            deadInfo[(int)InfoIdx.NAME] = AIName;

            NotifyKilling(deadInfo);
        }
        UIManager.Instance.CreateTopKillLog(AIName, enemyInfo);
    }

    // 킬러에게 알림
    private void NotifyKilling(object[] deadInfo)
    {
        _killCount++;
        UIManager.Instance.CreateBottomKillLog(deadInfo, _killCount, _isDead);
    }

    public void GameOver() =>
        OnEnd(!_isDead);

    public void MoveCameraToOthers()
    {
        _otherCamera = Instantiate(_otherCamera);
    }

    #endregion

    #region EVENT

    private void OnDeath()
    {
        if (NetworkManager.Instance.IsEnd())
        {
            return;
        }

        UIManager.Instance.PlayerInfoViewOnOff(false);

        GameOver();
    }

    private void OnEnd(bool isWinner)
    {
        UIManager.Instance.ChangeActiveCursor(false);

        if (isWinner == true)
        {
            _rank = NetworkManager.Instance.GetAlive();
            UIManager.Instance.RefreshWinner();
        }
        else
        {
            UIManager.Instance.RefreshLoser();
        }

        UIManager.Instance.RefreshRanking(_rank, NetworkManager.Instance.GetMaxPlayers());
        UIManager.Instance.RefreshKills(_killCount);

        if (NetworkManager.Instance.GetAlive() == 1)
        {
            UIManager.Instance._watchingButton.SetActive(false);
            UIManager.Instance._watchingView.SetActive(false);
            UIManager.Instance._leaveButton.SetActive(true);
        }

        PlayEnding();
    }

    #endregion

    #region PRIVATE

    private void PlayEnding()
    {
        float phase = 0;
        float startTime = 1f;
        float delay = 3f;
        GameObject obj;

        obj = UIManager.Instance._endingView;
        StartCoroutine(SetActiveObj(obj, true, startTime));

        while (phase < (byte)Step.FINISH)
        {
            // 순차적으로 UI 활성화
            if (phase == (byte)Step.FIRST)
            {
                obj = UIManager.Instance._endingTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager.Instance._leaveButton;
                //StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager.Instance._watchingButton;
                StartCoroutine(SetActiveObj(obj, !NetworkManager.Instance.IsEnd(), delay + phase));
            }
            else if (phase == (byte)Step.SECOND)
            {
                obj = UIManager.Instance._myRankingTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));
            }
            else if (phase == (byte)Step.THIRD)
            {
                obj = UIManager.Instance._ranking.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager.Instance._killCount.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));
            }
            else
            {
                obj = UIManager.Instance._rankingTxt.gameObject;
                StartCoroutine(SetActiveObj(obj, true, delay + phase));

                obj = UIManager.Instance._killCountTxt.gameObject;
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

    #endregion
}
