using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    private const float basicSizeBullet = 0.2f;
    private const float sizeMaxBullet = 1.3f;
    private const float sizeUpTickSpeed = 0.4f;
    private const float basicColliRadiusBullet = 0.5f;
    private const float sizeUpColliRadiusBullet = 20.0f;
    private const float colliderActRange = 0.25f;
    private const float colliderDisactRange = 0.75f;
    private const float destoryTime = 1.0f;


    public float _bulletSpeed;

    private object[] _ownerInfo;
    private Vector2 _dir = new Vector2();
    private Vector2 _pos = new Vector2();
    private float _damageToPlayer = 0;
    private float _damageToObject = 0;
    private float _attackRange = 0;
    private float _destoryTickTime = 0;
    private bool _isMove = true;
    private bool _startBombTimer = false;


    CircleCollider2D _circleCollider = null;

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _isMove = true;
    }

    private void Update()
    {
        if (_isMove == true)
        {
            transform.Translate(_dir * _bulletSpeed * Time.deltaTime, Space.World);
        }

        if (Vector2.Distance(_dir + _pos, transform.position) > _attackRange * colliderActRange)
        {
            _circleCollider.radius = sizeUpColliRadiusBullet;
        }
        RefrestSizeBomb();

        if (Vector2.Distance(_pos, transform.position) > _attackRange)
        {
            _isMove = false;
            _startBombTimer = true;
        }

        if (_startBombTimer == true && _destoryTickTime < destoryTime)
        {
            _destoryTickTime += Time.deltaTime;

        }
        else if (_startBombTimer == true && _destoryTickTime >= destoryTime)
        {
            ObjectPoolManager.AllocObject("Explosion", transform.position);
            _circleCollider.enabled = true;

            Invoke("DestoryThis", 0.5f);
            _destoryTickTime = -99;
        }

    }

    private void DestoryThis()
    {
        _startBombTimer = false;
        _destoryTickTime = 0.0f;
        _circleCollider.radius = basicColliRadiusBullet;

        transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);

        _circleCollider.enabled = false;
        _isMove = true;
        
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
    
        ObjectPoolManager.FreeObjectToPool(gameObject);
        CancelInvoke();
    }

    public void SetData(object[] ownerInfo, Vector2 dir, Vector2 pos, float damageToPlayer, float damageToObject, float attackRange)
    {
        this._ownerInfo = ownerInfo;
        this._dir = dir;
        if(dir.magnitude == 0)
        {
            this._dir = new Vector3(1,1, 0);
        }
        this._pos = pos;
        this._damageToPlayer = damageToPlayer;
        this._damageToObject = damageToObject;
        this._attackRange = attackRange;
    }

    public void RefrestSizeBomb()
    {
        if (Vector2.Distance(_pos, transform.position) < _attackRange / 2.0f)
        {
            if (transform.localScale.x < sizeMaxBullet)
            {
                transform.localScale += new Vector3(Time.deltaTime * sizeUpTickSpeed, Time.deltaTime * sizeUpTickSpeed, 0);
            }
            else
            {
                Debug.Log("Too mush size up");
            }
        }
        else
        {
            if (transform.localScale.x > basicSizeBullet)
            {
                transform.localScale -= new Vector3(Time.deltaTime * sizeUpTickSpeed, Time.deltaTime * sizeUpTickSpeed, 0);
            }
            else
            {
                transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);

            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            IDamageable target = null;

            if (collision.TryGetComponent(out target))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    target.TakeDamage(_ownerInfo, _damageToPlayer, _damageToObject, _dir, true);
                }
                else
                {
                    target.TakeDamage(_ownerInfo, _damageToPlayer, _damageToObject, _dir, false);
                }
            }
            else
            {
                Debug.Log("There is no target in bullet");
            }

            ObjectPoolManager.AllocObject("Explosion", transform.position);
        }
        else
        {
            Debug.Log("collision null in bullet");
        }

        transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);
        _circleCollider.radius = basicColliRadiusBullet;
        _circleCollider.enabled = false;
        gameObject.SetActive(false);
    }
}
