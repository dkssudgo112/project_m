using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MagnetActivator : MonoBehaviourPunCallbacks
{
    private const int numOfAllPhaze = 6;
    private const float magDamage1 = 1.0f;
    private const float magDamage2 = 3.0f;
    private const float magDamage3 = 5.0f;
    private const float magWorldRadius = 49.325f;

    private static class LocalSizeMag
    {
        public const float ONE = 10.0f;
        public const float TWO = 5.0f;
        public const float THREE = 2.0f;
        public const float FOUR = 1.0f;
        public const float FIVE = 0.5f;
        public const float SIX = 0.25f;
    }

    private static class WorldSizeMag
    {
        public const float ONE = 300.0f;
        public const float TWO = 150.0f;
        public const float THREE = 60.0f;
        public const float FOUR = 30.0f;
        public const float FIVE = 15.0f;
        public const float SIX = 7.5f;
        public const float SEVEN = 3.75f;

    }

    private static class WorldSizeMagNoti
    {
        public const float ONE = 5.0f;
        public const float TWO = 2.0f;
        public const float THREE = 1f;
        public const float FOUR = 0.5f;
        public const float FIVE = 0.25f;
        
    }

    public Transform _magnetIn;
    public Transform _magnetNoti;
    public float[] _localSizeMagnet = new float[numOfAllPhaze] { LocalSizeMag.ONE, LocalSizeMag.TWO, LocalSizeMag.THREE, LocalSizeMag.FOUR, LocalSizeMag.FIVE, LocalSizeMag.SIX };
    public float[] _worldSizeMagnet = new float[7] { WorldSizeMag.ONE, WorldSizeMag.TWO, WorldSizeMag.THREE, WorldSizeMag.FOUR, WorldSizeMag.FIVE, WorldSizeMag.SIX, WorldSizeMag.SEVEN};
    public float[] _worldSizeMagnetNoti4 = new float[5] { WorldSizeMagNoti.ONE, WorldSizeMagNoti.TWO, WorldSizeMagNoti.THREE, WorldSizeMagNoti.FOUR, WorldSizeMagNoti.FIVE};
    public Vector3[] _randomCenterPoints = new Vector3[numOfAllPhaze];
    public bool _isStart;
    public PhotonView _photonView;
    public float _seed;

    private static int _magnetPhase;
    private float[] _pointsDistance = new float[numOfAllPhaze - 1];
    private float _damage;
    private float _radiusOutMag;
    private float _radiusInMag;
    private float _tickSizeMagnet = 0;


    private void Start()
    {
        _randomCenterPoints[0] = _magnetIn.position;
        if (PhotonNetwork.IsMasterClient)
        {
            _seed = Random.Range(1f, 1000f);
            _photonView.RPC("RPC_SetSeed", RpcTarget.AllBufferedViaServer, _seed);;
        }
    }

    private void Update()
    {
        _magnetPhase = NetworkManager._currentPhase - 1;

        if (NetworkManager._currentPhase == (int)Phase.WAIT)
        {

        }
        else if (NetworkManager._currentPhase == (int)Phase.FARM1)
        {
            SetMagNoti(NetworkManager._currentPhase);
        }
        else if (NetworkManager._currentPhase == (int)Phase.DECRESE1)
        {
            _damage = magDamage1;
            UpdateMagnet();
        }
        else if (NetworkManager._currentPhase == (int)Phase.FARM2)
        {
            SetMagNoti(NetworkManager._currentPhase);
        }
        else if (NetworkManager._currentPhase == (int)Phase.DECRESE2)
        {
            
            UpdateMagnet();
        }
        else if (NetworkManager._currentPhase == (int)Phase.FARM3)
        {
            _damage = magDamage2;
            SetMagNoti(NetworkManager._currentPhase);
        }
        else if (NetworkManager._currentPhase == (int)Phase.DECRESE3)
        {
            
            UpdateMagnet();
        }
        else if (NetworkManager._currentPhase == (int)Phase.FARM4)
        {
            SetMagNoti(NetworkManager._currentPhase);
        }
        else if (NetworkManager._currentPhase == (int)Phase.DECRESE4)
        {
            UpdateMagnet();
        }
        else if (NetworkManager._currentPhase == (int)Phase.FARM5)
        {
            Debug.Log(NetworkManager._currentPhase);
            SetMagNoti(NetworkManager._currentPhase);
        }
        else if (NetworkManager._currentPhase == (int)Phase.DECRESE5)
        {
            _damage = magDamage3;
            UpdateMagnet();
        }
    }

    [PunRPC]
    void RPC_SetSeed(float s) // Target - all
    {
        _seed = s;
        Init();
    }

    private void SetMagNoti(int phase)
    {
        
        _magnetNoti.localScale = new Vector3(_worldSizeMagnetNoti4[(phase / 2)], _worldSizeMagnetNoti4[(phase / 2) ], 1);
        _magnetNoti.transform.position = _randomCenterPoints[phase / 2 + 1];
       
    }

    private void UpdateMagnet()
    {
        _magnetIn.position = Vector3.MoveTowards(_magnetIn.position, _randomCenterPoints[_magnetPhase / 2 + 1], Time.deltaTime * _pointsDistance[_magnetPhase / 2] / ConstNums.PhaseTime[NetworkManager._currentPhase]);
        _tickSizeMagnet = Time.deltaTime * (_localSizeMagnet[_magnetPhase / 2] - _localSizeMagnet[_magnetPhase / 2 + 1]) / ConstNums.PhaseTime[NetworkManager._currentPhase];
        _magnetIn.localScale -= new Vector3(_tickSizeMagnet, _tickSizeMagnet, _tickSizeMagnet);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IDamageable target = null;
        OwnerInfo magnetInfo = new OwnerInfo(true);

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) //Player
        {
            if (Vector3.Distance(collision.gameObject.transform.position, _magnetIn.position) > _magnetIn.localScale.x * magWorldRadius)
            {
                
                if (collision.gameObject.TryGetComponent(out target))
                {
                    target.TakeDamage(magnetInfo.GetObjects(), _damage, 0, (Vector2)new Vector2(0, 0), false);
                }
                else
                {
                    Debug.Log("There is no target in Mag");
                }
            }

        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("AI")) //AI
        {
            if (Vector3.Distance(collision.gameObject.transform.position, _magnetIn.position) > _magnetIn.localScale.x * magWorldRadius)
            {
                
                if (collision.gameObject.TryGetComponent(out target))
                {
                    target.TakeDamage(magnetInfo.GetObjects(), _damage, 0, (Vector2)new Vector2(0, 0), false);
                }
                else
                {
                    Debug.Log("There is no target in Mag");
                }
            }

        }
    }

    public static void MoveToNextPhaze(int phase)
    {
        _magnetPhase = phase;
    }

    void StartMagnet()
    {
        _photonView.RPC("RPC_StartMag", RpcTarget.AllBufferedViaServer);
    }

    private void Init()
    {
        Random.InitState((int)_seed);

        for (int i = 1; i < numOfAllPhaze; i++)
        {
            _radiusOutMag = _worldSizeMagnet[i - 1];
            _radiusInMag = _worldSizeMagnet[i];
            float magnetRange = (_radiusOutMag - _radiusInMag) / Mathf.Sqrt(2);
            _randomCenterPoints[i] = new Vector3(Random.Range(-1 * magnetRange, magnetRange), Random.Range(-1 * magnetRange, magnetRange), 0.0f);
            _randomCenterPoints[i] += _randomCenterPoints[i - 1];
            _pointsDistance[i - 1] = Vector3.Distance(_randomCenterPoints[i - 1], _randomCenterPoints[i]);
        }
    }
}
