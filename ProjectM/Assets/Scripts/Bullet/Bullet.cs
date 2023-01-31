using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviour
{
    private const float basicSizeBullet = 0.2f;
    private const float sizeMaxBullet = 1.5f;
    private const float sizeUpTickSpeed = 2.0f;
    private const float colliderDisactRange = 0.75f;
    private const float basicColliRadiusBullet = 1.0f;

    public float _bulletSpeed = 0;

    private object[] _ownerInfo;
    private Vector2 _dir = new Vector2();
    private Vector2 _pos = new Vector2();
    private float _damageToPlayer = 0;
    private float _damageToObject = 0;
    private float _attackRange = 0;
    private float _fallOff = 0;
    private bool _isBomb = false;
    private bool _isStart = false;

    CircleCollider2D _circleColliderBullet = null;
    TrailRenderer _trailBullet = null;

    private void Start()
    {
        _circleColliderBullet = GetComponent<CircleCollider2D>();
        _trailBullet = GetComponent<TrailRenderer>();
        _isStart = true;
    }

    private void Update()
    {
        if (Vector2.Distance(_dir + _pos, transform.position) > _attackRange)
        {
            transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);
            
            gameObject.SetActive(false);
        }

        transform.Translate(_dir * _bulletSpeed * Time.deltaTime, Space.World);
    }

    private void OnDisable()
    {
        ObjectPoolManager.FreeObjectToPool(gameObject);
        CancelInvoke();
    }

    private void OnEnable()
    {
        if (_isStart == true)
        {
            _trailBullet.Clear();
        }
    }

    public void SetData(object[] ownerInfo, Vector2 dir, Vector2 pos, float damageToPlayer, float damageToObject, float attackRange, float fallOff, bool isBomb)
    {
        this._ownerInfo = ownerInfo;
        this._dir = dir;
        this._pos = pos;
        this._damageToPlayer = damageToPlayer;
        this._damageToObject = damageToObject;
        this._attackRange = attackRange;
        this._fallOff = fallOff;
        this._isBomb = isBomb;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            int id = (int)_ownerInfo[(int)InfoIdx.ID];

            if ((collision.CompareTag("Player") && (id == collision.gameObject.GetPhotonView().Owner.ActorNumber))
                || (collision.CompareTag("AI") && (id == collision.gameObject.GetPhotonView().ViewID)))
            {
                return;
            }

            IDamageable target = null;

            if ( collision.TryGetComponent(out target))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    target.TakeDamage(_ownerInfo, _damageToPlayer, _damageToObject, _dir, true);
                }
                else
                {
                    target.TakeDamage(_ownerInfo, _damageToPlayer, _damageToObject, _dir, false);
                }

                transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);
                if (_circleColliderBullet != null)
                {
                    _circleColliderBullet.radius = basicColliRadiusBullet;
                }
                else
                {
                    Debug.Log("in bullet");
                }
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("There is no target in bullet");
            }
        }
        else
        {
            Debug.Log("collision null in bullet");
        }
        
    }
}
