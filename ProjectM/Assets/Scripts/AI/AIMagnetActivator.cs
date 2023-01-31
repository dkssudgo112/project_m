using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMagnetActivator : MonoBehaviour
{
    private const int numOfAllPhaze = 6;
    private const float magDamage = 1.0f;
    private const float magWorldRadius = 49.34f;
    private const float startMagTime = 20.0f;
    private const float limitDistanceOfCenter = 0.1f;

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
    }

    private static class MagPhazeTime
    {
        public const float ONE = 40.0f;
        public const float TWO = 20.0f;
        public const float THREE = 10.0f;
        public const float FOUR = 10.0f;
        public const float FIVE = 5.0f;
    }

    enum MagnetState
    {
        FARM1,
        DECRESE1,
        FARM2,
        DECRESE2,
        FARM3,
        DECRESE3,
        FARM4,
        DECRESE4,
        FARM5,
        DECRESE5
    };

    enum OnceDoInvokeSet
    {
        ZERO,
        ONE,
        TWO,
        THREE,
        FOUR
    };

    public Transform _magnetIn;
    public float[] _localSizeMagnet = new float[numOfAllPhaze] { LocalSizeMag.ONE, LocalSizeMag.TWO, LocalSizeMag.THREE, LocalSizeMag.FOUR, LocalSizeMag.FIVE, LocalSizeMag.SIX };
    public float[] _worldSizeMagnet = new float[numOfAllPhaze] { WorldSizeMag.ONE, WorldSizeMag.TWO, WorldSizeMag.THREE, WorldSizeMag.FOUR, WorldSizeMag.FIVE, WorldSizeMag.SIX };
    public float[] _magnetPhazeTime = new float[numOfAllPhaze - 1] { MagPhazeTime.ONE, MagPhazeTime.TWO, MagPhazeTime.THREE, MagPhazeTime.FOUR, MagPhazeTime.FIVE };
    public Vector3[] _randomCenterPoints = new Vector3[numOfAllPhaze];
    public bool _isStart;
    public float _seed;

    private int _magnetPhaze;
    private OnceDoInvokeSet _onceDoinvoke;
    private float[] _pointsDistance = new float[numOfAllPhaze - 1];
    private float _damage;
    private float _radiusOutMag;
    private float _radiusInMag;
    private float _tickSizeMagnet = 0;

    private void Start()
    {
        _randomCenterPoints[0] = _magnetIn.position;
        //setMagCenter();
        //StartMagnet();
    }

    private void Update()
    {
        if (_magnetPhaze == (int)MagnetState.FARM1 && _isStart)
        {
            if (_onceDoinvoke == OnceDoInvokeSet.ZERO)
            {
                MoveToNextPhaze();
                _onceDoinvoke++;
            }
        }
        else if (_magnetPhaze == (int)MagnetState.DECRESE1 && _magnetIn.localScale.x > _localSizeMagnet[_magnetPhaze / 2 + 1])
        {
            UpdateMagnet();
        }
        else if (_magnetPhaze == (int)MagnetState.FARM2)
        {
            if (_onceDoinvoke == OnceDoInvokeSet.ONE)
            {
                MoveToNextPhaze();
                _onceDoinvoke++;
            }
        }
        else if (_magnetPhaze == (int)MagnetState.DECRESE2 && _magnetIn.localScale.x > _localSizeMagnet[_magnetPhaze / 2 + 1])
        {
            UpdateMagnet();
        }
        else if (_magnetPhaze == (int)MagnetState.FARM3)
        {
            if (_onceDoinvoke == OnceDoInvokeSet.TWO)
            {
                MoveToNextPhaze();
                _onceDoinvoke++;
            }
        }
        else if (_magnetPhaze == (int)MagnetState.DECRESE3 && _magnetIn.localScale.x > _localSizeMagnet[_magnetPhaze / 2 + 1])
        {
            UpdateMagnet();
        }
        else if (_magnetPhaze == (int)MagnetState.FARM4)
        {
            if (_onceDoinvoke == OnceDoInvokeSet.THREE)
            {
                MoveToNextPhaze();
                _onceDoinvoke++;
            }
        }
        else if (_magnetPhaze == (int)MagnetState.DECRESE4 && _magnetIn.localScale.x > _localSizeMagnet[_magnetPhaze / 2 + 1])
        {
            UpdateMagnet();
        }
        else if (_magnetPhaze == (int)MagnetState.FARM5)
        {
            if (_onceDoinvoke == OnceDoInvokeSet.FOUR)
            {
                MoveToNextPhaze();
                _onceDoinvoke++;
            }
        }
        else if (_magnetPhaze == (int)MagnetState.DECRESE5 && _magnetIn.localScale.x > _localSizeMagnet[_magnetPhaze / 2 + 1])
        {
            UpdateMagnet();
        }
    }

    private void UpdateMagnet()
    {
        _magnetIn.position = Vector3.MoveTowards(_magnetIn.position, _randomCenterPoints[_magnetPhaze / 2 + 1], Time.deltaTime * _pointsDistance[_magnetPhaze / 2] / _magnetPhazeTime[_magnetPhaze / 2]);
        _tickSizeMagnet = Time.deltaTime * (_localSizeMagnet[_magnetPhaze / 2] - _localSizeMagnet[_magnetPhaze / 2 + 1]) / _magnetPhazeTime[_magnetPhaze / 2];
        _magnetIn.localScale -= new Vector3(_tickSizeMagnet, _tickSizeMagnet, _tickSizeMagnet);

        if (Vector2.Distance(_magnetIn.position, _randomCenterPoints[_magnetPhaze / 2 + 1]) < limitDistanceOfCenter && _magnetIn.localScale.x < _localSizeMagnet[_magnetPhaze / 2 + 1])
        {
            _magnetPhaze++;
        }
    }

    public void StartMagnet()
    {
        _magnetIn.localPosition = new Vector3(0,0, -2);
        _magnetIn.localScale = new Vector3(10,10,10);
        _isStart = true;
        _magnetPhaze = 0;
        _onceDoinvoke = 0;
    }

    void MoveToNextPhaze()
    {
        _magnetPhaze++;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        IDamageable target = null;
        OwnerInfo magnetInfo = new OwnerInfo(true);

        if (collision.gameObject.layer == 12) //AI
        {
            if (Vector3.Distance(collision.gameObject.transform.position, _magnetIn.position) > _magnetIn.localScale.x * 49.34f)
            {
                _damage = magDamage;
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

    public void setMagCenter()
    {
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
